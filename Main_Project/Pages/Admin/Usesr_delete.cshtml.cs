using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System;

namespace Netflix.Pages.Admin
{
    public class Usesr_deleteModel : PageModel
    {
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        private readonly string _connectionString;
        public Usesr_deleteModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public IActionResult OnGet(int id)
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

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE User_data SET isactive = @isActive WHERE id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@isActive", false);
                        cmd.Parameters.AddWithValue("@Id", id);

                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        con.Close();

                        if (rowsAffected > 0)
                        {
                            return Redirect("/Admin/Login_data");
                        }
                        else
                        {
                            return NotFound(); // Handle case where user with given id was not found
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
    }
}
