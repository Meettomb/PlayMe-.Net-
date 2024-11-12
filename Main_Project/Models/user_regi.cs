using Main_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace Netflix.Models
{
    public class user_regi
    {
        [Key]
        public int userid { get; set; }
        public string fullname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string password { get; set; }
        public bool isactive { get; set; }

        public string price3 { get; set; }
        public string datetime3 { get; set; }
        public string duration3 { get; set; }

        public string paymentmethod { get; set; }
        public string role { get; set; }
        public bool logintime { get; set; }
        public string? profilepic { get; set; }
        public string? gender { get; set; }
        public string? dob { get; set; }
        public bool? subscriptionactive { get; set; }
        public DateOnly date { get; set; }

        public bool? emailsent { get; set; }
        public int? subid { get; set; }
        public string? auth_token { get; set; }
        public bool? autorenew { get; set; }

    }
    public class Revenue
    {
        [Key]
        public int userid { get; set; }

        public string email { get; set; }


        public string price3 { get; set; }
        public string datetime3 { get; set; }
        public string duration3 { get; set; }

        public string paymentmethod { get; set; }

        public bool? subscriptionactive { get; set; }
        public DateOnly date { get; set; }

    }

    public class question_answer
    {
        [Key]
        public int id { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }

    public class Movie_category_table
    {
        public int categoryid { get; set; }
        public string moviecategory { get; set; }
    }

    public class Feedback_table
    {
        public int feedbackid { get; set; }
        public string feedbackemail { get; set; }
        public string feedback { get; set; }
        public string username { get; set; }
        public int userid { get; set; }

        // Additional properties from the User_data table
        public string Email { get; set; }  // Add this line
        public string profilepic { get; set; }  // Add this line
        public string UserName { get; set; }  // Add this line

    }

    public class subscription
    {
        public int id { get; set; }
        public int price { get; set; }
        public int planeduration { get; set; }
        public string planedetail { get; set; }
        public string? planename { get; set; }
    }


    public class Search_history
    {
        public int id { get; set; }
        public int? userid { get; set; }
        public string searchtext { get; set; }
        public DateTime searchDateTime { get; set; }
    }

    public class User_activity
    {
        public int id { get; set; }
        public int? userid { get; set; }
        public string movietype { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class Watch_history { 
        public int id { get; set; }
        public int userid { get; set; }
        public int movieid { get; set; }
        public string watchtime { get; set; }
        public string toteltime { get; set; }
        public DateOnly lastwatchtime { get; set; }
        public bool moviecomplet { get; set; }

        // Navigation property
        public MoviesTable Movie { get; set; }
    }
    
    public class UserSessions
    {
        public int id { get; set; }
        public int UserId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public int UserActiveNumbers { get; set; }
    }

    public class Movie_like
    {
        public int id { get; set; }
        public int userid { get; set; }
        public int movieid { get; set; }

        // Navigation property
        public user_regi uid { get; set; }
        public MoviesTable mid { get; set; }
    } 
    public class Movie_dislike
    {
        public int id { get; set; }
        public int userid { get; set; }
        public int movieid { get; set; }

        // Navigation property
        public user_regi uid { get; set; }
        public MoviesTable mid { get; set; }
    }

    public class Profile_pic
    {
        public int Id { get; set; }

        [BindNever]
        public string? Pics { get; set; }
        public string Groups {  get; set; }
    }
}