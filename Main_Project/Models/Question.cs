using System;
using System.Collections.Generic;

namespace Main_Project.Models;

public partial class Question
{
    public int Id { get; set; }

    public string Question1 { get; set; } = null!;

    public string Answer { get; set; } = null!;
}
