using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Smart Expense Tracker - Enterprise .NET Architecture Overview";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Support and Developer Resources.";
            return View();
        }
    }
}
