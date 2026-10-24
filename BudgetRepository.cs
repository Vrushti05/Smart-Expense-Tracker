using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Expense_Tracker.Models
{
    public class BudgetRepository
    {
        public List<Budget> GetBudgetsForMonth(int userId, int month, int year)
        {
            List<Budget> list = new List<Budget>();
            string query = @"SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetLimit, b.BudgetMonth, b.BudgetYear,
                                    c.CategoryName,
                                    ISNULL(e.TotalSpent, 0) AS TotalSpent
                             FROM dbo.Budgets b
                             INNER JOIN dbo.Categories c ON b.CategoryId = c.CategoryId
                             LEFT JOIN (
                                 SELECT CategoryId, SUM(Amount) AS TotalSpent
                                 FROM dbo.Expenses
                                 WHERE UserId = @UserId AND MONTH(ExpenseDate) = @Month AND YEAR(ExpenseDate) = @Year
                                 GROUP BY CategoryId
                             ) e ON b.CategoryId = e.CategoryId
                             WHERE b.UserId = @UserId AND b.BudgetMonth = @Month AND b.BudgetYear = @Year
                             ORDER BY c.CategoryName";
            SqlParameter[] prms = {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId },
                new SqlParameter("@Month", SqlDbType.Int) { Value = month },
                new SqlParameter("@Year", SqlDbType.Int) { Value = year }
            };

            DataTable dt = DbHelper.ExecuteQuery(query, prms);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Budget
                {
                    BudgetId = Convert.ToInt32(row["BudgetId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    BudgetLimit = Convert.ToDecimal(row["BudgetLimit"]),
                    BudgetMonth = Convert.ToInt32(row["BudgetMonth"]),
                    BudgetYear = Convert.ToInt32(row["BudgetYear"]),
                    TotalSpent = Convert.ToDecimal(row["TotalSpent"])
                });
            }

            return list;
        }

        public Budget GetById(int budgetId, int userId)
        {
            string query = @"SELECT b.BudgetId, b.UserId, b.CategoryId, b.BudgetLimit, b.BudgetMonth, b.BudgetYear,
                                    c.CategoryName
                             FROM dbo.Budgets b
                             INNER JOIN dbo.Categories c ON b.CategoryId = c.CategoryId
                             WHERE b.BudgetId = @BudgetId AND b.UserId = @UserId";
            SqlParameter[] prms = {
                new SqlParameter("@BudgetId", SqlDbType.Int) { Value = budgetId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };

            DataTable dt = DbHelper.ExecuteQuery(query, prms);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Budget
                {
                    BudgetId = Convert.ToInt32(row["BudgetId"]),
                    UserId = Convert.ToInt32(row["UserId"]),
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    BudgetLimit = Convert.ToDecimal(row["BudgetLimit"]),
                    BudgetMonth = Convert.ToInt32(row["BudgetMonth"]),
                    BudgetYear = Convert.ToInt32(row["BudgetYear"])
                };
            }
            return null;
        }

        public bool SaveOrUpdate(Budget budget)
        {
            // Check if budget already exists for this category and month/year
            string checkQuery = @"SELECT BudgetId FROM dbo.Budgets 
                                  WHERE UserId = @UserId AND CategoryId = @CategoryId 
                                    AND BudgetMonth = @Month AND BudgetYear = @Year";
            SqlParameter[] checkPrms = {
                new SqlParameter("@UserId", SqlDbType.Int) { Value = budget.UserId },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = budget.CategoryId },
                new SqlParameter("@Month", SqlDbType.Int) { Value = budget.BudgetMonth },
                new SqlParameter("@Year", SqlDbType.Int) { Value = budget.BudgetYear }
            };

            object existingId = DbHelper.ExecuteScalar(checkQuery, checkPrms);

            if (existingId != null && existingId != DBNull.Value)
            {
                string updateQuery = @"UPDATE dbo.Budgets SET BudgetLimit = @Limit WHERE BudgetId = @BudgetId";
                SqlParameter[] updatePrms = {
                    new SqlParameter("@Limit", SqlDbType.Decimal) { Value = budget.BudgetLimit },
                    new SqlParameter("@BudgetId", SqlDbType.Int) { Value = Convert.ToInt32(existingId) }
                };
                return DbHelper.ExecuteNonQuery(updateQuery, updatePrms) > 0;
            }
            else
            {
                string insertQuery = @"INSERT INTO dbo.Budgets (UserId, CategoryId, BudgetLimit, BudgetMonth, BudgetYear)
                                       VALUES (@UserId, @CategoryId, @Limit, @Month, @Year)";
                SqlParameter[] insertPrms = {
                    new SqlParameter("@UserId", SqlDbType.Int) { Value = budget.UserId },
                    new SqlParameter("@CategoryId", SqlDbType.Int) { Value = budget.CategoryId },
                    new SqlParameter("@Limit", SqlDbType.Decimal) { Value = budget.BudgetLimit },
                    new SqlParameter("@Month", SqlDbType.Int) { Value = budget.BudgetMonth },
                    new SqlParameter("@Year", SqlDbType.Int) { Value = budget.BudgetYear }
                };
                return DbHelper.ExecuteNonQuery(insertQuery, insertPrms) > 0;
            }
        }

        public bool Delete(int budgetId, int userId)
        {
            string query = "DELETE FROM dbo.Budgets WHERE BudgetId = @BudgetId AND UserId = @UserId";
            SqlParameter[] prms = {
                new SqlParameter("@BudgetId", SqlDbType.Int) { Value = budgetId },
                new SqlParameter("@UserId", SqlDbType.Int) { Value = userId }
            };
            return DbHelper.ExecuteNonQuery(query, prms) > 0;
        }
    }
}
