using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.Model;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using MyQuanLyTrangSuc.View;
using Microsoft.EntityFrameworkCore;

namespace MyQuanLyTrangSuc.ViewModel
{
    //public class MonthlyStockReportPageLogic : INotifyPropertyChanged
    //{
    //    private const string ErrorTitle = "Error";
    //    private const string SuccessTitle = "Success";
    //    private const string ExportSuccessMessage = "Monthly report exported successfully!";
    //    private const string ImportSuccessMessage = "Monthly report imported successfully!";
    //    private const string DeleteConfirmMessage = "Are you sure you want to delete this report?";
    //    private const string DeleteSuccessMessage = "Report deleted successfully!";
    //    private const string GenerateReportSuccessMessage = "Monthly report generated successfully!";

    //    private ObservableCollection<MonthlyStockReportViewModel> _monthlyStockReports;
    //    private MonthlyStockReportViewModel _selectedMonthlyStockReport;
    //    private string _originalSearchText;
    //    private readonly Page _page;
    //    private readonly MyQuanLyTrangSucContext _dbContext;

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public ObservableCollection<MonthlyStockReportViewModel> MonthlyStockReports
    //    {
    //        get => _monthlyStockReports;
    //        set
    //        {
    //            _monthlyStockReports = value;
    //            OnPropertyChanged(nameof(MonthlyStockReports));
    //        }
    //    }

    //    public MonthlyStockReportViewModel SelectedMonthlyStockReport
    //    {
    //        get => _selectedMonthlyStockReport;
    //        set
    //        {
    //            _selectedMonthlyStockReport = value;
    //            OnPropertyChanged(nameof(SelectedMonthlyStockReport));
    //        }
    //    }

    //    public MonthlyStockReportPageLogic(Page page)
    //    {
    //        _page = page;
    //        _dbContext = MyQuanLyTrangSucContext.Instance;
    //        MonthlyStockReports = new ObservableCollection<MonthlyStockReportViewModel>();
    //        LoadReports();
    //    }

    //    protected virtual void OnPropertyChanged(string propertyName = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }

    //    private void LoadReports()
    //    {
    //        try
    //        {
    //            MonthlyStockReports.Clear();

    //            // Group stock reports by month and year
    //            var reports = _dbContext.StockReports
    //                .GroupBy(sr => new { Month = sr.MonthYear.Month, Year = sr.MonthYear.Year })
    //                .Select(g => new MonthlyStockReportViewModel
    //                {
    //                    Month = g.Key.Month,
    //                    Year = g.Key.Year,
    //                    TotalItems = g.Sum(sr => sr.FinishStock ?? 0),
    //                    TotalValue = CalculateTotalValue(g.ToList())
    //                })
    //                .OrderByDescending(r => r.Year)
    //                .ThenByDescending(r => r.Month)
    //                .ToList();

    //            foreach (var report in reports)
    //            {
    //                MonthlyStockReports.Add(report);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error loading reports: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }
    //    public decimal TotalValueCalculated()
    //    {
    //        // Đầu tiên, lấy dữ liệu từ database và tải về bộ nhớ
    //        var reports = _dbContext.StockReports
    //            .Include(r => r.Product)
    //            .ToList(); // Quan trọng: ToList() để tải dữ liệu về client

    //        // Sau đó thực hiện tính toán trên dữ liệu đã tải
    //        decimal totalValue = CalculateTotalValue(reports);
    //        return totalValue;
    //        // Tiếp tục xử lý với totalValue...
    //    }
    //    private decimal CalculateTotalValue(List<StockReport> reports)
    //    {
    //        decimal totalValue = 0;
    //        foreach (var report in reports)
    //        {
    //            if (report.Product != null && report.FinishStock.HasValue)
    //            {
    //                // Kiểm tra và xử lý nếu Price là nullable
    //                decimal price = report.Product.Price ?? 0m; // Hoặc sử dụng report.Product.Price.Value nếu bạn chắc chắn nó không null
    //                totalValue += price * report.FinishStock.Value;
    //            }
    //        }
    //        return totalValue;
    //    }

    //    public void GenerateMonthlyReport()
    //    {
    //        try
    //        {
    //            // Implement report generation logic here
    //            // Example: Generate stock report for the current month

    //            var currentDate = DateTime.Now;
    //            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

    //            // Get all products
    //            var products = _dbContext.Products.ToList();

    //            // Check if report already exists for this month
    //            var existingReports = _dbContext.StockReports
    //                .Where(sr => sr.MonthYear.Month == currentDate.Month && sr.MonthYear.Year == currentDate.Year)
    //                .ToList();

    //            if (existingReports.Any())
    //            {
    //                if (MessageBox.Show("Report already exists for this month. Do you want to regenerate it?",
    //                    "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
    //                {
    //                    return;
    //                }

    //                // Remove existing reports
    //                _dbContext.StockReports.RemoveRange(existingReports);
    //            }

    //            // Create new reports for each product
    //            foreach (var product in products)
    //            {
    //                // Get previous month's finish stock as begin stock
    //                var prevMonth = firstDayOfMonth.AddMonths(-1);
    //                var prevMonthStock = _dbContext.StockReports
    //                    .FirstOrDefault(sr => sr.ProductId == product.ProductId &&
    //                                         sr.MonthYear.Month == prevMonth.Month &&
    //                                         sr.MonthYear.Year == prevMonth.Year);

    //                var beginStock = prevMonthStock?.FinishStock ?? 0;

    //                // Calculate purchase and sale quantities for the current month
    //                // This is a simplified example - you would need to implement logic to get actual quantities
    //                var purchaseQuantity = 0; // Get from purchases table
    //                var salesQuantity = 0;    // Get from sales table

    //                var stockReport = new StockReport
    //                {
    //                    MonthYear = firstDayOfMonth,
    //                    ProductId = product.ProductId,
    //                    BeginStock = beginStock,
    //                    PurchaseQuantity = purchaseQuantity,
    //                    SalesQuantity = salesQuantity,
    //                    FinishStock = beginStock + purchaseQuantity - salesQuantity,
    //                    Product = product
    //                };

    //                _dbContext.StockReports.Add(stockReport);
    //            }

    //            _dbContext.SaveChanges();
    //            LoadReports();
    //            MessageBox.Show(GenerateReportSuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error generating report: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void ImportFromExcel()
    //    {
    //        try
    //        {
    //            OpenFileDialog openFileDialog = new OpenFileDialog
    //            {
    //                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
    //                Title = "Select an Excel File"
    //            };

    //            if (openFileDialog.ShowDialog() == true)
    //            {
    //                string filePath = openFileDialog.FileName;

    //                // Implement excel import logic here
    //                // Example: Read Excel file and save data to database

    //                // After import, reload the reports
    //                LoadReports();
    //                MessageBox.Show(ImportSuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error importing from Excel: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void ExportToExcel()
    //    {
    //        try
    //        {
    //            if (!MonthlyStockReports.Any())
    //            {
    //                MessageBox.Show("No data to export!", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    //                return;
    //            }

    //            SaveFileDialog saveFileDialog = new SaveFileDialog
    //            {
    //                Filter = "Excel Files|*.xlsx",
    //                Title = "Save Excel File",
    //                FileName = $"MonthlyStockReport_{DateTime.Now:yyyy-MM-dd}"
    //            };

    //            if (saveFileDialog.ShowDialog() == true)
    //            {
    //                string filePath = saveFileDialog.FileName;

    //                // Implement excel export logic here
    //                // Example: Create Excel file with report data

    //                MessageBox.Show(ExportSuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error exporting to Excel: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void FilterReports(string searchText, int filterType)
    //    {
    //        try
    //        {
    //            if (string.IsNullOrEmpty(searchText))
    //            {
    //                LoadReports();
    //                return;
    //            }

    //            if (_originalSearchText == null)
    //            {
    //                _originalSearchText = searchText;
    //            }

    //            var allReports = _dbContext.StockReports
    //                .GroupBy(sr => new { Month = sr.MonthYear.Month, Year = sr.MonthYear.Year })
    //                .Select(g => new MonthlyStockReportViewModel
    //                {
    //                    Month = g.Key.Month,
    //                    Year = g.Key.Year,
    //                    TotalItems = g.Sum(sr => sr.FinishStock ?? 0),
    //                    TotalValue = CalculateTotalValue(g.ToList())
    //                })
    //                .ToList();

    //            var filtered = allReports.Where(r =>
    //                filterType == 0 ?
    //                    r.Month.ToString().Contains(searchText) ||
    //                    CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(r.Month).Contains(searchText, StringComparison.OrdinalIgnoreCase) :
    //                    r.Year.ToString().Contains(searchText))
    //                .OrderByDescending(r => r.Year)
    //                .ThenByDescending(r => r.Month)
    //                .ToList();

    //            MonthlyStockReports = new ObservableCollection<MonthlyStockReportViewModel>(filtered);
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error filtering reports: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void ViewReportDetails(MonthlyStockReportViewModel report)
    //    {
    //        try
    //        {
    //            if (report == null)
    //            {
    //                MessageBox.Show("No report selected!", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    //                return;
    //            }

    //            // Fetch detailed report data
    //            var detailedReports = _dbContext.StockReports
    //                .Where(sr => sr.MonthYear.Month == report.Month && sr.MonthYear.Year == report.Year)
    //                .ToList();

    //            // Display report details (implement appropriate UI logic here)
    //            // Example: Open a new window to display the detailed report

    //            MessageBox.Show($"Viewing details for report: {report.Month}/{report.Year}",
    //                "Report Details", MessageBoxButton.OK, MessageBoxImage.Information);
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error viewing report details: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void DeleteReport(MonthlyStockReportViewModel report)
    //    {
    //        try
    //        {
    //            if (report == null)
    //            {
    //                MessageBox.Show("No report selected!", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
    //                return;
    //            }

    //            if (MessageBox.Show(DeleteConfirmMessage, "Confirm Delete",
    //                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
    //            {
    //                // Delete the report from database
    //                var reportsToDelete = _dbContext.StockReports
    //                    .Where(sr => sr.MonthYear.Month == report.Month && sr.MonthYear.Year == report.Year)
    //                    .ToList();

    //                _dbContext.StockReports.RemoveRange(reportsToDelete);
    //                _dbContext.SaveChanges();

    //                // Remove from view model collection
    //                MonthlyStockReports.Remove(report);

    //                MessageBox.Show(DeleteSuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            MessageBox.Show($"Error deleting report: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
    //        }
    //    }

    //    public void Reset()
    //    {
    //        LoadReports();
    //    }
    //}

    //public class MonthlyStockReportViewModel : INotifyPropertyChanged
    //{
    //    private int _month;
    //    private int _year;
    //    private int _totalItems;
    //    private decimal _totalValue;

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public int Month
    //    {
    //        get => _month;
    //        set
    //        {
    //            _month = value;
    //            OnPropertyChanged(nameof(Month));
    //            OnPropertyChanged(nameof(MonthName));
    //        }
    //    }

    //    public int Year
    //    {
    //        get => _year;
    //        set
    //        {
    //            _year = value;
    //            OnPropertyChanged(nameof(Year));
    //        }
    //    }

    //    public int TotalItems
    //    {
    //        get => _totalItems;
    //        set
    //        {
    //            _totalItems = value;
    //            OnPropertyChanged(nameof(TotalItems));
    //        }
    //    }

    //    public decimal TotalValue
    //    {
    //        get => _totalValue;
    //        set
    //        {
    //            _totalValue = value;
    //            OnPropertyChanged(nameof(TotalValue));
    //            OnPropertyChanged(nameof(FormattedTotalValue));
    //        }
    //    }

    //    public string MonthName => CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);

    //    public string FormattedTotalValue => TotalValue.ToString("C", CultureInfo.CurrentCulture);

    //    protected virtual void OnPropertyChanged(string propertyName = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //    }
    //}
}