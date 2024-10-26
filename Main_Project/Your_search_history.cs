using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Netflix.Models;
using System.Collections.Generic;

using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Main_Project
{
    public class Your_search_history
    {
        private readonly RequestDelegate _next;
        private readonly string connectionString;
        private readonly IConfiguration _configuration;

        public Your_search_history(RequestDelegate next, string connectionString, IConfiguration configuration)
        {
            _next = next;
            this.connectionString = connectionString;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            int? sessionUserId = httpContext.Session.GetInt32("Id");
            string connectionString = _configuration.GetConnectionString("NetflixDatabase");


            if (sessionUserId.HasValue) // Check if the user ID is present
            {
                List<Search_history> searchHistoryList = new List<Search_history>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT TOP 5 * FROM Search_history WHERE userid = @UserId ORDER BY searchDateTime DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", sessionUserId.Value);

                        con.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var searchHistoryItem = new Search_history
                                {
                                    id = dr.GetInt32(0),
                                    userid = dr.IsDBNull(1) ? (int?)null : dr.GetInt32(1),
                                    searchtext = dr.IsDBNull(2) ? null : dr.GetString(2),
                                    searchDateTime = dr.GetDateTime(3)
                                };
                                searchHistoryList.Add(searchHistoryItem);
                            }
                        }
                    }
                }

                httpContext.Items["SearchHistory"] = searchHistoryList;
            }

            await _next(httpContext);
        }
    }

    public static class Your_search_historyExtensions
    {
        public static IApplicationBuilder UseYour_search_history(this IApplicationBuilder builder, string connectionString)
        {
            return builder.UseMiddleware<Your_search_history>(connectionString);
        }
    }
}
