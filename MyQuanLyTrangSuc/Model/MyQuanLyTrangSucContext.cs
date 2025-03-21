﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace MyQuanLyTrangSuc.Model;

public partial class MyQuanLyTrangSucContext : DbContext {
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
    public void ResetProducts() {
        OnItemsReset?.Invoke();
    }
    public int SaveChangesAdded(Invoice invoice) {

        int result = base.SaveChanges();

        OnExportAdded?.Invoke(invoice);

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
    public void ResetEmployees() {
        OnEmployeesReset?.Invoke();
    }

    public event Action<Customer> OnCustomerAdded;
    public event Action<Supplier> OnSupplierAdded;
    public event Action<Supplier> OnSupplierEdited;

    public event Action<Product> OnItemAdded;
    public event Action<Product> OnItemRemoved;
    public event Action OnItemsReset;

    public event Action<Invoice> OnExportAdded;
    public event Action<Import> OnImportAdded;


    public event Action<Employee> OnEmployeeAdded;
    public event Action OnEmployeesReset;

    #region All the tables
    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Import> Imports { get; set; }

    public virtual DbSet<ImportDetail> ImportDetails { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<ServiceDetail> ServiceDetails { get; set; }

    public virtual DbSet<ServiceRecord> ServiceRecords { get; set; }

    public virtual DbSet<StockReport> StockReports { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Unit> Units { get; set; }
    #endregion

    #region Default configurations
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Account>(entity => {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__F267253EA4D3E57B");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__F3DBC572ACA7B43B").IsUnique();

            entity.Property(e => e.AccountId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("accountID");
            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeID");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passwordHash");
            entity.Property(e => e.Role)
                .HasMaxLength(25)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasOne(d => d.Employee).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("FK__Account__employe__6EF57B66");
        });

        modelBuilder.Entity<Customer>(entity => {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__B611CB9D7CF5DB80");

            entity.ToTable("Customer");

            entity.HasIndex(e => e.ContactNumber, "UQ__Customer__4F86E9D70CAE19B9").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__AB6E616404A2A6A6").IsUnique();

            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerID");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("contactNumber");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(255)
                .HasColumnName("customerName");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(6)
                .HasColumnName("gender");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
        });

        modelBuilder.Entity<Employee>(entity => {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__C134C9A12E43ED38");

            entity.ToTable("Employee");

            entity.HasIndex(e => e.ContactNumber, "UQ__Employee__4F86E9D76D690DC3").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Employee__AB6E6164093946CA").IsUnique();

            entity.Property(e => e.EmployeeId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("employeeID");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("contactNumber");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
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
            entity.Property(e => e.Position)
                .HasMaxLength(255)
                .HasColumnName("position");
        });

        modelBuilder.Entity<Import>(entity => {
            entity.HasKey(e => e.ImportId).HasName("PK__Import__2CC5AB07A36C8D51");

            entity.ToTable("Import");

            entity.Property(e => e.ImportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("importID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.SupplierId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("supplierID");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalAmount");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Imports)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK__Import__supplier__5EBF139D");
        });

        modelBuilder.Entity<ImportDetail>(entity => {
            entity.HasKey(e => e.Stt).HasName("PK__ImportDe__DDDF328ED945B462");

            entity.ToTable("ImportDetail");

            entity.HasIndex(e => new { e.ImportId, e.ProductId }, "UQ__ImportDe__9E14A612F93832C3").IsUnique();

            entity.Property(e => e.Stt)
                .ValueGeneratedNever()
                .HasColumnName("stt");
            entity.Property(e => e.ImportId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("importID");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalPrice");

            entity.HasOne(d => d.Import).WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ImportId)
                .HasConstraintName("FK__ImportDet__impor__628FA481");

            entity.HasOne(d => d.Product).WithMany(p => p.ImportDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ImportDet__produ__6383C8BA");
        });

        modelBuilder.Entity<Invoice>(entity => {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__1252410C84D2D083");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("invoiceID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerID");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalAmount");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Invoice__custome__66603565");
        });

        modelBuilder.Entity<InvoiceDetail>(entity => {
            entity.HasKey(e => e.Stt).HasName("PK__InvoiceD__DDDF328EBC6395CC");

            entity.ToTable("InvoiceDetail");

            entity.HasIndex(e => new { e.InvoiceId, e.ProductId }, "UQ__InvoiceD__A0834C19E3CAB5B1").IsUnique();

            entity.Property(e => e.Stt)
                .ValueGeneratedNever()
                .HasColumnName("stt");
            entity.Property(e => e.InvoiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("invoiceID");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productID");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("totalPrice");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("FK__InvoiceDe__invoi__6A30C649");

            entity.HasOne(d => d.Product).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__InvoiceDe__produ__6B24EA82");
        });

        modelBuilder.Entity<Parameter>(entity => {
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

        modelBuilder.Entity<Product>(entity => {
            entity.HasKey(e => e.ProductId).HasName("PK__Product__2D10D14A222AA78F");

            entity.ToTable("Product");

            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productID");
            entity.Property(e => e.CategoryId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("categoryID");
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

        modelBuilder.Entity<ProductCategory>(entity => {
            entity.HasKey(e => e.CategoryId).HasName("PK__ProductC__23CAF1F80606438A");

            entity.ToTable("ProductCategory");

            entity.Property(e => e.CategoryId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("categoryID");
            entity.Property(e => e.Categoryname)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("categoryname");
            entity.Property(e => e.ProfitPercentage).HasColumnName("profitPercentage");
            entity.Property(e => e.UnitId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("unitID");

            entity.HasOne(d => d.Unit).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.UnitId)
                .HasConstraintName("FK__ProductCa__unitI__4BAC3F29");
        });

        modelBuilder.Entity<Service>(entity => {
            entity.HasKey(e => e.ServiceId).HasName("PK__Service__4550733FFFCD3572");

            entity.ToTable("Service");

            entity.Property(e => e.ServiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceID");
            entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");
            entity.Property(e => e.MoreInfo).HasColumnName("moreInfo");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(20)
                .HasColumnName("serviceName");
            entity.Property(e => e.ServicePrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("servicePrice");
        });

        modelBuilder.Entity<ServiceDetail>(entity => {
            entity.HasKey(e => new { e.ServiceRecordId, e.ServiceId }).HasName("PK__ServiceD__6FC5D376D8970C76");

            entity.ToTable("ServiceDetail");

            entity.Property(e => e.ServiceRecordId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceRecordID");
            entity.Property(e => e.ServiceId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceID");
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
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");

            entity.HasOne(d => d.Service).WithMany(p => p.ServiceDetails)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceDe__servi__7A672E12");

            entity.HasOne(d => d.ServiceRecord).WithMany(p => p.ServiceDetails)
                .HasForeignKey(d => d.ServiceRecordId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServiceDe__servi__797309D9");
        });

        modelBuilder.Entity<ServiceRecord>(entity => {
            entity.HasKey(e => e.ServiceRecordId).HasName("PK__ServiceR__8B90D445D2BCF956");

            entity.ToTable("ServiceRecord");

            entity.Property(e => e.ServiceRecordId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("serviceRecordID");
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("customerID");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("imagePath");

            entity.HasOne(d => d.Customer).WithMany(p => p.ServiceRecords)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__ServiceRe__custo__76969D2E");
        });

        modelBuilder.Entity<StockReport>(entity => {
            entity.HasKey(e => new { e.MonthYear, e.ProductId }).HasName("PK__StockRep__2CD5A41C7742B722");

            entity.ToTable("StockReport");

            entity.Property(e => e.MonthYear).HasColumnName("monthYear");
            entity.Property(e => e.ProductId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("productID");
            entity.Property(e => e.BeginStock).HasColumnName("beginStock");
            entity.Property(e => e.FinishStock).HasColumnName("finishStock");
            entity.Property(e => e.PurchaseQuantity).HasColumnName("purchaseQuantity");
            entity.Property(e => e.SalesQuantity).HasColumnName("salesQuantity");

            entity.HasOne(d => d.Product).WithMany(p => p.StockReports)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StockRepo__produ__71D1E811");
        });

        modelBuilder.Entity<Supplier>(entity => {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__DB8E62CD1628F59F");

            entity.ToTable("Supplier");

            entity.HasIndex(e => e.ContactNumber, "UQ__Supplier__4F86E9D7CEE6345E").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Supplier__AB6E6164D15B47DE").IsUnique();

            entity.Property(e => e.SupplierId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("supplierID");
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

        modelBuilder.Entity<Unit>(entity => {
            entity.HasKey(e => e.UnitId).HasName("PK__Unit__55D7921538F28424");

            entity.ToTable("Unit");

            entity.Property(e => e.UnitId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("unitID");
            entity.Property(e => e.UnitName)
                .HasMaxLength(20)
                .HasColumnName("unitName");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (!optionsBuilder.IsConfigured) {
            //Your server goes here!
            optionsBuilder.UseSqlServer("Server=LAPTOP-TNOFNAMI\\SQLEXPRESS;Database=MyQuanLyTrangSuc;TrustServerCertificate=True;Trusted_Connection=True");
        }
    }
}