using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Expense_Tracker.Models
{
    /// <summary>
    /// Helper class providing ADO.NET connection and query execution utilities
    /// Demonstrates fundamental .NET ADO.NET concepts (SqlConnection, SqlCommand, SqlDataAdapter, IDisposable)
    /// </summary>
    public static class DbHelper
    {
        private static readonly string ConnectionString = 
            ConfigurationManager.ConnectionStrings["ExpenseTrackerConnection"].ConnectionString;

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    con.Open();
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
