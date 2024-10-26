using System;
using System.Collections.Generic;

namespace Main_Project.Models;

public partial class WatchList
{
    public int Id { get; set; }

    public int Userid { get; set; }

    public int Movieid { get; set; }

    public virtual MoviesTable Movie { get; set; } = null!;

    public virtual UserDatum User { get; set; } = null!;
}
