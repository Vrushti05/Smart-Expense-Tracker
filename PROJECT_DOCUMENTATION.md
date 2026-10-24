# Smart Expense Tracker - Architecture & RBAC Guide

A complete, fully functional, and production-tested **Smart Expense Tracker** built using **ASP.NET MVC 5 (.NET Framework 4.7.2)**, **C#**, and **Microsoft SQL Server / LocalDB** with pure **ADO.NET**.

---

## 👥 Dual Portal Architecture (Client & Admin)

The system features complete **Role-Based Access Control (RBAC)** separating the application into two dedicated experiences:

### 1. Client Side (Personal Financial Dashboard)
- **Role**: `Client` (Default for newly registered users)
- **Access URL**: `/Dashboard`, `/Expense`, `/Budget`, `/Category`, `/Account/UserProfile`
- **Features**:
  - Personal Financial KPI Cards (Declared Income, Spend, Net Savings, Budget Limits).
  - Smart Financial Insights (Deficit spending warnings, savings rate benchmarks, 85% & 100% budget limit warnings).
  - Add, edit, delete, and view personal transactions with Category tags, dates, notes, and payment methods.
  - Multi-parameter filtering and instant **CSV export**.
  - Monthly budget allocation per category with live visual progress meters.

### 2. Admin Side (Platform Control Center)
- **Role**: `Admin` (Secured with `[AuthorizeAdmin]` filter attribute)
- **Access URL**: `/Admin/Index`, `/Admin/Users`, `/Admin/AllExpenses`
- **Features**:
  - **System Overview Dashboard**: Total registered users, total system financial volume, current month throughput, active categories, and aggregate distribution charts.
  - **User Accounts Manager**: View all client accounts, see each user's declared income, total number of logged transactions, and total money spent. Allows toggling roles (`Client` <-> `Admin`) or deleting users.
  - **Global Transaction Audit**: Full oversight across all user expenditures with cross-user filtering, category analysis, and administrative record moderation.
  - **Security Guard**: If a Client attempts to navigate directly to `/Admin`, the `[AuthorizeAdmin]` action filter automatically intercepts the request, blocks it, and redirects back to the client dashboard with an `Access Denied` flash alert.

---

## 🔑 Pre-Seeded Login Credentials

| Role | Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Client** | `john@example.com` | `demo123` | Personal Dashboard, Personal Expenses, Personal Budgets |
| **Admin** | `admin@smarttracker.com` | `admin123` | System Control Center, All Users Manager, Global Audit Logs |

*(The login screen also provides 1-click **Auto-fill buttons** for both Client and Admin demo credentials).*

---

## 🏛️ ASP.NET & .NET Subject Concepts Applied

| Concept | Implementation in this Project |
| :--- | :--- |
| **Role-Based Access Control (RBAC)** | Custom `[AuthorizeAdmin]` and `[AuthorizeUser]` action filters inheriting from `ActionFilterAttribute`. |
| **MVC Architecture Pattern** | Separation of Models (strongly typed entities + ViewModels), Razor Views (`.cshtml`), and C# Controllers. |
| **ADO.NET Data Access** | Handled via [DbHelper.cs](file:///c:/Users/HP/OneDrive/Desktop/VS.NET/Expense_Tracker/Expense_Tracker/Models/DbHelper.cs) using `SqlConnection`, `SqlCommand`, `SqlDataAdapter`, `DataTable`, and `SqlParameter`. |
| **Resource Management** | Full adherence to C# `using (...)` pattern guaranteeing deterministic disposal (`IDisposable`) and connection pooling. |
| **Security & SQL Injection Prevention** | 100% Parameterized queries across all repositories; Anti-Forgery tokens (`@Html.AntiForgeryToken()`) on all POST submissions. |
| **State Management** | Server-side `Session["CurrentUser"]`, `TempData` flash notifications, and `ViewBag` for UI metadata. |
| **Data Annotations & Model Validation** | `[Required]`, `[Range]`, `[StringLength]`, `[DataType]`, and `ModelState.IsValid`. |
| **Dynamic Navigation** | Context-sensitive modern navigation bar adapting in real-time based on whether the logged-in user is a Client or an Admin. |

---

## 🚀 How to Run in Visual Studio

1. **Open Solution**:
   - Open **Visual Studio 2022**.
   - Open [`Expense_Tracker.slnx`](file:///c:/Users/HP/OneDrive/Desktop/VS.NET/Expense_Tracker/Expense_Tracker.slnx) or [`Expense_Tracker.csproj`](file:///c:/Users/HP/OneDrive/Desktop/VS.NET/Expense_Tracker/Expense_Tracker/Expense_Tracker.csproj).
2. **Press F5** (or click **IIS Express**):
   - The project builds cleanly with `0 Errors` and `0 Warnings`.
3. **Login**:
   - Log in as **Client** (`john@example.com` / `demo123`) or **Admin** (`admin@smarttracker.com` / `admin123`).
