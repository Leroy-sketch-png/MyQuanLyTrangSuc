using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportWindowLogic : INotifyPropertyChanged
    {
        #region Properties

        private List<StockReport> _stockReports;
        public List<StockReport> StockReports
        {
            get => _stockReports;
            set
            {
                _stockReports = value;
                OnPropertyChanged();
                CalculateTotalRevenue();
            }
        }

        private DateTime _selectedMonthYear;
        public DateTime SelectedMonthYear
        {
            get => _selectedMonthYear;
            set
            {
                _selectedMonthYear = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthDisplay));
                OnPropertyChanged(nameof(YearDisplay));
                LoadData();
            }
        }

        public string MonthDisplay => SelectedMonthYear.ToString("MMMM");
        public string YearDisplay => SelectedMonthYear.Year.ToString();

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public MonthlyStockReportWindowLogic(Window window)
        {
            _window = window;
            _dbContext = MyQuanLyTrangSucContext.Instance;
            MonthlyStockReports = new ObservableCollection<MonthlyStockReportViewModel>();
            LoadData();
        }

        #endregion

        #region Data Methods

        private void LoadData()
        {
            try
            {
                // Get data from database context
                var db = MyQuanLyTrangSucContext.Instance;

                // Query stock reports for the selected month/year
                var startOfMonth = new DateTime(SelectedMonthYear.Year, SelectedMonthYear.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // Get all stock reports for the selected month/year
                StockReports = db.StockReports
                    .Where(sr => sr.MonthYear >= startOfMonth && sr.MonthYear <= endOfMonth)
                    .Include(sr => sr.Product)  // Include product details
                    .ToList();

                // If no data exists for this month, generate reports for all products
                if (StockReports == null || !StockReports.Any())
                {
                    StockReports = new List<StockReport>();

                    // Get all products
                    var allProducts = db.Products.ToList();

                    // Get previous month's stock reports to calculate beginning stock
                    var previousMonth = SelectedMonthYear.AddMonths(-1);
                    var previousStartOfMonth = new DateTime(previousMonth.Year, previousMonth.Month, 1);
                    var previousEndOfMonth = previousStartOfMonth.AddMonths(1).AddDays(-1);

                    var previousReports = db.StockReports
                        .Where(sr => sr.MonthYear >= previousStartOfMonth && sr.MonthYear <= previousEndOfMonth)
                        .ToDictionary(sr => sr.ProductId, sr => sr);

                    // Create a new report for each product
                    foreach (var product in allProducts)
                    {
                        // Calculate beginning stock from previous month's finish stock (if available)
                        int beginStock = 0;
                        if (previousReports.TryGetValue(product.ProductId, out var prevReport))
                        {
                            beginStock = prevReport.FinishStock;
                        }

                        // Calculate purchase quantity from purchase records
                        int purchaseQty = db.PurchaseItems
                            .Where(pi => pi.ProductId == product.ProductId &&
                                  pi.Purchase.PurchaseDate >= startOfMonth &&
                                  pi.Purchase.PurchaseDate <= endOfMonth)
                            .Sum(pi => pi.Quantity);

                        // Calculate sales quantity from sales records
                        int salesQty = db.SalesItems
                            .Where(si => si.ProductId == product.ProductId &&
                                  si.Sale.SaleDate >= startOfMonth &&
                                  si.Sale.SaleDate <= endOfMonth)
                            .Sum(si => si.Quantity);

                        // Calculate finish stock
                        int finishStock = beginStock + purchaseQty - salesQty;

                        // Create new stock report
                        StockReports.Add(new StockReport
                        {
                            MonthYear = startOfMonth,
                            ProductId = product.ProductId,
                            BeginStock = beginStock,
                            PurchaseQuantity = purchaseQty,
                            SalesQuantity = salesQty,
                            FinishStock = finishStock,
                            Product = product
                        });
                    }
                }

                // Calculate total revenue
                CalculateTotalRevenue();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading stock report data: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void CalculateTotalRevenue()
        {
            if (StockReports == null || !StockReports.Any())
            {
                TotalRevenue = 0;
                return;
            }

            // Calculate total revenue based on sales quantity and product price
            // Add 4% markup as specified in the UI
            decimal revenue = StockReports.Sum(sr => sr.SalesQuantity * sr.Product.Price);
            TotalRevenue = revenue * 1.04m; // Apply 4% markup
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}