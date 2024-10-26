using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Netflix.Models;
using System.Collections.Generic;

namespace Main_Project.Pages.Movies
{
    public class Add_Movies_CategoryModel : PageModel
    {
        public List<Movie_category_table> category = new List<Movie_category_table>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }

        public IActionResult OnGet()
        {
            string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";

            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string query = "SELECT * FROM User_data WHERE email = @Email";
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
                                gender = reader["profilepic"].ToString();
                                dob = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }



            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                string query = "select * from Movie_category_table";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Movie_category_table category_Table = new Movie_category_table();
                            category_Table.categoryid = dr.GetInt32(0);
                            category_Table.moviecategory = dr.GetString(1);
                            category.Add(category_Table);
                        }
                        con.Close();
                    }
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                Movie_category_table category = new Movie_category_table();
                category.moviecategory = Request.Form["moviecategory"].ToString();
                string query = "Insert into Movie_category_table (moviecategory) values(@moviecategory)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@moviecategory", category.moviecategory);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToPage();
        }

        public IActionResult OnGetDelete(int categoryid)
        {
            string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
            using (SqlConnection con = new SqlConnection(connectionstring))
            {
                string query = "DELETE FROM Movie_category_table WHERE categoryid = @categoryid";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@categoryid", categoryid);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return RedirectToPage();
        }
    }
}
