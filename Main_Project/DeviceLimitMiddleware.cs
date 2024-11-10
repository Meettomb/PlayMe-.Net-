using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Main_Project
{
    public class DeviceLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _connectionString;

        public DeviceLimitMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.Session.GetInt32("Id");
            var isAdmin = context.Session.GetString("Role") == "admin"; // Check if the user is an admin

            if (userId.HasValue && !isAdmin) // Only apply this logic to non-admin users
            {
                var authToken = GetAuthToken(userId.Value);
                var maxLoginCount = GetMaxLoginCount(userId.Value); // Fetch from user subscription details

                // Ensure that authToken is not null and check if the limit is exceeded
                if (!string.IsNullOrEmpty(authToken) && authToken.Split(',').Length > maxLoginCount)
                {
                    // Logic to logout devices, remove excess device IDs from auth_token
                    string updatedAuthToken = GetUpdatedAuthToken(authToken, maxLoginCount);

                    // Log for debugging
                    Log($"Updated auth token: {updatedAuthToken}");

                    // Update the user's auth token in the database
                    UpdateUserAuthToken(userId.Value, updatedAuthToken);

                    // Update the session with the new token
                    context.Session.SetString("auth_token", updatedAuthToken);

                    // Delete the device's token from cookies (assuming the cookie is named "auth_token")
                    context.Response.Cookies.Delete("auth_token");

                    // Also, delete the last device's token from the cookies if it was stored in there
                    string[] tokenParts = updatedAuthToken.Split(',');
                    if (tokenParts.Length > 0)
                    {
                        string lastToken = tokenParts.Last();
                        // Delete the last token's cookie (you may adjust the cookie name based on your implementation)
                        context.Response.Cookies.Delete($"auth_token_{lastToken}");
                    }

                    // Log out the user by clearing the session and authentication cookies
                    context.Session.Clear();
                    context.Response.Cookies.Delete("auth_token");

                    // Redirect user to the index page if the login limit is exceeded
                    context.Response.Redirect("/Index");
                    return; // Stop further processing after redirect
                }
            }

            await _next(context); // Continue processing the request
        }

        private string GetAuthToken(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT auth_token FROM User_data WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    return cmd.ExecuteScalar()?.ToString() ?? string.Empty;
                }
            }
        }

        private int GetMaxLoginCount(int userId)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"SELECT planedetail FROM Subscription WHERE id = 
                                 (SELECT subid FROM User_data WHERE id = @UserId)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string planDetails = reader["planedetail"].ToString();
                        if (planDetails.Contains("User login at a time"))
                        {
                            string[] details = planDetails.Split('+');
                            foreach (string detail in details)
                            {
                                if (detail.Contains("User login at a time"))
                                {
                                    int.TryParse(new string(detail.Where(char.IsDigit).ToArray()), out int maxLoginCount);
                                    return maxLoginCount;
                                }
                            }
                        }
                    }
                }
            }

            return 1; // Default to 1 if not found
        }

        private string GetUpdatedAuthToken(string authToken, int maxLoginCount)
        {
            var deviceList = authToken.Split(',').ToList();
            while (deviceList.Count > maxLoginCount)
            {
                deviceList.RemoveAt(deviceList.Count - 1);  // Remove the last device ID when limit is exceeded
            }

            return string.Join(",", deviceList);
        }

        private void UpdateUserAuthToken(int userId, string updatedAuthToken)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@AuthToken", updatedAuthToken);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void Log(string message)
        {
            // This method could log to a file or database, but for now, it prints to the console for debugging
            Console.WriteLine(message);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DeviceLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseDeviceLimitMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DeviceLimitMiddleware>();
        }
    }
}
