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

        // Define the UserId and StoredAuthToken properties
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

            // Fetch the stored auth token from session
            StoredAuthToken = HttpContext.Session.GetString("auth_token");

            // Get the unique device ID
            DeviceId = GetUniqueDeviceId();

            // Check if UserId is not set, then verify device ID against database
            if (!UserId.HasValue)
            {
                UserId = ValidateDeviceIdAndFetchUser(DeviceId);

                if (UserId.HasValue)
                {
                    // Redirect to the home page after setting user details in session
                    return RedirectToPage("/Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Device authentication failed.");
                }
            }
            else
            {
                // Check if auth_token matches the unique device ID
                if (StoredAuthToken == DeviceId)
                {
                    return RedirectToPage("/Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Device authentication failed.");
                }
            }

            // Fetch data from the database
            GetQuestionsAndAnswers();

            return Page();
        }

        private string GetUniqueDeviceId()
        {
            try
            {
                // Implement logic to get the unique device ID
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["SerialNumber"]?.ToString();
                    }
                }
            }
            catch
            {
                // Handle exceptions as necessary
            }

            return "Device ID not found"; // Handle default case
        }

        private int? ValidateDeviceIdAndFetchUser(string deviceId)
        {
            string query = "SELECT id, username, profilepic, dob, gender, email, role FROM User_data WHERE auth_token = @DeviceId";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DeviceId", deviceId);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Retrieve and store user details in session
                        int userId = reader.GetInt32(0);
                        string username = reader.GetString(1);
                        string profilepic = reader.GetString(2);
                        string dob = reader.GetString(3);
                        string gender = reader.GetString(4);
                        string email = reader.GetString(5);
                        string role = reader.GetString(6);

                        // Store user details in session
                        HttpContext.Session.SetInt32("Id", userId);
                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetString("profilepic", profilepic);
                        HttpContext.Session.SetString("email", email);
                        HttpContext.Session.SetString("dob", dob);
                        HttpContext.Session.SetString("gender", gender);
                        HttpContext.Session.SetString("UserRole", role);

                        return userId; // Return the found user ID
                    }
                }
            }
            return null; // Return null if no user is found
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
