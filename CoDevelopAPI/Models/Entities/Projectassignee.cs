using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Projectassignee
{
    public int Projectassigneeid { get; set; }

    public int Projectid { get; set; }

    public int Userid { get; set; }

    public int? Assignedby { get; set; }

    public DateTime? Assigneddate { get; set; }

    public virtual User? AssignedbyNavigation { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
