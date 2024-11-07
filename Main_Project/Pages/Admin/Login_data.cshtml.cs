using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Netflix.Models;

namespace Netflix.Pages.Admin
{
    /*from Admin*/
    public class Login_dataModel : PageModel
    {
        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        private readonly string _connectionString;
        public Login_dataModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public IActionResult OnGet()
        {
         string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT username, dob, gender, profilepic FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                profilepic = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string quary = "select * from User_data where isactive='"+true+"'";
                using (SqlCommand cmd = new SqlCommand(quary, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            user_regi user = new user_regi();
                            user.userid = dr.GetInt32(0);
                            user.fullname = dr.GetString(1);
                            user.email = dr.GetString(2);
                            user.phone = dr.GetString(3);
                            user.password = dr.GetString(4);
                            user.isactive = dr.GetBoolean(5);
                            user.datetime3 = dr.GetString(7);
                            user.role = dr.GetString(10);
                            userlist.Add(user);
                        }
                        con.Close();
                    }
                }
            }
            return Page();
        }
    }
}
