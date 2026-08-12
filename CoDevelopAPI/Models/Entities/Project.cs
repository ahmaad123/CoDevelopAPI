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

    public DateTime Deadline { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public int? Createdby { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual User? CreatedbyNavigation { get; set; }

    public virtual ICollection<Projectassignee> Projectassignees { get; set; } = new List<Projectassignee>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
