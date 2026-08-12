using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Task
{
    public int Taskid { get; set; }

    public int Projectid { get; set; }

    public string Taskname { get; set; } = null!;

    public DateTime Startdatetime { get; set; }

    public DateTime Enddatetime { get; set; }

    public string Status { get; set; } = null!;

    public int? Assignedto { get; set; }

    public int? Assignedby { get; set; }

    public DateTime? Createddate { get; set; }

    public virtual User? AssignedbyNavigation { get; set; }

    public virtual User? AssignedtoNavigation { get; set; }

    public virtual Project Project { get; set; } = null!;
}
