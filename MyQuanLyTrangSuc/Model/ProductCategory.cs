﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class ProductCategory
{
    public string CategoryId { get; set; }

    public string Categoryname { get; set; }

    public string UnitId { get; set; }

    public int? ProfitPercentage { get; set; }

    public bool IsNotMarketable { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Unit Unit { get; set; }
}