using System;
using System.Collections.Generic;

namespace Main_Project.Models;

public partial class UserDatum
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool Isactive { get; set; }

    public string Price { get; set; } = null!;

    public string Datetime { get; set; } = null!;

    public string Duration { get; set; } = null!;

    public string Cardholdername { get; set; } = null!;

    public string Cardnumber { get; set; } = null!;

    public string Cvv { get; set; } = null!;

    public string Role { get; set; } = null!;

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();
}
