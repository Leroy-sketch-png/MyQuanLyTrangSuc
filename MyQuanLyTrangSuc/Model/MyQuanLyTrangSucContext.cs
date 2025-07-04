﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyQuanLyTrangSuc.Model;

public partial class MyQuanLyTrangSucContext : DbContext
{
    private static MyQuanLyTrangSucContext instance;
    public static MyQuanLyTrangSucContext Instance {
        get {
            if (instance == null)
                instance = new MyQuanLyTrangSucContext();
            return instance;
        }
    }
    private MyQuanLyTrangSucContext()
        : base() {
    }
    private MyQuanLyTrangSucContext(DbContextOptions<MyQuanLyTrangSucContext> options)
        : base(options) {
    }

    public int SaveChangesAdded(Product product) {

        int result = base.SaveChanges();

        OnItemAdded?.Invoke(product);

        return result;
    }
    public int SaveChangesRemoved(Product product) {
        int result = base.SaveChanges();

        OnItemRemoved?.Invoke(product);

        return result;
    }
    public int SaveChangesRemoved(Service service)
    {
        int result = base.SaveChanges();

        OnServiceRemoved?.Invoke(service);

        return result;
    }

    public void ResetProducts() {
        OnItemsReset?.Invoke();
    }
    public int SaveChangesAdded(Invoice invoice) {

        int result = base.SaveChanges();

        OnInvoiceAdded?.Invoke(invoice);

        return result;
    }
    public int SaveChangesAdded(Import import) {

        int result = base.SaveChanges();

        OnImportAdded?.Invoke(import);

        return result;
    }
    public int SaveChangesAdded(Supplier supplier) {
        int result = base.SaveChanges();

        OnSupplierAdded?.Invoke(supplier);

        return result;
    }
    public int SaveChangesEdited(Supplier supplier) {
        int result = base.SaveChanges();

        OnSupplierEdited?.Invoke(supplier);

        return result;
    }
    public int SaveChangesAdded(Customer customer) {
        int res = base.SaveChanges();
        OnCustomerAdded?.Invoke(customer);
        return res;
    }
    public int SaveChangesAdded(Employee employee) {

        int result = base.SaveChanges();

        OnEmployeeAdded?.Invoke(employee);

        return result;
    }
    public int SaveChangesAdded(Service service)
    {

        int result = base.SaveChanges();

        OnServiceAdded?.Invoke(service);

        return result;
    }

    public int SaveChangesEdited(Service service)
    {
        int result = base.SaveChanges();

        OnServiceEdited?.Invoke(service);

        return result;
    }

    public int SaveChangesAdded(StockReport stockreport)
    {

        int result = base.SaveChanges();

        OnStockReportAdded?.Invoke(stockreport);

        return result;
    }

    public int SaveChangesEdited(StockReport stockreport)
    {
        int result = base.SaveChanges();

        OnStockReportEdited?.Invoke(stockreport);

        return result;
    }
    public void ResetEmployees() {
        OnEmployeesReset?.Invoke();
    }
    #region ALL THE TABLES
    public event Action<Customer> OnCustomerAdded;
    public event Action<Supplier> OnSupplierAdded;
    public event Action<Supplier> OnSupplierEdited;
    public event Action<Account> OnAccountAdded;

    public event Action<Product> OnItemAdded;
    public event Action<Product> OnItemRemoved;
    public event Action OnItemsReset;

    public event Action<Invoice> OnInvoiceAdded;
    public event Action<Import> OnImportAdded;


    public event Action<Employee> OnEmployeeAdded;
    public event Action OnEmployeesReset;

    public event Action<Service> OnServiceAdded;
    public event Action<Service> OnServiceRemoved;
    public event Action<Service> OnServiceEdited;

    public event Action<StockReport> OnStockReportAdded;
    public event Action<StockReport> OnStockReportEdited;

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Function> Functions { get; set; }

    public virtual DbSet<Import> Imports { get; set; }

    public virtual DbSet<ImportDetail> ImportDetails { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<RevenueReport> RevenueReports { get; set; }

    public virtual DbSet<RevenueReportProductDetail> RevenueReportProductDetails { get; set; }

    public virtual DbSet<RevenueReportServiceDetail> RevenueReportServiceDetails { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceDetail> ServiceDetails { get; set; }

    public virtual DbSet<ServiceRecord> ServiceRecords { get; set; }

    public virtual DbSet<StockReport> StockReports { get; set; }

    public virtual DbSet<StockReportDetail> StockReportDetails { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__536C85E54B4E0963");

            entity.ToTable("Account");
            entity.HasIndex(a => a.Username).IsUnique();

            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasOne(d => d.Group).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_usergroups");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__B611CB7DD474EBBC");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.ContactNumber, "UQ__Customer__4F86E9D70E44F0CE").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__AB6E6164290E0E04").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerId");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("contactNumber");
            entity.Property(e => e.DateOfBirth)
                .HasColumnType("datetime")
                .HasColumnName("dateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(6)
                .HasColumnName("gender");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__C134C9C126E430CC");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.ContactNumber, "UQ__Employee__4F86E9D74B031F1A").IsUnique().HasFilter("[isDeleted] = CAST(0 AS BIT)");

            entity.HasIndex(e => e.Email, "UQ__Employee__AB6E616425200F63").IsUnique().HasFilter("[isDeleted] = CAST(0 AS BIT)");

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeId");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("contactNumber");
            entity.Property(e => e.DateOfBirth)
                .HasColumnType("datetime")
                .HasColumnName("dateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(6)
                .HasColumnName("gender");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("imagePath");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .HasColumnName("accountId");

            entity.HasOne(d => d.Account)
                .WithOne(p => p.Employee) 
                .HasForeignKey<Employee>(d => d.AccountId)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.SetNull) 
                .HasConstraintName("fk_employee_account");
        });


        modelBuilder.Entity<Function>(entity =>
        {
            entity.ToTable("Functions");
            entity.HasKey(e => e.FunctionId).HasName("PK__Function__31ABFAF86F0F7071");

            entity.Property(e => e.FunctionName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.ScreenToLoad)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<Import>(entity =>
        {
            entity.HasKey(e => e.ImportId).HasName("PK__Import__2CC5AB67D534D030");

            entity.ToTable("Import");

            entity.Property(e => e.ImportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("importId");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeId");
            entity.Property(e => e.SupplierId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("supplierId");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalAmount");

            entity.HasOne(d => d.Employee).WithMany(p => p.Imports)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Import__employee__7E37BEF6");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Imports)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Import__supplier__7D439ABD");
            entity.Property(d => d.IsDeleted).HasColumnName("isDeleted");
        });

        modelBuilder.Entity<ImportDetail>(entity =>
        {
            entity.HasKey(e => e.Stt).HasName("PK__ImportDe__DDDF328E2AE31392");

            entity.ToTable("ImportDetail");

            entity.HasIndex(e => new { e.ImportId, e.ProductId }, "UQ__ImportDe__9E14A6709C373E82").IsUnique();

            entity.Property(e => e.Stt)
                .ValueGeneratedNever()
                .HasColumnName("stt");
            entity.Property(e => e.ImportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("importId");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productId");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalPrice");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .HasConstraintName("FK__ImportDet__impor__02084FDA");

            entity.HasOne(d => d.Product).WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ImportDet__produ__02FC7413");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__1252416C71196B2D");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("invoiceId");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerId");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeId");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalAmount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Invoice__custome__05D8E0BE");

            entity.HasOne(d => d.Employee).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Invoice__employe__06CD04F7");
            entity.Property(d => d.IsDeleted).HasColumnName("isDeleted");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.Stt).HasName("PK__InvoiceD__DDDF328E419C490B");

            entity.ToTable("InvoiceDetail");

            entity.HasIndex(e => new { e.InvoiceId, e.ProductId }, "UQ__InvoiceD__A0834C7B7856ABA2").IsUnique();

            entity.Property(e => e.Stt)
                .ValueGeneratedNever()
                .HasColumnName("stt");
            entity.Property(e => e.InvoiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("invoiceId");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productId");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalPrice");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceDe__invoi__0A9D95DB");

            entity.HasOne(d => d.Product).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__InvoiceDe__produ__0B91BA14");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Parameter");

            entity.Property(e => e.ConstName)
                .HasMaxLength(20)
                .HasColumnName("constName");
            entity.Property(e => e.ConstValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("constValue");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F3F62ADAF");

            entity.ToTable("Permission");

            entity.HasIndex(e => new { e.GroupId, e.FunctionId }, "UQ__Permissi__87804C9AFB0265DB").IsUnique();

            entity.Property(e => e.FunctionId).HasColumnName("FunctionID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.IsDeleted).HasColumnName("IsDeleted");

            entity.HasOne(d => d.Function)
                .WithMany(p => p.Permissions)
                .HasForeignKey(d => d.FunctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_permissions_functions");

            entity.HasOne(d => d.Group)
                .WithMany(p => p.Permissions)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_permissions_usergroups");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__2D10D16A7550EE51");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productId");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("categoryId");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("imagePath");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Material)
                .HasMaxLength(255)
                .HasColumnName("material");
            entity.Property(e => e.MoreInfo).HasColumnName("moreInfo");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__Product__categor__4E88ABD4");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__ProductC__23CAF1D81E01E14A");

            entity.ToTable("ProductCategory");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("categoryId");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(20)
                .HasColumnName("categoryName");
            entity.Property(e => e.IsNotMarketable).HasColumnName("isNotMarketable");
            entity.Property(e => e.ProfitPercentage).HasColumnName("profitPercentage");
            entity.Property(e => e.UnitId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("unitId");

            entity.HasOne(d => d.Unit).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("FK__ProductCa__unitI__4BAC3F29");
        });

        modelBuilder.Entity<RevenueReport>(entity =>
        {
            entity.HasKey(e => e.RevenueReportId).HasName("PK__RevenueR__F8164E987496DC39");

            entity.ToTable("RevenueReport");

            entity.Property(e => e.RevenueReportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("revenueReportId");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MonthYear)
                .HasColumnType("datetime")
                .HasColumnName("monthYear");
            entity.Property(e => e.TotalRevenue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalRevenue");
        });

        modelBuilder.Entity<RevenueReportProductDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("RevenueReportProductDetail");

            entity.HasIndex(e => new { e.RevenueReportId, e.ProductId }, "UQ__RevenueR__4AC7438FA1B54800").IsUnique();

            entity.Property(e => e.Percentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("percentage");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productId");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Revenue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("revenue");
            entity.Property(e => e.RevenueReportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("revenueReportId");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__RevenueRe__produ__6477ECF3");

            entity.HasOne(d => d.RevenueReport).WithMany()
                .HasForeignKey(d => d.RevenueReportId)
                .HasConstraintName("FK__RevenueRe__reven__6383C8BA");
        });

        modelBuilder.Entity<RevenueReportServiceDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("RevenueReportServiceDetail");

            entity.HasIndex(e => new { e.RevenueReportId, e.ServiceId }, "UQ__RevenueR__1C434994F1684961").IsUnique();

            entity.Property(e => e.Percentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("percentage");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Revenue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("revenue");
            entity.Property(e => e.RevenueReportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("revenueReportId");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceId");

            entity.HasOne(d => d.RevenueReport).WithMany()
                .HasForeignKey(d => d.RevenueReportId)
                .HasConstraintName("FK__RevenueRe__reven__6754599E");

            entity.HasOne(d => d.Service).WithMany()
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__RevenueRe__servi__68487DD7");
        });

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RevenueReportProductDetail>()
            .HasKey(r => new { r.RevenueReportId, r.ProductId });

        modelBuilder.Entity<RevenueReportServiceDetail>()
            .HasKey(r => new { r.RevenueReportId, r.ServiceId });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__455070DF04F2629C");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceId");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("imagePath");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MoreInfo).HasColumnName("moreInfo");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(20)
                .HasColumnName("serviceName");
            entity.Property(e => e.ServicePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("servicePrice");
        });

        modelBuilder.Entity<ServiceDetail>(entity =>
        {
            entity.HasKey(e => e.Stt).HasName("PK__ServiceD__DDDF328EEBFB29AD");

            entity.ToTable("ServiceDetail");

            entity.HasIndex(e => new { e.ServiceRecordId, e.ServiceId }, "UQ__ServiceD__6FC5D0A96B3E1231").IsUnique();

            entity.Property(e => e.Stt)
                .ValueGeneratedNever()
                .HasColumnName("stt");
            entity.Property(e => e.DueDay)
                .HasColumnType("datetime")
                .HasColumnName("dueDay");
            entity.Property(e => e.ExtraExpense)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("extraExpense");
            entity.Property(e => e.Prepaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("prepaid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceId");
            entity.Property(e => e.ServiceRecordId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceRecordId");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Unpaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unpaid");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceDetails)
                .HasForeignKey(d => d.ServiceId)
                .HasConstraintName("FK__ServiceDe__servi__160F4887");

            entity.HasOne(d => d.ServiceRecord).WithMany(p => p.ServiceDetails)
                .HasForeignKey(d => d.ServiceRecordId)
                .HasConstraintName("FK__ServiceDe__servi__151B244E");
        });

        modelBuilder.Entity<ServiceRecord>(entity =>
        {
            entity.HasKey(e => e.ServiceRecordId).HasName("PK__ServiceR__8B90D7A501D7DF28");

            entity.ToTable("ServiceRecord");

            entity.Property(e => e.ServiceRecordId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceRecordId");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerId");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeId");
            entity.Property(e => e.GrandTotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("grandTotal");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.TotalPaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalPaid");
            entity.Property(e => e.TotalUnpaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalUnpaid");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRecords)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__ServiceRe__custo__0F624AF8");

            entity.HasOne(d => d.Employee).WithMany(p => p.ServiceRecords)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__ServiceRe__emplo__10566F31");
        });

        modelBuilder.Entity<StockReport>(entity =>
        {
            entity.HasKey(e => e.StockReportId).HasName("PK__StockRep__F16E8D88EE89A6E8");

            entity.ToTable("StockReport");

            entity.Property(e => e.StockReportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("stockReportId");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MonthYear)
                .HasColumnType("datetime")
                .HasColumnName("monthYear");
            entity.Property(e => e.TotalBeginStock).HasColumnName("totalBeginStock");
            entity.Property(e => e.TotalFinishStock).HasColumnName("totalFinishStock");
        });

        modelBuilder.Entity<StockReportDetail>(entity =>
        {
            entity.HasKey(e => new { e.StockReportId, e.ProductId }); // ❗ Sửa tại đây

            entity.ToTable("StockReportDetail");

            entity.HasIndex(e => new { e.StockReportId, e.ProductId }, "UQ__StockRep__43BF809F530D93D1").IsUnique();

            entity.Property(e => e.BeginStock).HasColumnName("beginStock");
            entity.Property(e => e.FinishStock).HasColumnName("finishStock");
            entity.Property(e => e.ImportQuantity).HasColumnName("importQuantity");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productId");
            entity.Property(e => e.SaleQuantity).HasColumnName("saleQuantity");
            entity.Property(e => e.StockReportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("stockReportId");

            entity.HasOne(d => d.Product).WithMany()
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__StockRepo__produ__5EBF139D");

            entity.HasOne(d => d.StockReport).WithMany(p => p.StockReportDetails)
                .HasForeignKey(d => d.StockReportId)
                .HasConstraintName("FK__StockRepo__stock__5DCAEF64");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__DB8E62ED1AF4D02D");

            entity.ToTable("Supplier");

            entity.HasIndex(e => e.ContactNumber, "UQ__Supplier__4F86E9D74CD3E897").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Supplier__AB6E6164E951C97E").IsUnique();

            entity.Property(e => e.SupplierId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("supplierId");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("contactNumber");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PK__Unit__55D7923577E49203");

            entity.ToTable("Unit");

            entity.Property(e => e.UnitId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("unitId");
            entity.Property(e => e.IsNotMarketable).HasColumnName("isNotMarketable");
            entity.Property(e => e.UnitName)
                .HasMaxLength(20)
                .HasColumnName("unitName");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__UserGrou__149AF36A5F5FD2A5");

            entity.ToTable("UserGroup");

            entity.HasIndex(e => e.GroupName, "UQ__UserGrou__6EFCD43460934E45").IsUnique();

            entity.Property(e => e.GroupName)
                .IsRequired()
                .HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    #endregion
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            //Your server goes here!
            optionsBuilder.UseLazyLoadingProxies().UseSqlServer("Server=LAPTOP-TNOFNAMI\\SQLEXPRESS;Database=MyQuanLyTrangSuc;TrustServerCertificate=True;Trusted_Connection=True");
        }
    }
}