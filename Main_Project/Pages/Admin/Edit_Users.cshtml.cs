using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;

namespace Netflix.Pages.Admin
{
    public class Edit_UsersModel : PageModel
    {
        public user_regi user = new user_regi();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        private readonly string _connectionString;
        public Edit_UsersModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        public IActionResult OnGet()
        {
            int? userId = HttpContext.Session.GetInt32("Id");
            string role = HttpContext.Session.GetString("UserRole");
            if (!userId.HasValue | role != "admin")
            {
                return Redirect("/");
            }
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

            int userid = Convert.ToInt32(Request.Query["userid"].ToString());
            DataSet ds = new DataSet();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "select * from User_data where id=" + userid + "";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        da.Fill(ds);
                        con.Close();
                    }
                }

                user.userid = Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());
               
                user.isactive = Convert.ToBoolean(ds.Tables[0].Rows[0][5].ToString());
                user.role = ds.Tables[0].Rows[0][10].ToString();
            }
            return Page();
        }

        public IActionResult OnPost()
        {

            int userid = Convert.ToInt32(Request.Form["userid"].ToString());

            string isactive = Request.Form["isactive"].ToString();


            string role = Request.Form["role"].ToString();


            if (isactive != "Select")
            {
                isactive = Request.Form["isactive"].ToString();
            }
            else
                isactive = Request.Form["currentstatus"].ToString();

            string query = "update User_data set isactive = '" + isactive + "', role = '"+ role +"' where id = " + userid + " ";
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return Redirect("/Admin/Login_data");
        }
    }
}
