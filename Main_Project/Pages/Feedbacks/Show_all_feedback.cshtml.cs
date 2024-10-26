using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;

namespace Main_Project.Pages.Feedbacks
{
    public class Show_all_feedbackModel : PageModel
    {
        string connectionString = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";


        public List<user_regi> userlist = new List<user_regi>();
        public List<Feedback_table> feedbacklist = new List<Feedback_table>();
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public IActionResult OnGet()
        {

            string sessionEmail = HttpContext.Session.GetString("email");
            // Fetch the user's username from the database using their email
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
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
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Fetch feedback data and user details
                string feedbackQuery = @"
                SELECT f.feedbackid, f.username, f.feedbackemail, f.feedback, f.userid, 
                       u.email, u.profilepic, u.username 
                FROM Feedback_table f
                INNER JOIN User_data u ON f.userid = u.id
                ORDER BY f.feedbackid ASC"; 

                using (SqlCommand cmd = new SqlCommand(feedbackQuery, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var feedback = new Feedback_table
                            {
                                feedbackid = dr.IsDBNull(0) ? 0 : dr.GetInt32(0),
                                username = dr.IsDBNull(1) ? string.Empty : dr.GetString(1),
                                feedbackemail = dr.IsDBNull(2) ? string.Empty : dr.GetString(2),
                                feedback = dr.IsDBNull(3) ? string.Empty : dr.GetString(3),
                                userid = dr.IsDBNull(4) ? 0 : dr.GetInt32(4),
                                Email = dr.IsDBNull(5) ? string.Empty : dr.GetString(5),
                                profilepic = dr.IsDBNull(6) ? string.Empty : dr.GetString(6),
                                UserName = dr.IsDBNull(7) ? string.Empty : dr.GetString(7)
                            };

                            feedbacklist.Add(feedback);
                        }
                    }


                }
                return Page();
            }
        }


        [HttpGet]
        public IActionResult Delete_feedback(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM Feedback_table WHERE feedbackid = @FeedbackId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FeedbackId", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Optional: Check if deletion was successful
                    if (rowsAffected > 0)
                    {
                        TempData["Message"] = "Feedback deleted successfully!";
                    }
                    else
                    {
                        TempData["Message"] = "Feedback not found.";
                    }
                }
            }

            return Page(); // Redirect to the list or any other page
        }

    }
}
