using Netflix.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Main_Project.Models;

[Table("Movies_table")]
public partial class MoviesTable
{
    public int Movieid { get; set; }

    public string Movieposter { get; set; } = null!;

    public string? Movie { get; set; }

    public string Movieposter2 { get; set; } = null!;

    public string Discription { get; set; } = null!;

    public string? Moviename { get; set; }

    public string? Moviecast { get; set; }

    public string? Movietype { get; set; }

    public string? Moviereleaseyear { get; set; }

    public int? Movielike { get; set; }

    public string? Moviedirector { get; set; }
    public string? Movierating { get; set; }
    public string? Movielanguage { get; set; }
    public string? Movieagerestrictions { get; set; }
    public string? Category { get; set; }
    public string? Content_advisory { get; set; }
    public string? Trailer { get; set; }

    public virtual ICollection<WatchList> WatchLists { get; set; } = new List<WatchList>();
    public virtual ICollection<Watch_history> WatchHistories { get; set; } = new List<Watch_history>();

}

