using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Main_Project.Pages.User_Profile_manage
{
    public class Subscription_detailModel : PageModel
    {
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string id { get; set; }
        public string UserRole { get; set; }
        public string Subscription_id { get; set; }

        // Subscription details properties
        public int? SubscriptionPrice { get; set; }
        public int? SubscriptionDuration { get; set; }
        public string SubscriptionDetail { get; set; }
        public string SubscriptionName { get; set; }


        private readonly string _connectionString;
        public Subscription_detailModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public void OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");
            if (UserId.HasValue)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    // Retrieve user data
                    string selectUserQuery = "SELECT * FROM User_data WHERE id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(selectUserQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId); // Corrected parameter name
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                id = reader["id"].ToString();
                                UserRole = reader["role"].ToString();
                                Subscription_id = reader["subid"].ToString();
                            }
                        }
                        con.Close();
                    }

                    // Check if Subscription_id is valid and fetch subscription details
                    if (int.TryParse(Subscription_id, out int subscriptionId))
                    {
                        string selectSubscriptionQuery = "SELECT * FROM Subscription WHERE id = @SubscriptionId";
                        using (SqlCommand cmd = new SqlCommand(selectSubscriptionQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@SubscriptionId", subscriptionId);
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    SubscriptionPrice = (int)reader["price"];
                                    SubscriptionDuration = (int)reader["planeduration"];
                                    SubscriptionDetail = reader["planedetail"].ToString();
                                    SubscriptionName = reader["planename"]?.ToString();
                                }
                            }
                            con.Close();
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Subscription ID.");
                    }
                }
            }
        }
    }
}
