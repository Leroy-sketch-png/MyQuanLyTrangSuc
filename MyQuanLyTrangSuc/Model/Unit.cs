﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class Unit
{
    public string UnitId { get; set; }

    public string UnitName { get; set; }

    public bool IsNotMarketable { get; set; }

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}