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
            int userId = HttpContext.Session.GetInt32("Id") ?? 0;
            Console.WriteLine($"User ID: {userId}");

            if (userId > 0)
            {
                string connectionString = _configuration.GetConnectionString("NetflixDatabase");
                string deviceId = Request.Cookies["deviceUniqueId"];
                Console.WriteLine($"Device ID: {deviceId}");

                if (!string.IsNullOrEmpty(deviceId))
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        string selectQuery = "SELECT auth_token FROM User_data WHERE id = @UserId";
                        using (SqlCommand selectCmd = new SqlCommand(selectQuery, con))
                        {
                            selectCmd.Parameters.AddWithValue("@UserId", userId);
                            string authToken = selectCmd.ExecuteScalar()?.ToString() ?? string.Empty;

                            Console.WriteLine($"Auth Token Before Update: {authToken}");

                            if (authToken.Contains(deviceId))
                            {
                                var updatedAuthToken = string.Join(",", authToken
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(token => token != deviceId));

                                string updateQuery = "UPDATE User_data SET auth_token = @AuthToken WHERE id = @UserId";
                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                {
                                    updateCmd.Parameters.AddWithValue("@AuthToken", string.IsNullOrEmpty(updatedAuthToken) ? DBNull.Value : (object)updatedAuthToken);
                                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                                    updateCmd.ExecuteNonQuery();
                                }

                                Console.WriteLine($"Auth Token After Update: {updatedAuthToken}");
                            }
                        }
                    }
                }
            }

            HttpContext.Session.Clear();
            Response.Cookies.Delete("deviceUniqueId");
            Console.WriteLine("Session cleared and cookie deleted.");

            return Redirect("/Index");
        }

    }
}
