using Main_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Netflix.Models;
using System.Data;

namespace Main_Project.Pages.Feedbacks
{
    public class Feedback_pageModel : PageModel
    {
        private readonly IEmailService _emailService;

        public Feedback_pageModel(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Feedback_table feedback = new Feedback_table();

        string connectionstring = "Server=LAPTOP-2850PE29\\SQLEXPRESS;Database=NetflixData;Trusted_Connection=True;Encrypt=False";
        public string UserName { get; set; }
        public string id { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public void OnGet()
        {
            // Retrieve the email from session
            string sessionEmail = HttpContext.Session.GetString("email");

            // If session email is not null, fetch user data from the database
            if (!string.IsNullOrEmpty(sessionEmail))
            {
                using (SqlConnection con = new SqlConnection(connectionstring))
                {
                    string selectQuery = "SELECT id, username FROM User_data WHERE email = @Email";
                    using (SqlCommand cmd = new SqlCommand(selectQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", sessionEmail);
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserName = reader["username"].ToString();
                                id = reader["id"].ToString();
                                Email = sessionEmail; // Set email from session

                                UserRole = reader["role"].ToString();
                            }
                        }
                        con.Close();
                    }
                }
            }
        }


        public async Task<IActionResult> OnPost()
        {
            // Fetch username, email, and userid from form data
            feedback.username = Request.Form["username"];
            feedback.feedbackemail = Request.Form["email"];
            feedback.feedback = Request.Form["feedback"];

            // Convert userid from form data to int
            if (int.TryParse(Request.Form["userid"], out int userId))
            {
                feedback.userid = userId;
            }
            else
            {
                // Handle the case where conversion fails (e.g., log an error or return a validation message)
                TempData["ErrorMessage"] = "Invalid User ID.";
                return Page();
            }

            // Insert feedback into the database
            string insertQuery = "INSERT INTO Feedback_table (username, feedbackemail, feedback, userid) VALUES (@username, @FeedbackEmail, @Feedback, @userid)";
            using (SqlConnection connection = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@username", feedback.username);
                    cmd.Parameters.AddWithValue("@FeedbackEmail", feedback.feedbackemail);
                    cmd.Parameters.AddWithValue("@Feedback", feedback.feedback);
                    cmd.Parameters.AddWithValue("@userid", feedback.userid);

                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }

            // Send a confirmation email
            var subject = "Thank You for Your Feedback";
            var message = $"Dear {feedback.username},\n\nThank you for your feedback. We appreciate your input and will use it to improve our services.\n\nBest regards,\nThe PLAYME Team";
            bool emailSent = await _emailService.SendEmailAsync(feedback.feedbackemail, subject, message);

            // Feedback to the user
            if (emailSent)
            {
                TempData["SuccessMessage"] = "Thank you for your feedback!";
            }
            else
            {
                TempData["ErrorMessage"] = "There was an issue sending the confirmation email, but your feedback was received.";
            }

            // Clear the feedback field while retaining username and email
            feedback.feedback = string.Empty;
            UserName = feedback.username;
            Email = feedback.feedbackemail;
            id = feedback.userid.ToString();

            return Page(); // Stay on the same page after submission
        }


    }
}
