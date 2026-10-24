using System;
using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class Expense
    {
        public int ExpenseId { get; set; }

        public int UserId { get; set; }

        // User navigation info (useful for Admin logs)
        public string UserFullName { get; set; }
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        // Category navigation property for views
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public string CategoryIcon { get; set; }

        [Required(ErrorMessage = "Expense title/description is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 10000000.00, ErrorMessage = "Amount must be greater than zero.")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Expense date is required.")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpenseDate { get; set; }

        [Required(ErrorMessage = "Payment method is required.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
