using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Expense_Tracker.Models
{
    public class CategoryRepository
    {
        public List<Category> GetAll(string type = null)
        {
            List<Category> list = new List<Category>();
            string query = "SELECT CategoryId, CategoryName, CategoryType, Color, Icon FROM dbo.Categories";
            SqlParameter[] prms = null;

            if (!string.IsNullOrEmpty(type))
            {
                query += " WHERE CategoryType = @Type";
                prms = new SqlParameter[] {
                    new SqlParameter("@Type", SqlDbType.NVarChar) { Value = type }
                };
            }

            query += " ORDER BY CategoryName";

            DataTable dt = DbHelper.ExecuteQuery(query, prms);
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    CategoryType = row["CategoryType"].ToString(),
                    Color = row["Color"] != DBNull.Value ? row["Color"].ToString() : "#0d6efd",
                    Icon = row["Icon"] != DBNull.Value ? row["Icon"].ToString() : "bi-tag"
                });
            }
            return list;
        }

        public Category GetById(int id)
        {
            string query = "SELECT CategoryId, CategoryName, CategoryType, Color, Icon FROM dbo.Categories WHERE CategoryId = @Id";
            SqlParameter[] prms = {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id }
            };

            DataTable dt = DbHelper.ExecuteQuery(query, prms);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                return new Category
                {
                    CategoryId = Convert.ToInt32(row["CategoryId"]),
                    CategoryName = row["CategoryName"].ToString(),
                    CategoryType = row["CategoryType"].ToString(),
                    Color = row["Color"].ToString(),
                    Icon = row["Icon"].ToString()
                };
            }
            return null;
        }

        public int Add(Category category)
        {
            string query = @"INSERT INTO dbo.Categories (CategoryName, CategoryType, Color, Icon)
                             VALUES (@CategoryName, @CategoryType, @Color, @Icon);
                             SELECT SCOPE_IDENTITY();";
            SqlParameter[] prms = {
                new SqlParameter("@CategoryName", SqlDbType.NVarChar) { Value = category.CategoryName },
                new SqlParameter("@CategoryType", SqlDbType.NVarChar) { Value = category.CategoryType ?? "Expense" },
                new SqlParameter("@Color", SqlDbType.NVarChar) { Value = category.Color ?? "#0d6efd" },
                new SqlParameter("@Icon", SqlDbType.NVarChar) { Value = category.Icon ?? "bi-tag" }
            };

            return Convert.ToInt32(DbHelper.ExecuteScalar(query, prms));
        }

        public bool Update(Category category)
        {
            string query = @"UPDATE dbo.Categories 
                             SET CategoryName = @CategoryName, CategoryType = @CategoryType, Color = @Color, Icon = @Icon
                             WHERE CategoryId = @CategoryId";
            SqlParameter[] prms = {
                new SqlParameter("@CategoryName", SqlDbType.NVarChar) { Value = category.CategoryName },
                new SqlParameter("@CategoryType", SqlDbType.NVarChar) { Value = category.CategoryType },
                new SqlParameter("@Color", SqlDbType.NVarChar) { Value = category.Color },
                new SqlParameter("@Icon", SqlDbType.NVarChar) { Value = category.Icon },
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = category.CategoryId }
            };

            return DbHelper.ExecuteNonQuery(query, prms) > 0;
        }

        public bool Delete(int categoryId)
        {
            // Verify if there are expenses attached
            string checkQuery = "SELECT COUNT(1) FROM dbo.Expenses WHERE CategoryId = @CategoryId";
            SqlParameter[] checkPrms = {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId }
            };
            int count = Convert.ToInt32(DbHelper.ExecuteScalar(checkQuery, checkPrms));
            if (count > 0)
            {
                return false; // Cannot delete category that has existing expenses
            }

            string query = "DELETE FROM dbo.Categories WHERE CategoryId = @CategoryId";
            SqlParameter[] prms = {
                new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId }
            };

            return DbHelper.ExecuteNonQuery(query, prms) > 0;
        }
    }
}
