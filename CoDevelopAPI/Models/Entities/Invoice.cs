using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Invoice
{
    public int Invoicenumber { get; set; }

    public int Clientid { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public string Paymentstatus { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
