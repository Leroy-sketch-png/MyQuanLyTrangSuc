﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class StockReport
{
    public string StockReportId { get; set; }

    public DateTime? MonthYear { get; set; }

    public int? TotalBeginStock { get; set; }

    public int? TotalFinishStock { get; set; }

    public bool IsDeleted { get; set; }
    public virtual ICollection<StockReportDetail> StockReportDetails { get; set; }
}