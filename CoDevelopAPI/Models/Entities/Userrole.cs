using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Userrole
{
    public int Userroleid { get; set; }

    public int Userid { get; set; }

    public int Roleid { get; set; }

    public int? Assignedby { get; set; }

    public bool Isactive { get; set; }

    public virtual User? AssignedbyNavigation { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
