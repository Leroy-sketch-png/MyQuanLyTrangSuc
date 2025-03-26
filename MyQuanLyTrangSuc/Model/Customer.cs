﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class Customer
{
    public string CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string ContactNumber { get; set; }

    public string Email { get; set; }

    public string Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string Gender { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
}