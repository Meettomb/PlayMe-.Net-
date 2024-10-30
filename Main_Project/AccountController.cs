using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace YourNamespace.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult AutoLogin()
        {
            if (Request.Cookies.TryGetValue("AuthToken", out string authToken))
            {
                string connectionString = _configuration.GetConnectionString("NetflixDatabase");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT id, username, email FROM User_data WHERE auth_token = @AuthToken AND isactive = 1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@AuthToken", authToken);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["id"]);
                            string username = reader["username"].ToString();
                            string email = reader["email"].ToString();

                            // Restore session data
                            HttpContext.Session.SetInt32("UserId", userId);
                            HttpContext.Session.SetString("Username", username);
                            HttpContext.Session.SetString("Email", email);

                            con.Close();
                            return RedirectToPage("/Home"); // Redirect after restoring session
                        }
                        con.Close();
                    }
                }
            }

            return RedirectToPage("/Index");
        }
    }

}
