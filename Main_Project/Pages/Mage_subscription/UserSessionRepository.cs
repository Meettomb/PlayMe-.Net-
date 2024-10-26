using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Netflix.Models;

namespace Main_Project.Pages.Mage_subscription
{
    public class UserSessionRepository
    {
        private readonly string _connectionString;

        public UserSessionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NetflixDatabase");
        }

        public void AddSession(UserSessions session) // Changed from UserSession to UserSessions
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO UserSessions (UserId, SubscriptionPlanId, LastLogin, IsWatching) VALUES (@UserId, @SubscriptionPlanId, @LastLogin, @IsWatching)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", session.UserId);
                command.Parameters.AddWithValue("@SubscriptionPlanId", session.SubscriptionPlanId);
                command.Parameters.AddWithValue("@LastLogin", session.LastLogin);
                command.Parameters.AddWithValue("@IsWatching", session.IsWatching);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateWatchStatus(int userId, bool isWatching) // Change userId type to match UserId in UserSessions
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE UserSessions SET IsWatching = @IsWatching WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@IsWatching", isWatching);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void RemoveSession(int userId) // Change userId type to match UserId in UserSessions
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM UserSessions WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int GetCurrentLogins(int userId) // Change userId type to match UserId in UserSessions
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM UserSessions WHERE UserId = @UserId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        public int GetCurrentWatchers(int userId) // Change userId type to match UserId in UserSessions
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT COUNT(*) FROM UserSessions WHERE UserId = @UserId AND IsWatching = 1";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }
    }
}
