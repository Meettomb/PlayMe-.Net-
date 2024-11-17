using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Main_Project.Pages.User_Profile_manage
{
    public class All_connected_devicesModel : PageModel
    {
        private readonly string _connectionString;

        public string Email { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }
        public string UserRole { get; set; }
        public List<string> ConnectedDevices { get; set; } = new List<string>();

        public All_connected_devicesModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public IActionResult OnGet()
        {
            int? userId = HttpContext.Session.GetInt32("Id");

            // Redirect to Index if userId is null (user not logged in)
            if (!userId.HasValue)
            {
                return RedirectToPage("/Index");
            }

            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            // If session email is not null, fetch user data from the database
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string selectQuery = "SELECT username, role, auth_token FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                Email = sessionEmail; // Set email from session
                                UserRole = reader["role"].ToString();

                                // Retrieve the auth_token to get connected devices
                                var authToken = reader["auth_token"]?.ToString();
                                if (!string.IsNullOrEmpty(authToken))
                                {
                                    // Split the auth_token by commas to get individual device IDs
                                    ConnectedDevices = new List<string>(authToken.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                                }
                            }
                        }
                    }
                }
            }

            return Page();
        }


        public IActionResult OnPostDeleteDevice(string deviceId)
        {
            // Ensure the user is logged in
            int? userId = HttpContext.Session.GetInt32("Id");
            if (!userId.HasValue)
            {
                return RedirectToPage("/Index"); // Redirect if not logged in
            }

            // Remove the device ID from the auth_token field for the user
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string updateQuery = @"
            UPDATE User_data 
            SET auth_token = 
                CASE 
                    WHEN CHARINDEX(@DeviceId + ',', auth_token) > 0 
                        THEN REPLACE(auth_token, @DeviceId + ',', '')
                        ELSE REPLACE(auth_token, ',' + @DeviceId, '')
                END
            WHERE id = @UserId AND auth_token LIKE '%' + @DeviceId + '%'";

                using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            // Clear the device cookie on the current device
            HttpContext.Response.Cookies.Delete("deviceId");

            // Clear the session to log out the user on the current device
            HttpContext.Session.Clear();

            // Redirect to the Index page or appropriate page to refresh the device list
            return RedirectToPage("/Index");
        }



    }
}
