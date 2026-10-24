using System.Collections.Generic;

namespace Expense_Tracker.Models
{
    public class DashboardViewModel
    {
        public decimal MonthlyIncome { get; set; }
        public decimal TotalExpensesThisMonth { get; set; }
        public decimal TotalExpensesAllTime { get; set; }
        public decimal NetSavingsThisMonth { get; set; }
        public decimal TotalBudgetThisMonth { get; set; }

        public List<Expense> RecentExpenses { get; set; }
        public List<CategorySpending> CategorySpendings { get; set; }
        public List<Budget> BudgetStatuses { get; set; }
        public List<MonthlyTrend> MonthlyTrends { get; set; }
        public List<SmartInsight> Insights { get; set; }

        public DashboardViewModel()
        {
            RecentExpenses = new List<Expense>();
            CategorySpendings = new List<CategorySpending>();
            BudgetStatuses = new List<Budget>();
            MonthlyTrends = new List<MonthlyTrend>();
            Insights = new List<SmartInsight>();
        }
    }

    public class CategorySpending
    {
        public string CategoryName { get; set; }
        public string Color { get; set; }
        public decimal TotalAmount { get; set; }
        public double Percentage { get; set; }
    }

    public class MonthlyTrend
    {
        public string MonthLabel { get; set; } // e.g., "Jan 2026"
        public decimal Amount { get; set; }
    }

    public class SmartInsight
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string AlertType { get; set; } // "warning", "success", "info", "danger"
        public string Icon { get; set; }
    }
}
