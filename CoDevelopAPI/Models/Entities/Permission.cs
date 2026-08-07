using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Permission
{
    public int Permid { get; set; }

    public string Permname { get; set; } = null!;

    public string? Module { get; set; }

    public string? Action { get; set; }

    public string? Resource { get; set; }

    public int? Categoryid { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Rolepermission> Rolepermissions { get; set; } = new List<Rolepermission>();
}
