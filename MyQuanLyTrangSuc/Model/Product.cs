﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class Product
{
    public string ProductId { get; set; }

    public string Name { get; set; }

    public string CategoryId { get; set; }

    public string Material { get; set; }

    public decimal? Price { get; set; }

    public int? Quantity { get; set; }

    public string ImagePath { get; set; }

    public string MoreInfo { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ProductCategory Category { get; set; }

    public virtual ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<StockReport> StockReports { get; set; } = new List<StockReport>();
}