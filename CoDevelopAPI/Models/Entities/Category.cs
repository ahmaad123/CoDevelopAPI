using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Category
{
    public int Categoryid { get; set; }

    public string? Categoryname { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
