using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Expense_Tracker.Models
{
    public class ExpenseRepository
    {
        public List<Expense> GetExpenses(int? userId = null, int? categoryId = null, string paymentMethod = null, 
                                        DateTime? startDate = null, DateTime? endDate = null, string searchTerm = null)
        {
            List<Expense> list = new List<Expense>();
            List<string> conditions = new List<string>();
            List<SqlParameter> prms = new List<SqlParameter>();

            if (userId.HasValue && userId.Value > 0)
            {
                conditions.Add("e.UserId = @UserId");
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                conditions.Add("e.CategoryId = @CategoryId");
                prms.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId.Value });
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                conditions.Add("e.PaymentMethod = @PaymentMethod");
                prms.Add(new SqlParameter("@PaymentMethod", SqlDbType.NVarChar) { Value = paymentMethod });
            }

            if (startDate.HasValue)
            {
                conditions.Add("e.ExpenseDate >= @StartDate");
                prms.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate.Value });
            }

            if (endDate.HasValue)
            {
                conditions.Add("e.ExpenseDate <= @EndDate");
                prms.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate.Value });
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                conditions.Add("(e.Title LIKE @Search OR e.Notes LIKE @Search OR u.FullName LIKE @Search OR u.Email LIKE @Search)");
                prms.Add(new SqlParameter("@Search", SqlDbType.NVarChar) { Value = "%" + searchTerm.Trim() + "%" });
            }

            string whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
            string query = $@"SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Title, e.Amount, 
                                    e.ExpenseDate, e.PaymentMethod, e.Notes, e.CreatedAt,
                                    c.CategoryName, c.Color AS CategoryColor, c.Icon AS CategoryIcon,
                                    u.FullName AS UserFullName, u.Email AS UserEmail
                             FROM dbo.Expenses e
                             INNER JOIN dbo.Categories c ON e.CategoryId = c.CategoryId
                             INNER JOIN dbo.Users u ON e.UserId = u.UserId
                             {whereClause}
                             ORDER BY e.ExpenseDate DESC, e.ExpenseId DESC";

            DataTable dt = DbHelper.ExecuteQuery(query, prms.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapExpense(row));
            }
            return list;
        }

        public Expense GetById(int expenseId, int? userId = null)
        {
            string query = @"SELECT e.ExpenseId, e.UserId, e.CategoryId, e.Title, e.Amount, 
                                    e.ExpenseDate, e.PaymentMethod, e.Notes, e.CreatedAt,
                                    c.CategoryName, c.Color AS CategoryColor, c.Icon AS CategoryIcon,
                                    u.FullName AS UserFullName, u.Email AS UserEmail
                             FROM dbo.Expenses e
                             INNER JOIN dbo.Categories c ON e.CategoryId = c.CategoryId
                             INNER JOIN dbo.Users u ON e.UserId = u.UserId
                             WHERE e.ExpenseId = @ExpenseId";
            
            List<SqlParameter> prms = new List<SqlParameter> {
                new SqlParameter("@ExpenseId", SqlDbType.Int) { Value = expenseId }
            };

            if (userId.HasValue && userId.Value > 0)
            {
                query += " AND e.UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            DataTable dt = DbHelper.ExecuteQuery(query, prms.ToArray());
            if (dt.Rows.Count > 0)
            {
                return MapExpense(dt.Rows[0]);
            }
            return null;
        }

        public int Add(Expense expense)
        {
            string query = @"INSERT INTO dbo.Expenses (UserId, CategoryId, Title, Amount, ExpenseDate, PaymentMethod, Notes)
                             VALUES (@UserId, @CategoryId, @Title, @Amount, @ExpenseDate, @PaymentMethod, @Notes);
                             SELECT SCOPE_IDENTITY();";
            SqlParameter[] prms = {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = expense.UserId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = expense.CategoryId },
                new SqlParameter("@Title", SqlDbType.NVarChar) { Value = expense.Title },
                new SqlParameter("@Amount", SqlDbType.Decimal) { Value = expense.Amount },
                new SqlParameter("@ExpenseDate", SqlDbType.Date) { Value = expense.ExpenseDate },
                new SqlParameter("@PaymentMethod", SqlDbType.NVarChar) { Value = expense.PaymentMethod ?? "Cash" },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { Value = (object)expense.Notes ?? DBNull.Value }
            };

            return Convert.ToInt32(DbHelper.ExecuteScalar(query, prms));
        }

        public bool Update(Expense expense, bool isAdmin = false)
        {
            string query = @"UPDATE dbo.Expenses 
                             SET CategoryId = @CategoryId, Title = @Title, Amount = @Amount, 
                                 ExpenseDate = @ExpenseDate, PaymentMethod = @PaymentMethod, Notes = @Notes
                             WHERE ExpenseId = @ExpenseId";
            
            List<SqlParameter> prms = new List<SqlParameter> {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = expense.CategoryId },
                new SqlParameter("@Title", SqlDbType.NVarChar) { Value = expense.Title },
                new SqlParameter("@Amount", SqlDbType.Decimal) { Value = expense.Amount },
                new SqlParameter("@ExpenseDate", SqlDbType.Date) { Value = expense.ExpenseDate },
                new SqlParameter("@PaymentMethod", SqlDbType.NVarChar) { Value = expense.PaymentMethod },
                new SqlParameter("@Notes", SqlDbType.NVarChar) { Value = (object)expense.Notes ?? DBNull.Value },
                new SqlParameter("@ExpenseId", SqlDbType.Int) { Value = expense.ExpenseId }
            };

            if (!isAdmin)
            {
                query += " AND UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = expense.UserId });
            }

            return DbHelper.ExecuteNonQuery(query, prms.ToArray()) > 0;
        }

        public bool Delete(int expenseId, int? userId = null)
        {
            string query = "DELETE FROM dbo.Expenses WHERE ExpenseId = @ExpenseId";
            List<SqlParameter> prms = new List<SqlParameter> {
                new SqlParameter("@ExpenseId", SqlDbType.Int) { Value = expenseId }
            };

            if (userId.HasValue && userId.Value > 0)
            {
                query += " AND UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            return DbHelper.ExecuteNonQuery(query, prms.ToArray()) > 0;
        }

        public decimal GetTotalExpensesThisMonth(int? userId, int month, int year)
        {
            string query = @"SELECT ISNULL(SUM(Amount), 0) FROM dbo.Expenses
                             WHERE MONTH(ExpenseDate) = @Month 
                               AND YEAR(ExpenseDate) = @Year";
            List<SqlParameter> prms = new List<SqlParameter> {
                new SqlParameter("@Month", SqlDbType.Int) { Value = month },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year }
            };

            if (userId.HasValue && userId.Value > 0)
            {
                query += " AND UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            object result = DbHelper.ExecuteScalar(query, prms.ToArray());
            return Convert.ToDecimal(result);
        }

        public decimal GetTotalExpensesAllTime(int? userId = null)
        {
            string query = @"SELECT ISNULL(SUM(Amount), 0) FROM dbo.Expenses";
            List<SqlParameter> prms = new List<SqlParameter>();

            if (userId.HasValue && userId.Value > 0)
            {
                query += " WHERE UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            object result = DbHelper.ExecuteScalar(query, prms.ToArray());
            return Convert.ToDecimal(result);
        }

        public int GetTotalTransactionsCount(int? userId = null)
        {
            string query = @"SELECT COUNT(1) FROM dbo.Expenses";
            List<SqlParameter> prms = new List<SqlParameter>();

            if (userId.HasValue && userId.Value > 0)
            {
                query += " WHERE UserId = @UserId";
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            return Convert.ToInt32(DbHelper.ExecuteScalar(query, prms.ToArray()));
        }

        public List<CategorySpending> GetCategoryBreakdown(int? userId, int month, int year)
        {
            List<CategorySpending> list = new List<CategorySpending>();
            string userFilter = userId.HasValue && userId.Value > 0 ? "AND e.UserId = @UserId" : "";
            string query = $@"SELECT c.CategoryName, c.Color, ISNULL(SUM(e.Amount), 0) AS TotalAmount
                             FROM dbo.Expenses e
                             INNER JOIN dbo.Categories c ON e.CategoryId = c.CategoryId
                             WHERE MONTH(e.ExpenseDate) = @Month 
                               AND YEAR(e.ExpenseDate) = @Year
                               {userFilter}
                             GROUP BY c.CategoryName, c.Color
                             ORDER BY TotalAmount DESC";

            List<SqlParameter> prms = new List<SqlParameter> {
                new SqlParameter("@Month", SqlDbType.Int) { Value = month },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year }
            };

            if (userId.HasValue && userId.Value > 0)
            {
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            DataTable dt = DbHelper.ExecuteQuery(query, prms.ToArray());
            decimal totalMonth = 0;
            foreach (DataRow row in dt.Rows)
            {
                totalMonth += Convert.ToDecimal(row["TotalAmount"]);
            }

            foreach (DataRow row in dt.Rows)
            {
                decimal amount = Convert.ToDecimal(row["TotalAmount"]);
                double pct = totalMonth > 0 ? (double)((amount / totalMonth) * 100) : 0;
                list.Add(new CategorySpending
                {
                    CategoryName = row["CategoryName"].ToString(),
                    Color = row["Color"].ToString(),
                    TotalAmount = amount,
                    Percentage = Math.Round(pct, 1)
                });
            }

            return list;
        }

        public List<MonthlyTrend> GetLast6MonthsTrend(int? userId = null)
        {
            List<MonthlyTrend> list = new List<MonthlyTrend>();
            string userFilter = userId.HasValue && userId.Value > 0 ? "AND UserId = @UserId" : "";
            string query = $@"SELECT 
                                FORMAT(ExpenseDate, 'MMM yyyy') AS MonthLabel,
                                YEAR(ExpenseDate) AS ExpYear,
                                MONTH(ExpenseDate) AS ExpMonth,
                                SUM(Amount) AS TotalAmount
                             FROM dbo.Expenses
                             WHERE ExpenseDate >= DATEADD(MONTH, -5, DATEADD(DAY, 1 - DAY(GETDATE()), CAST(GETDATE() AS DATE)))
                               {userFilter}
                             GROUP BY FORMAT(ExpenseDate, 'MMM yyyy'), YEAR(ExpenseDate), MONTH(ExpenseDate)
                             ORDER BY ExpYear ASC, ExpMonth ASC";

            List<SqlParameter> prms = new List<SqlParameter>();
            if (userId.HasValue && userId.Value > 0)
            {
                prms.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId.Value });
            }

            DataTable dt = DbHelper.ExecuteQuery(query, prms.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new MonthlyTrend
                {
                    MonthLabel = row["MonthLabel"].ToString(),
                    Amount = Convert.ToDecimal(row["TotalAmount"])
                });
            }

            return list;
        }

        private Expense MapExpense(DataRow row)
        {
            return new Expense
            {
                ExpenseId = Convert.ToInt32(row["ExpenseId"]),
                UserId = Convert.ToInt32(row["UserId"]),
                UserFullName = row.Table.Columns.Contains("UserFullName") && row["UserFullName"] != DBNull.Value ? row["UserFullName"].ToString() : null,
                UserEmail = row.Table.Columns.Contains("UserEmail") && row["UserEmail"] != DBNull.Value ? row["UserEmail"].ToString() : null,
                CategoryId = Convert.ToInt32(row["CategoryId"]),
                CategoryName = row["CategoryName"].ToString(),
                CategoryColor = row["CategoryColor"] != DBNull.Value ? row["CategoryColor"].ToString() : "#0d6efd",
                CategoryIcon = row["CategoryIcon"] != DBNull.Value ? row["CategoryIcon"].ToString() : "bi-tag",
                Title = row["Title"].ToString(),
                Amount = Convert.ToDecimal(row["Amount"]),
                ExpenseDate = Convert.ToDateTime(row["ExpenseDate"]),
                PaymentMethod = row["PaymentMethod"].ToString(),
                Notes = row["Notes"] != DBNull.Value ? row["Notes"].ToString() : null,
                CreatedAt = Convert.ToDateTime(row["CreatedAt"])
            };
        }
    }
}
