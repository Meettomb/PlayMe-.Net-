using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Data;
using Netflix.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Main_Project.Pages.Admin
{
    public class DetailsModel : PageModel
    {
        public user_regi user { get; set; } = new user_regi();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        private readonly string _connectionString;
        public DetailsModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public void OnGet(int userid)
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
                string query = "SELECT * FROM User_data WHERE id=@UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user.userid = reader.GetInt32(0);
                            user.fullname = reader.GetString(1);
                            user.email = reader.GetString(2);
                            user.phone = reader.GetString(3);
                            user.password = reader.GetString(4);
                            user.isactive = reader.GetBoolean(5);

                            user.price3 = reader.GetString(6);
                            user.datetime3 = reader.GetString(7);
                            user.duration3 = reader.GetString(8);
                            user.paymentmethod = reader.GetString(9);
                          
                            user.role = reader.GetString(10);
                            user.logintime = reader.GetBoolean(11);

                            if (!reader.IsDBNull(12))
                            {
                                user.profilepic = reader.GetString(12);
                            }
                            else
                            {
                                user.profilepic = null; // or default value
                            }

                            if (!reader.IsDBNull(13))
                            {
                                user.gender = reader.GetString(13);
                            }
                            else
                            {
                                user.gender = null; // or default value
                            }
                            if (!reader.IsDBNull(14))
                            { 
                                user.dob = reader.GetString(14);
                            }
                            else
                            {
                                user.dob = null;
                            }
                        }
                    }
                    con.Close();
                }
            }
        }
    }
}
