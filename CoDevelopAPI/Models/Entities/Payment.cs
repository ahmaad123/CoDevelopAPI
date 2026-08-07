using System;
using System.Collections.Generic;

namespace CoDevelopAPI.Models.Entities;

public partial class Payment
{
    public int Paymentid { get; set; }

    public int Invoicenumber { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public virtual Invoice InvoicenumberNavigation { get; set; } = null!;
}
