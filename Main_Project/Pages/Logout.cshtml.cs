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

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    // Set auth_token to NULL for the user ID in the User_data table
                    string query = "UPDATE User_data SET auth_token = NULL WHERE id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Optionally, you can also remove the device ID from any relevant table
                // If you have a separate table for devices, you would include that logic here
            }

            // Clear session data and delete the auth token cookie
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AuthToken");

            return RedirectToPage("/Index"); // Redirect to the desired page after logout
        }
    }
}
