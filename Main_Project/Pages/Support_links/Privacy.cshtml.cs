using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace Main_Project.Pages.Support_links
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;
        private readonly string _connectionString;

        public PrivacyModel(ILogger<PrivacyModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public string UserName { get; set; }
        public string id { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public void OnGet()
        {
            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            // If session email is not null, fetch user data from the database
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string selectQuery = "SELECT * FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                id = reader["id"].ToString();
                                Email = sessionEmail; // Set email from session

                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }
        }

    }

}
