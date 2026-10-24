using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    [AuthorizeUser]
    public class BudgetController : BaseController
    {
        private readonly BudgetRepository _budgetRepo = new BudgetRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        // GET: Budget
        public ActionResult Index(int? month, int? year)
        {
            DateTime now = DateTime.Now;
            int selMonth = month ?? now.Month;
            int selYear = year ?? now.Year;

            List<Budget> budgets = _budgetRepo.GetBudgetsForMonth(CurrentUser.UserId, selMonth, selYear);

            ViewBag.SelectedMonth = selMonth;
            ViewBag.SelectedYear = selYear;
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll("Expense"), "CategoryId", "CategoryName");

            return View(budgets);
        }

        // POST: Budget/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(Budget model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = CurrentUser.UserId;
                if (_budgetRepo.SaveOrUpdate(model))
                {
                    SetSuccessNotification("Budget limit updated successfully!");
                }
                else
                {
                    SetErrorNotification("Failed to set budget.");
                }
            }
            else
            {
                SetErrorNotification("Please enter a valid budget limit and select category.");
            }

            return RedirectToAction("Index", new { month = model.BudgetMonth, year = model.BudgetYear });
        }

        // POST: Budget/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int month, int year)
        {
            if (_budgetRepo.Delete(id, CurrentUser.UserId))
            {
                SetSuccessNotification("Budget removed successfully.");
            }
            else
            {
                SetErrorNotification("Failed to remove budget.");
            }
            return RedirectToAction("Index", new { month = month, year = year });
        }
    }
}
