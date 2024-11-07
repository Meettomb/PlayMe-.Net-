using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Netflix.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Main_Project.Pages.DeshbordModel;

namespace Main_Project.Pages
{
    public class DeshbordModel : PageModel
    {
        public List<user_regi> userlist { get; private set; } = new List<user_regi>();
        public List<Revenue> Revenue = new List<Revenue>();
        public List<Feedback_table> feedbacklist = new List<Feedback_table>();
        public List<UserGrowthData> UserGrowth { get; set; } = new List<UserGrowthData>();
        public List<UserGrowthPercentage> UserGrowthPercentages { get; set; } = new List<UserGrowthPercentage>();
        public string UserGrowthJson => JsonConvert.SerializeObject(UserGrowth);

        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public decimal TotalcardPayment { get; set; }
        public decimal TotalUPIPayment { get; set; }
        public decimal CurrentMonthUPIPayment { get; set; }
        public decimal PreviousMonthUPIPayment { get; set; }
        public decimal CurrentMonthCardPayment { get; set; }
        public decimal PreviousMonthCardPayment { get; set; }
        public decimal TotalUPICardPayment { get; set; }


        public int Users2022 { get; private set; }
        public int Users2023 { get; private set; }
        public int Users2024 { get; private set; }
        public int TotalUser { get; private set; }
      

        public decimal CurrentMonthPercentage { get; set; }
        public decimal PreviousMonthPercentage { get; set; }
        public decimal MonthBeforePreviousPercentage { get; set; }


        private readonly string _connectionString;
        public DeshbordModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }



        public List<MonthlyProfitData> MonthlyProfits { get; set; } = new List<MonthlyProfitData>();


        public IActionResult OnGet()
        {


            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT username, dob, gender, profilepic FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Fetch recent users
                string query = "SELECT TOP 5 * FROM User_data WHERE isactive = @isactive ORDER BY date DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@isactive", true);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            user_regi user = new user_regi
                            {
                                userid = dr.IsDBNull(0) ? 0 : dr.GetInt32(0),
                                fullname = dr.IsDBNull(1) ? string.Empty : dr.GetString(1),
                                email = dr.IsDBNull(2) ? string.Empty : dr.GetString(2),
                                phone = dr.IsDBNull(3) ? string.Empty : dr.GetString(3),
                                password = dr.IsDBNull(4) ? string.Empty : dr.GetString(4),
                                isactive = dr.IsDBNull(5) ? false : dr.GetBoolean(5),
                                paymentmethod = dr.IsDBNull(9) ? string.Empty : dr.GetString(9),
                                role = dr.IsDBNull(10) ? string.Empty : dr.GetString(10),
                                profilepic = dr.IsDBNull(12) ? string.Empty : dr.GetString(12),
                                datetime3 = dr.IsDBNull(dr.GetOrdinal("datetime")) ? string.Empty : dr.GetString(dr.GetOrdinal("datetime")),

                            };

                            userlist.Add(user);
                        }

                    }
                }

                // Total payments by method
                string totalcardPaymentQuery = "SELECT SUM(CAST(price1 AS DECIMAL(18, 2))) FROM Revenue WHERE paymentmethod = @paymentmethod";
                using (SqlCommand cmd = new SqlCommand(totalcardPaymentQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "Card");
                    var result = cmd.ExecuteScalar();
                    TotalcardPayment = result == DBNull.Value ? 0 : (decimal)result;
                }

                string totalUPIPaymentQuery = "SELECT SUM(CAST(price1 AS DECIMAL(18, 2))) FROM Revenue WHERE paymentmethod = @paymentmethod";
                using (SqlCommand cmd = new SqlCommand(totalUPIPaymentQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "UPI");
                    var result = cmd.ExecuteScalar();
                    TotalUPIPayment = result == DBNull.Value ? 0 : (decimal)result;
                }



                string totalPaymentQuery = @"
                SELECT SUM(CAST(price1 AS DECIMAL(18, 2))) AS TotalPayment
                FROM Revenue
                WHERE paymentmethod IN ('Card', 'UPI')";

                using (SqlCommand cmd = new SqlCommand(totalPaymentQuery, con))
                {
                    TotalUPICardPayment = (decimal)cmd.ExecuteScalar();
                }


                // Current Month UPI Payment
                string currentMonthQuery = @"
                SELECT SUM(CAST(price1 AS DECIMAL(18, 2)))
                FROM Revenue
                WHERE paymentmethod = @paymentmethod
                  AND LEFT(datetime1, 7) = LEFT(CONVERT(VARCHAR, GETDATE(), 120), 7)";
                using (SqlCommand cmd = new SqlCommand(currentMonthQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "UPI");
                    var result = cmd.ExecuteScalar();
                    CurrentMonthUPIPayment = result == DBNull.Value ? 0 : (decimal)result;
                }
                // Previous Month UPI Payment
                string previousMonthQuery = @"
                SELECT SUM(CAST(price1 AS DECIMAL(18, 2)))
                FROM Revenue
                WHERE paymentmethod = @paymentmethod
                  AND LEFT(datetime1, 7) = LEFT(CONVERT(VARCHAR, DATEADD(MONTH, -1, GETDATE()), 120), 7)";
                using (SqlCommand cmd = new SqlCommand(previousMonthQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "UPI");
                    var result = cmd.ExecuteScalar();
                    PreviousMonthUPIPayment = result == DBNull.Value ? 0 : (decimal)result;
                }




                // Current Month Card Payment
                string currentMonthCardQuery = @"
                SELECT SUM(CAST(price1 AS DECIMAL(18, 2)))
                FROM Revenue
                WHERE paymentmethod = @paymentmethod
                  AND LEFT(datetime1, 7) = LEFT(CONVERT(VARCHAR, GETDATE(), 120), 7)";

                using (SqlCommand cmd = new SqlCommand(currentMonthCardQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "Card");
                    var result = cmd.ExecuteScalar();
                    CurrentMonthCardPayment = result == DBNull.Value ? 0 : (decimal)result;
                }
                // Previous Month Card Payment
                string previousMonthCardQuery = @"
                SELECT SUM(CAST(price1 AS DECIMAL(18, 2)))
                FROM Revenue
                WHERE paymentmethod = @paymentmethod
                  AND LEFT(datetime1, 7) = LEFT(CONVERT(VARCHAR, DATEADD(MONTH, -1, GETDATE()), 120), 7)";
                using (SqlCommand cmd = new SqlCommand(previousMonthCardQuery, con))
                {
                    cmd.Parameters.AddWithValue("@paymentmethod", "Card");
                    var result = cmd.ExecuteScalar();
                    PreviousMonthCardPayment = result == DBNull.Value ? 0 : (decimal)result;
                }


                // Monthly Profit Data
                string monthlyProfitQuery = @"
                    SELECT 
                        YEAR(CONVERT(DATE, LEFT(datetime1, 10), 120)) AS Year,
                        MONTH(CONVERT(DATE, LEFT(datetime1, 10), 120)) AS Month,
                        SUM(CAST(price1 AS DECIMAL(18, 2))) AS TotalPayment
                    FROM Revenue
                    WHERE paymentmethod IN ('Card', 'UPI')
                      AND ISDATE(LEFT(datetime1, 10)) = 1
                      AND YEAR(CONVERT(DATE, LEFT(datetime1, 10), 120)) BETWEEN YEAR(GETDATE()) - 2 AND YEAR(GETDATE())
                    GROUP BY 
                        YEAR(CONVERT(DATE, LEFT(datetime1, 10), 120)),
                        MONTH(CONVERT(DATE, LEFT(datetime1, 10), 120))
                    ORDER BY 
                        Year, 
                        Month";


                using (SqlCommand cmd = new SqlCommand(monthlyProfitQuery, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var data = new MonthlyProfitData
                            {
                                Year = dr.GetInt32(0),
                                Month = dr.GetInt32(1),
                                TotalPayment = dr.GetDecimal(2)
                            };
                            MonthlyProfits.Add(data);
                        }
                    }
                }


                // Fetch user Groth
                string userGrowthQuery = @"
                SELECT 
                    YEAR(date) AS Year, 
                    MONTH(date) AS Month,
                    COUNT(*) AS UserCount 
                FROM User_data 
                WHERE date >= DATEADD(YEAR, -2, GETDATE()) 
                GROUP BY YEAR(date), MONTH(date) 
                ORDER BY Year, Month";

                using (SqlCommand cmd = new SqlCommand(userGrowthQuery, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var growthData = new UserGrowthData
                            {
                                Year = dr.GetInt32(0),
                                Month = dr.GetInt32(1),
                                UserCount = dr.GetInt32(2)
                            };
                            UserGrowth.Add(growthData);
                        }
                    }
                }


                // Fetch feedback data and user details
                string feedbackQuery = @"
                SELECT TOP 5 f.username, f.feedbackemail, f.feedback, f.userid, 
                             u.email, u.profilepic, u.username 
                FROM Feedback_table f
                INNER JOIN User_data u ON f.userid = u.id
                ORDER BY f.userid ASC";

                using (SqlCommand cmd = new SqlCommand(feedbackQuery, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var feedback = new Feedback_table
                            {
                                username = dr.IsDBNull(0) ? string.Empty : dr.GetString(0),
                                feedbackemail = dr.IsDBNull(1) ? string.Empty : dr.GetString(1),
                                feedback = dr.IsDBNull(2) ? string.Empty : dr.GetString(2),
                                userid = dr.IsDBNull(3) ? 0 : dr.GetInt32(3),
                                // Additional fields from User_data table
                                Email = dr.IsDBNull(4) ? string.Empty : dr.GetString(4),
                                profilepic = dr.IsDBNull(5) ? string.Empty : dr.GetString(5),
                                UserName = dr.IsDBNull(6) ? string.Empty : dr.GetString(6)
                            };

                            feedbacklist.Add(feedback);
                        }
                    }


                }
                CalculateUserStatistics(_connectionString);
            }
            return Page();
        }
        private void CalculateUserStatistics(string connectionString)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Count users for 2022
                Users2022 = CountUsersByYear(con, 2022);

                // Count users for 2023
                Users2023 = CountUsersByYear(con, 2023);

                // Count users for 2024
                Users2024 = CountUsersByYear(con, 2024);
                // Count total users
                TotalUser = CountTotalUsers(con);
            }
        }

        private int CountUsersByYear(SqlConnection con, int year)
        {
            // Updated SQL query using the DateOnly column
            string query = "SELECT COUNT(*) FROM User_data WHERE YEAR(date) = @Year";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Year", year);
                return (int)cmd.ExecuteScalar();
            }

            //TotalUser
        }

        private int CountTotalUsers(SqlConnection con)
        {
            // SQL query to count the total number of users
            string query = "SELECT COUNT(*) FROM User_data";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                // Execute the query and return the result as an integer
                return (int)cmd.ExecuteScalar();
            }
        }

        public class UserGrowthData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public int UserCount { get; set; }
        }

        public class UserGrowthPercentage
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal GrowthPercentage { get; set; }
        }

        public class MonthlyProfitData
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal TotalPayment { get; set; }
        }
    }
}