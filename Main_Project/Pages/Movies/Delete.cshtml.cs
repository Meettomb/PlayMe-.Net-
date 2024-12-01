using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Main_Project.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace Main_Project.Pages.Movies
{
    public class DeleteModel : PageModel
    {
        private readonly Main_Project.Models.NetflixDataContext _context;
        private readonly string _connectionString;

        public DeleteModel(Main_Project.Models.NetflixDataContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        [BindProperty]
        public MoviesTable MoviesTable { get; set; } = default!;
        public string UserName { get; set; }
        public string email { get; set; }
        public string profilepic { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
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

            if (id == null)
            {
                return NotFound();
            }

            var moviestable = await _context.MoviesTables.FirstOrDefaultAsync(m => m.Movieid == id);

            if (moviestable == null)
            {
                return NotFound();
            }
            else
            {
                MoviesTable = moviestable;
            }
            return Page();
        }


        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound("Movie ID is required.");
            }

            string connectionString = _connectionString; // Ensure this is correctly set in your configuration.

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Begin a transaction
                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete from Watch_history
                            string deleteWatchHistoryQuery = "DELETE FROM Watch_history WHERE movieid = @MovieId";
                            using (SqlCommand deleteWatchHistoryCommand = new SqlCommand(deleteWatchHistoryQuery, connection, transaction))
                            {
                                deleteWatchHistoryCommand.Parameters.AddWithValue("@MovieId", id.Value);
                                int rowsDeleted = await deleteWatchHistoryCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Deleted {rowsDeleted} rows from Watch_history.");
                            }

                            // Delete from Watch_list
                            string deleteWatchListQuery = "DELETE FROM Watch_list WHERE Movieid = @MovieId";
                            using (SqlCommand deleteWatchListCommand = new SqlCommand(deleteWatchListQuery, connection, transaction))
                            {
                                deleteWatchListCommand.Parameters.AddWithValue("@MovieId", id.Value);
                                int rowsDeleted = await deleteWatchListCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Deleted {rowsDeleted} rows from Watch_list.");
                            }

                            // Delete from Movie_like
                            string deleteMovieLikeQuery = "DELETE FROM Movie_like WHERE movieid = @MovieId";
                            using (SqlCommand deleteMovieLikeCommand = new SqlCommand(deleteMovieLikeQuery, connection, transaction))
                            {
                                deleteMovieLikeCommand.Parameters.AddWithValue("@MovieId", id.Value);
                                int rowsDeleted = await deleteMovieLikeCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Deleted {rowsDeleted} rows from Movie_like.");
                            }

                            // Delete from Movie_dislike
                            string deleteMovieDislikeQuery = "DELETE FROM Movie_dislike WHERE movieid = @MovieId";
                            using (SqlCommand deleteMovieDislikeCommand = new SqlCommand(deleteMovieDislikeQuery, connection, transaction))
                            {
                                deleteMovieDislikeCommand.Parameters.AddWithValue("@MovieId", id.Value);
                                int rowsDeleted = await deleteMovieDislikeCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Deleted {rowsDeleted} rows from Movie_dislike.");
                            }

                            // Delete from MoviesTables
                            string deleteMoviesQuery = "DELETE FROM Movies_Table WHERE Movieid = @MovieId";
                            using (SqlCommand deleteMoviesCommand = new SqlCommand(deleteMoviesQuery, connection, transaction))
                            {
                                deleteMoviesCommand.Parameters.AddWithValue("@MovieId", id.Value);
                                int rowsDeleted = await deleteMoviesCommand.ExecuteNonQueryAsync();
                                Console.WriteLine($"Deleted {rowsDeleted} rows from MoviesTables.");
                            }

                            // Commit the transaction
                            transaction.Commit();
                            Console.WriteLine("Transaction committed successfully.");
                        }
                        catch (Exception ex)
                        {
                            // Rollback transaction on error
                            transaction.Rollback();
                            Console.WriteLine($"Transaction rolled back. Error: {ex.Message}");
                            return StatusCode(500, "An error occurred while deleting the movie and related data.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"General Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(500, "An unexpected error occurred while processing your request.");
            }

            // Redirect to the index page after successful deletion
            return RedirectToPage("./Index");
        }


    }
}
