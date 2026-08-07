using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class User
{
    public int Userid { get; set; }

    public string Email { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public int? Roleid { get; set; }

    public bool? IsActive { get; set; }

    public int? Phone { get; set; }

    public string? Department { get; set; }

    public string Passwordhash { get; set; } = null!;

    public virtual Role? Role { get; set; }

    public virtual ICollection<Rolepermission> Rolepermissions { get; set; } = new List<Rolepermission>();

    public virtual ICollection<Ticket> TicketAssignees { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketClients { get; set; } = new List<Ticket>();

    public virtual ICollection<Ticket> TicketReporters { get; set; } = new List<Ticket>();

    public virtual ICollection<Userrole> UserroleAssignedbyNavigations { get; set; } = new List<Userrole>();

    public virtual ICollection<Userrole> UserroleUsers { get; set; } = new List<Userrole>();
}
