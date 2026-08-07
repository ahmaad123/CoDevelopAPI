using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Ticket
{
    public int Ticketid { get; set; }

    public int Reporterid { get; set; }

    public int? Assigneeid { get; set; }

    public int? Clientid { get; set; }

    public string Subject { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public virtual User? Assignee { get; set; }

    public virtual User? Client { get; set; }

    public virtual User Reporter { get; set; } = null!;
}
