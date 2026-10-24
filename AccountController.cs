using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserRepository _userRepo = new UserRepository();

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (IsAuthenticated)
            {
                if (CurrentUser.IsAdmin)
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                User user = _userRepo.Authenticate(model.Email, model.Password);
                if (user != null)
                {
                    CurrentUser = user;
                    SetSuccessNotification($"Welcome back, {user.FullName}! Logged in as [{user.Role}].");

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    if (user.IsAdmin)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email address or password.");
                }
            }
            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            if (IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User model)
        {
            if (ModelState.IsValid)
            {
                if (_userRepo.EmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "An account with this email address already exists.");
                    return View(model);
                }

                // Default registration is Client
                if (string.IsNullOrEmpty(model.Role))
                {
                    model.Role = "Client";
                }

                int newUserId = _userRepo.Register(model);
                if (newUserId > 0)
                {
                    model.UserId = newUserId;
                    CurrentUser = model;
                    SetSuccessNotification("Account registered successfully! Welcome to Smart Expense Tracker.");
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to register user. Please try again.");
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            SetSuccessNotification("You have been logged out successfully.");
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/Profile
        [AuthorizeUser]
        [HttpGet]
        public ActionResult UserProfile()
        {
            User user = _userRepo.GetById(CurrentUser.UserId);
            return View(user);
        }

        // POST: Account/UpdateIncome
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateIncome(decimal monthlyIncome)
        {
            if (monthlyIncome < 0)
            {
                SetErrorNotification("Income cannot be negative.");
                return RedirectToAction("UserProfile");
            }

            if (_userRepo.UpdateIncome(CurrentUser.UserId, monthlyIncome))
            {
                CurrentUser.MonthlyIncome = monthlyIncome;
                SetSuccessNotification("Monthly income updated successfully!");
            }
            else
            {
                SetErrorNotification("Unable to update income.");
            }

            return RedirectToAction("UserProfile");
        }
    }
}
