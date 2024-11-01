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
            StoredAuthToken = HttpContext.Session.GetString("auth_token");
            DeviceId = GetUniqueDeviceId();

            if (!UserId.HasValue)
            {
                UserId = ValidateDeviceIdAndFetchUser(DeviceId);

                if (UserId.HasValue)
                {
                    return RedirectToHomeOrDashboard();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Device authentication failed.");
                }
            }
            else
            {
                if (StoredAuthToken == DeviceId)
                {
                    return RedirectToHomeOrDashboard();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Device authentication failed.");
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

        private string GetUniqueDeviceId()
        {
            try
            {
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

            return "Device ID not found"; // Default case if no device ID is found
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
                        int userId = reader.GetInt32(0);
                        string username = reader.GetString(1);
                        string profilepic = reader.GetString(2);
                        string email = reader.GetString(5);
                        string role = reader.GetString(6);

                        HttpContext.Session.SetInt32("Id", userId);
                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetString("profilepic", profilepic);
                        HttpContext.Session.SetString("email", email);
                        HttpContext.Session.SetString("UserRole", role);

                        return userId;
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
