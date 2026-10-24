using System.Collections.Generic;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    [AuthorizeUser]
    public class CategoryController : BaseController
    {
        private readonly CategoryRepository _categoryRepo = new CategoryRepository();

        // GET: Category
        public ActionResult Index()
        {
            List<Category> categories = _categoryRepo.GetAll();
            return View(categories);
        }

        // GET: Category/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Category { CategoryType = "Expense", Color = "#0d6efd", Icon = "bi-tag" });
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category model)
        {
            if (ModelState.IsValid)
            {
                int id = _categoryRepo.Add(model);
                if (id > 0)
                {
                    SetSuccessNotification($"Category '{model.CategoryName}' added successfully!");
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to create category.");
            }
            return View(model);
        }

        // GET: Category/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Category cat = _categoryRepo.GetById(id);
            if (cat == null)
            {
                SetErrorNotification("Category not found.");
                return RedirectToAction("Index");
            }
            return View(cat);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category model)
        {
            if (ModelState.IsValid)
            {
                if (_categoryRepo.Update(model))
                {
                    SetSuccessNotification($"Category '{model.CategoryName}' updated successfully!");
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Failed to update category.");
            }
            return View(model);
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (_categoryRepo.Delete(id))
            {
                SetSuccessNotification("Category deleted successfully.");
            }
            else
            {
                SetWarningNotification("Cannot delete category because it has expenses associated with it.");
            }
            return RedirectToAction("Index");
        }
    }
}
