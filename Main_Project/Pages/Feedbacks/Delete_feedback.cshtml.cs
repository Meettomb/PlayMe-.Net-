using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace Main_Project.Pages.Feedbacks
{
    public class Delete_feedbackModel : PageModel
    {

        private readonly string _connectionString;
        public Delete_feedbackModel(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }


        [BindProperty]
        public int FeedbackId { get; set; }
        public string UserName { get; set; }
        public string FeedbackEmail { get; set; }
        public string Feedback { get; set; }
        public string ProfilePic { get; set; }

        public string email { get; set; }
        public string Email { get; set; }
        public string gender { get; set; }
        public string dob { get; set; }
        public string profilepic { get; set; }

        public void OnGet(int id)
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
                                gender = reader["profilepic"].ToString();
                                dob = reader["profilepic"].ToString();
                                email = sessionEmail; // Set email from session
                            }
                        }
                        con.Close();
                    }
                }
            }

            FeedbackId = id;
            FetchFeedbackDetails();
        }
        private void FetchFeedbackDetails()
        {
           
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                string query = @"
                SELECT f.username, f.feedbackemail, f.feedback, f.userid, 
                       u.email, u.profilepic, u.username 
                FROM Feedback_table f
                INNER JOIN User_data u ON f.userid = u.id
                WHERE f.feedbackid = @FeedbackId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FeedbackId", FeedbackId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            UserName = reader["username"].ToString();
                            FeedbackEmail = reader["feedbackemail"].ToString();
                            Feedback = reader["feedback"].ToString();
                            Email = reader["email"].ToString();
                            ProfilePic = reader["profilepic"].ToString();
                        }
                    }
                }
                con.Close();
            }
        }
        public IActionResult OnPost()
        {
            if (FeedbackId <= 0)
            {
                return RedirectToPage("/Feedbacks/Show_all_feedback");
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                string query = "DELETE FROM Feedback_table WHERE feedbackid = @FeedbackId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FeedbackId", FeedbackId);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        // Log or handle the case where no rows were affected
                        // Example:
                        Console.WriteLine($"No rows were deleted for FeedbackId: {FeedbackId}");
                    }
                }

                con.Close();
            }

            return RedirectToPage("/Feedbacks/Show_all_feedback");
        }

    }


}