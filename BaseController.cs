using System;
using System.Web;
using System.Web.Mvc;
using Expense_Tracker.Models;

namespace Expense_Tracker.Controllers
{
    /// <summary>
    /// Base Controller demonstrating ASP.NET Session Management, Role-Based Access Control, Custom Action Filters, and Flash Messages (TempData)
    /// </summary>
    public abstract class BaseController : Controller
    {
        public const string SessionUserKey = "CurrentUser";

        public User CurrentUser
        {
            get
            {
                return Session[SessionUserKey] as User;
            }
            set
            {
                Session[SessionUserKey] = value;
            }
        }

        public bool IsAuthenticated => CurrentUser != null;
        public bool IsAdmin => CurrentUser != null && CurrentUser.IsAdmin;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Populate ViewBag for layouts and navigation
            if (CurrentUser != null)
            {
                ViewBag.CurrentUser = CurrentUser;
                ViewBag.IsAdmin = CurrentUser.IsAdmin;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
        }

        protected void SetSuccessNotification(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorNotification(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetWarningNotification(string message)
        {
            TempData["WarningMessage"] = message;
        }
    }

    /// <summary>
    /// Action filter attribute to guard controller actions requiring logged in user
    /// Demonstrates ASP.NET MVC Action Filters / Authentication
    /// </summary>
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            if (session == null || session[BaseController.SessionUserKey] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "returnUrl", filterContext.HttpContext.Request.RawUrl }
                    });
            }
            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// Action filter attribute enforcing strict Admin authorization (Role-Based Access Control - RBAC)
    /// Demonstrates ASP.NET Authorization Filters
    /// </summary>
    public class AuthorizeAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            User user = session?[BaseController.SessionUserKey] as User;

            if (user == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Account" },
                        { "action", "Login" },
                        { "returnUrl", filterContext.HttpContext.Request.RawUrl }
                    });
            }
            else if (!user.IsAdmin)
            {
                filterContext.Controller.TempData["ErrorMessage"] = "Access Denied: You do not possess Administrator privileges.";
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Dashboard" },
                        { "action", "Index" }
                    });
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
