﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MyQuanLyTrangSuc.Model;

public partial class Account
{
    public string AccountId { get; set; }

    public string EmployeeId { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; }

    public virtual Employee Employee { get; set; }
}