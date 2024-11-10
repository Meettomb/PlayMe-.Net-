using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using System.Management; // Add this line for ManagementObject and ManagementObjectSearcher

using Xamarin.Essentials;

using NuGet.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace Netflix.Pages
{
    public class Sign_inModel : PageModel
    {
        [BindProperty]
        public user_regi User { get; set; }

        private readonly IConfiguration _configuration;
        private static Random random = new Random();

        private readonly string _connectionString;

        public Sign_inModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
            _configuration = configuration;
        }

        public int? UserId { get; set; } // Property to hold the User ID
        public string StoredAuthToken { get; set; } // Auth token from session
        public string DeviceId { get; set; } // New property for device ID

        public IActionResult OnGet()
        {
            // Retrieve user ID from session to check if the user is already logged in
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

            // If auth_token does not match any user, stay on the Index page
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







        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(User.email) || string.IsNullOrEmpty(User.password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string userQuery = @"SELECT id, username, profilepic, dob, gender, email, role, isactive, 
                            subscriptionactive, password, auth_token, subid
                            FROM User_data 
                            WHERE email = @Email AND isactive = 1";

                using (SqlCommand cmd = new SqlCommand(userQuery, con))
                {
                    cmd.Parameters.AddWithValue("@Email", User.email);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Retrieve user information
                        string storedPassword = reader["password"].ToString();
                        int userId = Convert.ToInt32(reader["id"]);
                        string username = reader["username"].ToString();
                        string profilepic = reader["profilepic"].ToString();
                        string dob = reader["dob"].ToString();
                        string gender = reader["gender"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = Convert.ToBoolean(reader["isactive"]);
                        bool subscriptionActive = Convert.ToBoolean(reader["subscriptionactive"]);
                        string auth_token = reader["auth_token"]?.ToString() ?? string.Empty;
                        int subId = Convert.ToInt32(reader["subid"]);

                        reader.Close();

                        // Verify the password
                        var passwordHasher = new PasswordHasher<object>();
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(null, storedPassword, User.password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            if (!isActive)
                            {
                                ModelState.AddModelError(string.Empty, "User not found.");
                                return Page();
                            }

                            // Check if the user is an admin; skip device check if so
                            if (role == "admin")
                            {
                                // Set session values
                                HttpContext.Session.SetInt32("Id", userId);
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("profilepic", profilepic);
                                HttpContext.Session.SetString("email", email);
                                HttpContext.Session.SetString("dob", dob);
                                HttpContext.Session.SetString("gender", gender);
                                HttpContext.Session.SetString("UserRole", role);
                                HttpContext.Session.SetString("SubscriptionActive", subscriptionActive.ToString());

                                return RedirectToPage("/Deshbord");
                            }
                            else
                            {


                            // For users, fetch subscription details and check device limit
                            string subscriptionQuery = @"SELECT planedetail FROM Subscription WHERE id = @SubId";
                            using (SqlCommand subCmd = new SqlCommand(subscriptionQuery, con))
                            {
                                subCmd.Parameters.AddWithValue("@SubId", subId);
                                SqlDataReader subReader = subCmd.ExecuteReader();

                                if (subReader.Read())
                                {
                                    string planDetails = subReader["planedetail"].ToString();
                                    subReader.Close();

                                    // Parse the maximum login count from plan details
                                    int maxLoginCount = 1; // Default to 1 if not specified
                                    if (planDetails.Contains("User login at a time"))
                                    {
                                        string[] details = planDetails.Split('+');
                                        foreach (string detail in details)
                                        {
                                            if (detail.Contains("User login at a time"))
                                            {
                                                int.TryParse(new string(detail.Where(char.IsDigit).ToArray()), out maxLoginCount);
                                                break;
                                            }
                                        }
                                    }

                                    // Retrieve or create the device ID
                                    string deviceId = Request.Cookies["deviceUniqueId"];
                                    if (string.IsNullOrEmpty(deviceId))
                                    {
                                        deviceId = "id-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" + Guid.NewGuid().ToString("N");
                                        Response.Cookies.Append("deviceUniqueId", deviceId, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
                                    }

                                    // Check current device count
                                    var deviceList = auth_token.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    if (!deviceList.Contains(deviceId))
                                    {
                                        if (deviceList.Count < maxLoginCount)
                                        {
                                            // Add new device ID to auth_token
                                            deviceList.Add(deviceId);
                                        }
                                        else
                                        {
                                            // Remove excess devices if they exceed maxLoginCount
                                            while (deviceList.Count > maxLoginCount)
                                            {
                                                deviceList.RemoveAt(0);  // Remove the oldest devices
                                            }

                                            // Logout the last device if the limit is exceeded
                                            ModelState.AddModelError(string.Empty, $"Your plan allows a maximum of {maxLoginCount} devices to be logged in at the same time.");
                                            string updateQuery = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";
                                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                            {
                                                updateCmd.Parameters.AddWithValue("@AuthToken", string.Join(",", deviceList));
                                                updateCmd.Parameters.AddWithValue("@UserId", userId);
                                                updateCmd.ExecuteNonQuery();
                                            }

                                                return Page(); // Redirect to index page if limit exceeded
                                            }
                                    }

                                    // Update the auth_token in the database
                                    string updatedAuthToken = string.Join(",", deviceList);
                                    string updateQueryAuthToken = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";
                                    using (SqlCommand updateCmd = new SqlCommand(updateQueryAuthToken, con))
                                    {
                                        updateCmd.Parameters.AddWithValue("@AuthToken", updatedAuthToken);
                                        updateCmd.Parameters.AddWithValue("@UserId", userId);
                                        updateCmd.ExecuteNonQuery();
                                    }

                                    // Set session values for the user
                                    HttpContext.Session.SetInt32("Id", userId);
                                    HttpContext.Session.SetString("Username", username);
                                    HttpContext.Session.SetString("profilepic", profilepic);
                                    HttpContext.Session.SetString("email", email);
                                    HttpContext.Session.SetString("dob", dob);
                                    HttpContext.Session.SetString("gender", gender);
                                    HttpContext.Session.SetString("UserRole", role);
                                    HttpContext.Session.SetString("SubscriptionActive", subscriptionActive.ToString());

                                    return RedirectToPage("/Home");
                                }
                            }
                        }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Email not found.");
                    }

                    con.Close();
                }
            }
            return Page();
        }




    }
}
