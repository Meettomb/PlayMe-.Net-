using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Main_Project.Models;
using Netflix.Models;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages.Question_answer
{
    public class Question_answer_CreateModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;

        private readonly string _connectionString;
        public Question_answer_CreateModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
        
        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
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

            return Page();
        }

        [BindProperty]
        public Question Question { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Question.Add(Question);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Question_answer/Question_answer_Create");
        }
    }
}
