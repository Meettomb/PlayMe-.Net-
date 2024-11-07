using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using Netflix.Models;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages.Question_answer
{
    public class DeleteModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;


        public List<user_regi> userlist = new List<user_regi>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }

        private readonly string _connectionString;
        public DeleteModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }
      

        [BindProperty]
        public Question Question { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question.FirstOrDefaultAsync(m => m.Id == id);

            if (question == null)
            {
                return NotFound();
            }
            else
            {
                Question = question;
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


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Question.FindAsync(id);
            if (question != null)
            {
                Question = question;
                _context.Question.Remove(Question);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
