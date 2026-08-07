using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Rolepermission
{
    public int Rolepermid { get; set; }

    public int Roleid { get; set; }

    public int Permissionid { get; set; }

    public bool Isallowed { get; set; }

    public int? Grantedby { get; set; }

    public virtual User? GrantedbyNavigation { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
