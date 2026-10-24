using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class Budget
    {
        public int BudgetId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Budget Limit is required.")]
        [Range(1, 10000000.00, ErrorMessage = "Budget limit must be at least 1.")]
        [Display(Name = "Monthly Limit")]
        public decimal BudgetLimit { get; set; }

        [Required]
        [Display(Name = "Month")]
        public int BudgetMonth { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int BudgetYear { get; set; }

        // Computed for analytics
        public decimal TotalSpent { get; set; }
        public decimal Remaining => BudgetLimit - TotalSpent;
        public double PercentageUsed => BudgetLimit > 0 ? (double)((TotalSpent / BudgetLimit) * 100) : 0;
        public bool IsExceeded => TotalSpent > BudgetLimit;
    }
}
