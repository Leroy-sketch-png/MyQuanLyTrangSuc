﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class RevenueReport
{
    public string RevenueReportId { get; set; }

    public DateTime? MonthYear { get; set; }

    public decimal? TotalRevenue { get; set; }

    public bool IsDeleted { get; set; }
}