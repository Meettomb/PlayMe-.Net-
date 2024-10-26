using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using Main_Project.Models;
using Main_Project.Services;
using Microsoft.Extensions.Options;

namespace Main_Project.Pages
{
    public class IndexModel : PageModel
    {
        public List<question_answer> questio_answer_list { get; set; } = new List<question_answer>();

        public IndexModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        private readonly string _connectionString;
        public int? UserId { get; set; } 

        public IActionResult OnGet()
        {
            UserId = HttpContext.Session.GetInt32("Id");

            if (UserId.HasValue)
            {
                return RedirectToPage("/Home");
            }

            // Fetch data from the database
            GetQuestionsAndAnswers();

            return Page();
        }

        private void GetQuestionsAndAnswers()
        {
            string connectionString = "Your_Connection_String_Here";
            string query = "SELECT * FROM Questions";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        question_answer qa = new question_answer
                        {
                            id = reader.GetInt32(0),
                            question = reader.GetString(1),
                            answer = reader.GetString(2)
                        };
                        questio_answer_list.Add(qa);
                    }
                }
            }
        }

    }
}
