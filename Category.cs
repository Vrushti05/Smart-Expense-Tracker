using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(50)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Required]
        [Display(Name = "Type")]
        public string CategoryType { get; set; } // "Expense" or "Income"

        [Display(Name = "Badge Color")]
        public string Color { get; set; }

        [Display(Name = "Icon")]
        public string Icon { get; set; }
    }
}
