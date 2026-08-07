using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Client
{
    public int Clientid { get; set; }

    public string Businesstype { get; set; } = null!;

    public string Businessname { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public decimal Monthlyprice { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
