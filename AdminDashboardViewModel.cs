using System.Collections.Generic;

namespace Expense_Tracker.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalRegisteredUsers { get; set; }
        public int TotalClientsCount { get; set; }
        public int TotalAdminsCount { get; set; }
        public int TotalSystemTransactions { get; set; }
        public decimal TotalSystemVolume { get; set; }
        public decimal TotalVolumeThisMonth { get; set; }
        public int TotalActiveCategories { get; set; }

        public List<User> UsersList { get; set; }
        public List<Expense> RecentSystemExpenses { get; set; }
        public List<CategorySpending> SystemCategoryDistribution { get; set; }
        public List<MonthlyTrend> SystemVolumeTrends { get; set; }

        public AdminDashboardViewModel()
        {
            UsersList = new List<User>();
            RecentSystemExpenses = new List<Expense>();
            SystemCategoryDistribution = new List<CategorySpending>();
            SystemVolumeTrends = new List<MonthlyTrend>();
        }
    }
}
