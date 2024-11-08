using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Main_Project.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LogoutModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGetLogout()
        {
            int userId = HttpContext.Session.GetInt32("Id") ?? 0; // Fetch the user ID from session

            if (userId > 0)
            {
                string connectionString = _configuration.GetConnectionString("NetflixDatabase");

                // Get the device ID from the cookie
                string deviceId = Request.Cookies["deviceUniqueId"];

                if (!string.IsNullOrEmpty(deviceId))
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        // Retrieve current auth_token value from the database
                        string selectQuery = "SELECT auth_token FROM User_data WHERE id = @UserId";
                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
                        {
                            selectCmd.Parameters.AddWithValue("@UserId", userId);
                            string authToken = selectCmd.ExecuteScalar()?.ToString() ?? string.Empty;

                            // Remove the specific device ID from auth_token
                            if (authToken.Contains(deviceId))
                            {
                                var updatedAuthToken = string.Join(",", authToken
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(token => token != deviceId));

                                // Update the auth_token in the database
                                string updateQuery = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";
                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                {
                                    updateCmd.Parameters.AddWithValue("@AuthToken", string.IsNullOrEmpty(updatedAuthToken) ? DBNull.Value : (object)updatedAuthToken);
                                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }

            // Clear session data and delete the device ID cookie
            HttpContext.Session.Clear();
            Response.Cookies.Delete("deviceUniqueId");

            return RedirectToPage("/Index"); // Redirect to the desired page after logout
        }
    }
}
