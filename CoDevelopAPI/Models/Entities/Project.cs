using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Project
{
    public int Projectid { get; set; }

    public string Projectname { get; set; } = null!;

    public int Clientid { get; set; }

    public string Developer { get; set; } = null!;

    public string Manager { get; set; } = null!;

    public int? Progress { get; set; }

    public decimal Budget { get; set; }

    public DateOnly Deadline { get; set; }

    public virtual Client Client { get; set; } = null!;
}
