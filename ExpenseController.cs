using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    [AuthorizeUser]
    public class ExpenseController : BaseController
    {
        private readonly ExpenseRepository _expenseRepo = new ExpenseRepository();
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        // GET: Expense
        public ActionResult Index(int? categoryId, string paymentMethod, DateTime? startDate, DateTime? endDate, string search)
        {
            int userId = CurrentUser.UserId;
            List<Expense> expenses = _expenseRepo.GetExpenses(userId, categoryId, paymentMethod, startDate, endDate, search);

            ViewBag.Categories = new SelectList(_categoryRepo.GetAll("Expense"), "CategoryId", "CategoryName", categoryId);
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedPaymentMethod = paymentMethod;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchTerm = search;
            ViewBag.TotalAmount = expenses.Sum(e => e.Amount);

            return View(expenses);
        }

        // GET: Expense/Create
        [HttpGet]
        public ActionResult Create()
        {
            PopulateCategoriesDropdown();
            Expense model = new Expense
            {
                ExpenseDate = DateTime.Today,
                PaymentMethod = "Credit Card"
            };
            return View(model);
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Expense model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = CurrentUser.UserId;
                int id = _expenseRepo.Add(model);
                if (id > 0)
                {
                    SetSuccessNotification("Expense recorded successfully!");
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save expense. Please try again.");
                }
            }

            PopulateCategoriesDropdown(model.CategoryId);
            return View(model);
        }

        // GET: Expense/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Expense expense = _expenseRepo.GetById(id, CurrentUser.UserId);
            if (expense == null)
            {
                SetErrorNotification("Expense not found.");
                return RedirectToAction("Index");
            }

            PopulateCategoriesDropdown(expense.CategoryId);
            return View(expense);
        }

        // POST: Expense/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Expense model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = CurrentUser.UserId;
                if (_expenseRepo.Update(model))
                {
                    SetSuccessNotification("Expense updated successfully!");
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to update expense.");
                }
            }

            PopulateCategoriesDropdown(model.CategoryId);
            return View(model);
        }

        // GET: Expense/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            Expense expense = _expenseRepo.GetById(id, CurrentUser.UserId);
            if (expense == null)
            {
                SetErrorNotification("Expense record not found.");
                return RedirectToAction("Index");
            }

            return View(expense);
        }

        // POST: Expense/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_expenseRepo.Delete(id, CurrentUser.UserId))
            {
                SetSuccessNotification("Expense deleted successfully.");
            }
            else
            {
                SetErrorNotification("Could not delete the selected expense.");
            }
            return RedirectToAction("Index");
        }

        // GET: Expense/ExportCsv
        public ActionResult ExportCsv(int? categoryId, string paymentMethod, DateTime? startDate, DateTime? endDate, string search)
        {
            List<Expense> list = _expenseRepo.GetExpenses(CurrentUser.UserId, categoryId, paymentMethod, startDate, endDate, search);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ExpenseId,Date,Title,Category,PaymentMethod,Amount,Notes");

            foreach (var item in list)
            {
                string safeTitle = "\"" + (item.Title ?? "").Replace("\"", "\"\"") + "\"";
                string safeCategory = "\"" + (item.CategoryName ?? "").Replace("\"", "\"\"") + "\"";
                string safeMethod = "\"" + (item.PaymentMethod ?? "").Replace("\"", "\"\"") + "\"";
                string safeNotes = "\"" + (item.Notes ?? "").Replace("\"", "\"\"") + "\"";

                sb.AppendLine($"{item.ExpenseId},{item.ExpenseDate:yyyy-MM-dd},{safeTitle},{safeCategory},{safeMethod},{item.Amount:F2},{safeNotes}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "text/csv", $"Expenses_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private void PopulateCategoriesDropdown(object selectedValue = null)
        {
            var categories = _categoryRepo.GetAll("Expense");
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", selectedValue);
        }
    }
}
