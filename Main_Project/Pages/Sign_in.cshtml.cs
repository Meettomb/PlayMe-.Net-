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

        public Sign_inModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int? UserId { get; set; } // Property to hold the User ID

        public IActionResult OnGet()
        {
            // Retrieve user ID from session and store it in the UserId property
            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                // If the user is logged in (UserId exists), redirect to the Home page
                return RedirectToPage("/Home");
            }

            // If the user is not logged in (UserId does not exist), stay on the Index page
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(User.email) || string.IsNullOrEmpty(User.password))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return Page();
            }

            string connectionString = _configuration.GetConnectionString("NetflixDatabase");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT id, username, profilepic, dob, gender, email, role, isactive, 
                        logintime, subscriptionactive, password, auth_token 
                        FROM User_data 
                        WHERE email = @Email AND isactive = 1";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", User.email);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Retrieve and store necessary fields in local variables
                        string storedPassword = reader["password"].ToString();
                        int userId = Convert.ToInt32(reader["id"]);
                        string username = reader["username"].ToString();
                        string profilepic = reader["profilepic"].ToString();
                        string dob = reader["dob"].ToString();
                        string gender = reader["gender"].ToString();
                        string email = reader["email"].ToString();
                        string role = reader["role"].ToString();
                        bool isActive = reader["isactive"] != DBNull.Value && Convert.ToBoolean(reader["isactive"]);
                        bool subscriptionActive = reader["subscriptionactive"] != DBNull.Value && Convert.ToBoolean(reader["subscriptionactive"]);

                        // Fetch the auth_token safely
                        string auth_token = reader["auth_token"] != DBNull.Value ? reader["auth_token"].ToString() : string.Empty;

                        reader.Close(); // Close the reader now that data is stored in variables

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

                            // Set session values
                            HttpContext.Session.SetInt32("Id", userId);
                            HttpContext.Session.SetString("Username", username);
                            HttpContext.Session.SetString("profilepic", profilepic);
                            HttpContext.Session.SetString("email", email);
                            HttpContext.Session.SetString("dob", dob);
                            HttpContext.Session.SetString("gender", gender);
                            HttpContext.Session.SetString("UserRole", role);
                            HttpContext.Session.SetString("SubscriptionActive", subscriptionActive.ToString());

                            // Retrieve the unique device ID based on the platform
                            string deviceId = GetUniqueDeviceId();

                            // Check if device ID already exists in auth_token
                            if (!string.IsNullOrEmpty(auth_token))
                            {
                                // Split existing tokens into a list
                                var existingTokens = new List<string>(auth_token.Split(','));

                                // Add the new device ID if it doesn't already exist
                                if (!existingTokens.Contains(deviceId))
                                {
                                    existingTokens.Add(deviceId);
                                }

                                // Reconstruct the auth_token
                                auth_token = string.Join(",", existingTokens);
                            }
                            else
                            {
                                // If no existing tokens, use the new device ID
                                auth_token = deviceId;
                            }

                            // Update the auth_token in the database
                            string updateTokenQuery = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";

                            using (SqlCommand updateCmd = new SqlCommand(updateTokenQuery, con))
                            {
                                updateCmd.Parameters.AddWithValue("@AuthToken", auth_token); // Use the updated auth token here
                                updateCmd.Parameters.AddWithValue("@UserId", userId);
                                updateCmd.ExecuteNonQuery();
                            }

                            // Set auth token in a secure, HTTP-only cookie
                            HttpContext.Response.Cookies.Append("AuthToken", deviceId, new CookieOptions
                            {
                                HttpOnly = true,
                                Expires = DateTimeOffset.UtcNow.AddDays(30),
                                Secure = true
                            });

                            // Redirect based on user role
                            if (role == "admin")
                            {
                                return RedirectToPage("/Deshbord");
                            }
                            else if (role == "user")
                            {
                                return RedirectToPage("/Home");
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

                    con.Close(); // Ensure connection is closed after use
                }
            }
            return Page();
        }


        private string GetUniqueDeviceId()
        {
            string deviceId = string.Empty;

            // Detect the platform (this is a simplified example)
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) // Windows
            {
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            deviceId = obj["SerialNumber"]?.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred while retrieving the device ID for Windows: " + ex.Message);
                }
            }
            else if (IsAndroid()) // Check for Android
            {
                // Implement Android-specific logic here, e.g., retrieving the Android ID
                deviceId = GetAndroidDeviceId(); // You can call your method to get Android ID here
            }
            else if (IsIOS()) // Check for iOS
            {
                // Implement iOS-specific logic here, e.g., retrieving the iOS ID
                deviceId = GetIOSDeviceId(); // You can call your method to get iOS ID here
            }

            // Fallback if device ID could not be retrieved
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString(); // Use a GUID as a fallback device ID
            }

            return deviceId;
        }

        // Check if the app is running on Android
        private bool IsAndroid()
        {
            return DeviceInfo.Platform == DevicePlatform.Android;
        }

        // Check if the app is running on iOS
        private bool IsIOS()
        {
            return DeviceInfo.Platform == DevicePlatform.iOS;
        }

        // Method to get Android ID (implement the logic as required)
        private string GetAndroidDeviceId()
        {
            // Add Android ID retrieval logic here, if applicable
            return "AndroidDeviceID"; // Placeholder logic
        }

        // Method to get iOS ID (implement the logic as required)
        private string GetIOSDeviceId()
        {
            // Add iOS ID retrieval logic here, if applicable
            return "iOSDeviceID"; // Placeholder logic
        }





    }
}
