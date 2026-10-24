using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    [AuthorizeUser]
    public class DashboardController : BaseController
    {
        private readonly ExpenseRepository _expenseRepo = new ExpenseRepository();
        private readonly BudgetRepository _budgetRepo = new BudgetRepository();
        private readonly UserRepository _userRepo = new UserRepository();

        // GET: Dashboard
        public ActionResult Index()
        {
            int userId = CurrentUser.UserId;
            DateTime now = DateTime.Now;

            // Fetch fresh user data (for latest income)
            User freshUser = _userRepo.GetById(userId);
            if (freshUser != null)
            {
                CurrentUser.MonthlyIncome = freshUser.MonthlyIncome;
            }

            decimal monthlyIncome = CurrentUser.MonthlyIncome;
            decimal expensesThisMonth = _expenseRepo.GetTotalExpensesThisMonth(userId, now.Month, now.Year);
            decimal expensesAllTime = _expenseRepo.GetTotalExpensesAllTime(userId);
            decimal netSavings = monthlyIncome - expensesThisMonth;

            // Recent expenses (top 6)
            List<Expense> recentExpenses = _expenseRepo.GetExpenses(userId).Take(6).ToList();

            // Category breakdown for current month
            List<CategorySpending> breakdown = _expenseRepo.GetCategoryBreakdown(userId, now.Month, now.Year);

            // Budgets for current month
            List<Budget> budgets = _budgetRepo.GetBudgetsForMonth(userId, now.Month, now.Year);
            decimal totalBudget = budgets.Sum(b => b.BudgetLimit);

            // Last 6 months trend
            List<MonthlyTrend> trends = _expenseRepo.GetLast6MonthsTrend(userId);

            // Generate Smart Insights
            List<SmartInsight> insights = GenerateSmartInsights(monthlyIncome, expensesThisMonth, budgets, breakdown);

            DashboardViewModel vm = new DashboardViewModel
            {
                MonthlyIncome = monthlyIncome,
                TotalExpensesThisMonth = expensesThisMonth,
                TotalExpensesAllTime = expensesAllTime,
                NetSavingsThisMonth = netSavings,
                TotalBudgetThisMonth = totalBudget,
                RecentExpenses = recentExpenses,
                CategorySpendings = breakdown,
                BudgetStatuses = budgets,
                MonthlyTrends = trends,
                Insights = insights
            };

            return View(vm);
        }

        private List<SmartInsight> GenerateSmartInsights(decimal income, decimal spentThisMonth, 
                                                        List<Budget> budgets, List<CategorySpending> breakdown)
        {
            List<SmartInsight> insights = new List<SmartInsight>();

            // 1. Budget Alerts
            var exceededBudgets = budgets.Where(b => b.IsExceeded).ToList();
            if (exceededBudgets.Any())
            {
                insights.Add(new SmartInsight
                {
                    Title = "Budget Alert!",
                    Message = $"You have exceeded the budget in {exceededBudgets.Count} category(s): {string.Join(", ", exceededBudgets.Select(b => b.CategoryName))}.",
                    AlertType = "danger",
                    Icon = "bi-exclamation-triangle-fill"
                });
            }

            var nearLimitBudgets = budgets.Where(b => !b.IsExceeded && b.PercentageUsed >= 85).ToList();
            if (nearLimitBudgets.Any())
            {
                insights.Add(new SmartInsight
                {
                    Title = "Approaching Budget Limit",
                    Message = $"{string.Join(", ", nearLimitBudgets.Select(b => b.CategoryName))} is above 85% of monthly limit.",
                    AlertType = "warning",
                    Icon = "bi-exclamation-circle-fill"
                });
            }

            // 2. Savings Rate Insight
            if (income > 0)
            {
                decimal savingsRate = ((income - spentThisMonth) / income) * 100;
                if (savingsRate >= 30)
                {
                    insights.Add(new SmartInsight
                    {
                        Title = "Great Savings Rate!",
                        Message = $"You're currently saving {savingsRate:F1}% of your income this month. Keep it up!",
                        AlertType = "success",
                        Icon = "bi-piggy-bank-fill"
                    });
                }
                else if (savingsRate < 10 && savingsRate >= 0)
                {
                    insights.Add(new SmartInsight
                    {
                        Title = "Low Savings Rate",
                        Message = $"Your savings rate is only {savingsRate:F1}% this month. Consider cutting down discretionary expenses.",
                        AlertType = "warning",
                        Icon = "bi-shield-exclamation"
                    });
                }
                else if (savingsRate < 0)
                {
                    insights.Add(new SmartInsight
                    {
                        Title = "Deficit Warning",
                        Message = $"Your monthly spending exceeds your declared income by ₹{Math.Abs(income - spentThisMonth):N2}!",
                        AlertType = "danger",
                        Icon = "bi-arrow-down-circle-fill"
                    });
                }
            }

            // 3. Top Spending Category Insight
            if (breakdown.Any())
            {
                var topCategory = breakdown.First();
                if (topCategory.Percentage >= 40)
                {
                    insights.Add(new SmartInsight
                    {
                        Title = "High Concentration Spending",
                        Message = $"{topCategory.Percentage}% of your total expenses went into '{topCategory.CategoryName}'.",
                        AlertType = "info",
                        Icon = "bi-pie-chart-fill"
                    });
                }
            }

            if (!insights.Any())
            {
                insights.Add(new SmartInsight
                {
                    Title = "Finances in Good Shape",
                    Message = "Your expenditures are well within budget limits for this month.",
                    AlertType = "success",
                    Icon = "bi-check-circle-fill"
                });
            }

            return insights;
        }
    }
}
