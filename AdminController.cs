using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : BaseController
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly ExpenseRepository _expenseRepo = new ExpenseRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        // GET: Admin/Index
        public ActionResult Index()
        {
            DateTime now = DateTime.Now;
            List<User> allUsers = _userRepo.GetAllUsersWithMetrics();
            int totalTransactions = _expenseRepo.GetTotalTransactionsCount();
            decimal totalSystemVolume = _expenseRepo.GetTotalExpensesAllTime();
            decimal volumeThisMonth = _expenseRepo.GetTotalExpensesThisMonth(null, now.Month, now.Year);
            var categories = _categoryRepo.GetAll();

            // Recent system-wide transactions across all users (top 10)
            List<Expense> recentTransactions = _expenseRepo.GetExpenses().Take(10).ToList();

            // System-wide category distribution for current month
            List<CategorySpending> distribution = _expenseRepo.GetCategoryBreakdown(null, now.Month, now.Year);

            // System-wide 6-month transaction trends
            List<MonthlyTrend> trends = _expenseRepo.GetLast6MonthsTrend();

            AdminDashboardViewModel vm = new AdminDashboardViewModel
            {
                TotalRegisteredUsers = allUsers.Count,
                TotalClientsCount = allUsers.Count(u => u.Role == "Client"),
                TotalAdminsCount = allUsers.Count(u => u.Role == "Admin"),
                TotalSystemTransactions = totalTransactions,
                TotalSystemVolume = totalSystemVolume,
                TotalVolumeThisMonth = volumeThisMonth,
                TotalActiveCategories = categories.Count,
                UsersList = allUsers,
                RecentSystemExpenses = recentTransactions,
                SystemCategoryDistribution = distribution,
                SystemVolumeTrends = trends
            };

            return View(vm);
        }

        // GET: Admin/Users
        public ActionResult Users()
        {
            List<User> users = _userRepo.GetAllUsersWithMetrics();
            return View(users);
        }

        // POST: Admin/ChangeRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangeRole(int userId, string role)
        {
            if (userId == CurrentUser.UserId)
            {
                SetWarningNotification("You cannot modify your own administrator role.");
                return RedirectToAction("Users");
            }

            if (_userRepo.UpdateUserRole(userId, role))
            {
                SetSuccessNotification($"User role changed to [{role}] successfully.");
            }
            else
            {
                SetErrorNotification("Failed to update user role.");
            }

            return RedirectToAction("Users");
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int userId)
        {
            if (userId == CurrentUser.UserId)
            {
                SetErrorNotification("Security protection: You cannot delete your own active administrator account.");
                return RedirectToAction("Users");
            }

            if (_userRepo.DeleteUser(userId))
            {
                SetSuccessNotification("User account and all associated transactions removed successfully.");
            }
            else
            {
                SetErrorNotification("Failed to delete user account.");
            }

            return RedirectToAction("Users");
        }

        // GET: Admin/AllExpenses
        public ActionResult AllExpenses(int? userId, int? categoryId, string paymentMethod, DateTime? startDate, DateTime? endDate, string search)
        {
            List<Expense> expenses = _expenseRepo.GetExpenses(userId, categoryId, paymentMethod, startDate, endDate, search);

            ViewBag.Users = new SelectList(_userRepo.GetAllUsersWithMetrics(), "UserId", "FullName", userId);
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll("Expense"), "CategoryId", "CategoryName", categoryId);
            ViewBag.SelectedUser = userId;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedPaymentMethod = paymentMethod;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchTerm = search;
            ViewBag.TotalVolume = expenses.Sum(e => e.Amount);

            return View(expenses);
        }

        // POST: Admin/DeleteExpense/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExpense(int id)
        {
            if (_expenseRepo.Delete(id))
            {
                SetSuccessNotification("Transaction deleted from system records.");
            }
            else
            {
                SetErrorNotification("Failed to delete transaction.");
            }

            return RedirectToAction("AllExpenses");
        }
    }
}
