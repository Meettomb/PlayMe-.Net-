using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using Main_Project.Models;
using Microsoft.Extensions.Configuration;

namespace Main_Project.Pages
{
    public class IndexModel : PageModel
    {
        public List<question_answer> questio_answer_list { get; set; } = new List<question_answer>();

        private readonly string _connectionString;

        public int? UserId { get; set; }
        public string StoredAuthToken { get; set; } // Auth token from session
        public string DeviceId { get; set; } // New property for device ID

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");


            if (UserId.HasValue)
            {
                // If the user is logged in, redirect to the Home page
                return RedirectToPage("/Home");
            }

            // Retrieve the device ID from the cookie
            string deviceId = Request.Cookies["deviceUniqueId"];
            if (string.IsNullOrEmpty(deviceId))
            {
                // If the device ID is not present, stay on the Index page
                return Page();
            }

            // Query the database to check if the auth_token (device ID) exists in the comma-separated list
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT id, username, profilepic, dob, gender, email, role, isactive, subscriptionactive 
                         FROM User_data 
                         WHERE ',' + auth_token + ',' LIKE '%,' + @DeviceId + ',%'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Retrieve user information
                        int userId = Convert.ToInt32(reader["id"]);
                        string username = reader["username"].ToString();
                        string profilepic = reader["profilepic"].ToString();
                        string dob = reader["dob"].ToString();
                        string gender = reader["gender"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = reader["isactive"] != DBNull.Value && Convert.ToBoolean(reader["isactive"]);
                        bool subscriptionActive = reader["subscriptionactive"] != DBNull.Value && Convert.ToBoolean(reader["subscriptionactive"]);

                        if (isActive)
                        {
                            // Store necessary data in the session
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("Username", username);
                            HttpContext.Session.SetString("profilepic", profilepic);
                            HttpContext.Session.SetString("email", email);
                            HttpContext.Session.SetString("dob", dob);
                            HttpContext.Session.SetString("gender", gender);
                            HttpContext.Session.SetString("UserRole", role);
                            HttpContext.Session.SetString("SubscriptionActive", subscriptionActive.ToString());

                            // Redirect to the Home page
                            return RedirectToHomeOrDashboard();
                        }
                    }

                    reader.Close();
                }
            }

            GetQuestionsAndAnswers();
            return Page();
        }

        private IActionResult RedirectToHomeOrDashboard()
        {
            string userRole = HttpContext.Session.GetString("UserRole");

            if (userRole == "admin")
            {
                return RedirectToPage("/Deshbord");
            }
            else if (userRole == "user")
            {
                return RedirectToPage("/Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid user role.");
            return Page();
        }

      
        private void GetQuestionsAndAnswers()
        {
            string query = "SELECT * FROM Questions";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        question_answer qa = new question_answer
                        {
                            id = reader.GetInt32(0),
                            question = reader.GetString(1),
                            answer = reader.GetString(2)
                        };
                        questio_answer_list.Add(qa);
                    }
                }
            }
        }
    }
}
