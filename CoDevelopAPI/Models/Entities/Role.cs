using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Role
{
    public int Roleid { get; set; }

    public string Rolename { get; set; } = null!;

    public int Level { get; set; }

    public string? Rolecode { get; set; }

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Rolepermission> Rolepermissions { get; set; } = new List<Rolepermission>();

    public virtual ICollection<Userrole> Userroles { get; set; } = new List<Userrole>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
