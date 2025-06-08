using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static MyQuanLyTrangSuc.ViewModel.DashboardPageLogic;
namespace MyQuanLyTrangSuc.ViewModel {
    public class DashboardPageLogic : INotifyPropertyChanged {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly DashboardPage dashboardPage;

        private readonly TotalProductSold totalProductSold;
        private readonly RevenueCalculator revenueCalculator;
        private Func<double, string> DateFormatter;
        private Func<double, string> NetFormatter;
        public SeriesCollection SeriesCollection { get; set; }

        private LineSeries positiveSeries, negativeSeries;

        private DateTime? selectedFromDate;
        private DateTime? selectedUntilDate;

        public DateTime? SelectedFromDate {
            get => selectedFromDate;
            set {
                if (value != selectedFromDate) {
                    selectedFromDate = value;
                    OnPropertyChanged(nameof(SelectedFromDate));
                    LoadData();
                }
            }
        }

        public DateTime? SelectedUntilDate {
            get => selectedUntilDate;
            set {
                if (value != selectedUntilDate) {
                    selectedUntilDate = value;
                    OnPropertyChanged(nameof(SelectedUntilDate));
                    LoadData();
                }
            }
        }

        public DashboardPageLogic(DashboardPage dashboardPage) {
            this.dashboardPage = dashboardPage;

            totalProductSold = new TotalProductSold();
            revenueCalculator = new RevenueCalculator();

            // Formatters for axes
            DateFormatter = value => new DateTime((long)value).ToString("dd-MM");
            NetFormatter = value => $"{value:N}";

            // Map DateTimePoint X,Y
            var dayConfig = Mappers.Xy<DateTimePoint>()
                .X(dp => dp.DateTime.Ticks)
                .Y(dp => dp.Value);

            // Positive (green) line
            positiveSeries = new LineSeries {
                Title = ">= 0",
                Values = new ChartValues<DateTimePoint>(),
                Configuration = dayConfig,
                Stroke = Brushes.Green,
                Fill = Brushes.Transparent,
            };

            // Negative (red) line
            negativeSeries = new LineSeries {
                Title = "< 0",
                Values = new ChartValues<DateTimePoint>(),
                Configuration = dayConfig,
                Stroke = Brushes.Red,
                Fill = Brushes.Transparent,
            };

            SeriesCollection = new SeriesCollection
            {
                positiveSeries,
                negativeSeries
            };

            // Default range: one month ago -> today
            selectedFromDate = DateTime.Today.AddMonths(-1);
            selectedUntilDate = DateTime.Today;

            // Initial load & binding
            LoadData();
            BindDataToUI();
        }

        private void LoadData() {
            positiveSeries.Values.Clear();
            negativeSeries.Values.Clear();

            if (selectedFromDate == null || selectedUntilDate == null) return;

            var importData = context.Imports
                .Where(i => i.Date >= selectedFromDate && i.Date <= selectedUntilDate && !i.IsDeleted)
                .Select(i => new { Date = i.Date.Value, Value = -i.TotalAmount })
                .ToList();

            var exportData = context.Invoices
                .Where(e => e.Date >= selectedFromDate && e.Date <= selectedUntilDate && !e.IsDeleted)
                .Select(e => new { Date = e.Date.Value, Value = e.TotalAmount })
                .ToList();

            var serviceData = context.ServiceRecords
                .Where(s => s.CreateDate >= selectedFromDate && s.CreateDate <= selectedUntilDate)
                .Select(s => new { Date = s.CreateDate.Value, Value = s.GrandTotal })
                .ToList();

            var combined = importData
                .Select(i => new DateTimePoint(i.Date, (double)i.Value))
                .Concat(exportData.Select(e => new DateTimePoint(e.Date, (double)e.Value)))
                .Concat(serviceData.Select(s => new DateTimePoint(s.Date, (double)s.Value)))
                .OrderBy(pt => pt.DateTime)
                .ToList();

            if (!combined.Any())
            {
                // No data? Plot a flat zero line between from..until to prevent axis error
                positiveSeries.Values.Add(new DateTimePoint(selectedFromDate.Value, 0));
                positiveSeries.Values.Add(new DateTimePoint(selectedUntilDate.Value, 0));

                // Still fill negativeSeries with NaNs so they align visually
                negativeSeries.Values.Add(new DateTimePoint(selectedFromDate.Value, double.NaN));
                negativeSeries.Values.Add(new DateTimePoint(selectedUntilDate.Value, double.NaN));

                // you can optionally skip negativeSeries here
                return;
            }
            double cumulative = 0;

            var from = selectedFromDate.Value;
            positiveSeries.Values.Add(new DateTimePoint(from, 0));
            negativeSeries.Values.Add(new DateTimePoint(from, double.NaN));

            // track that our last sign was non-negative
            bool? prevPositive = true;

            foreach (var pt in combined) {
                cumulative += pt.Value;
                bool currPositive = cumulative >= 0;

                // On sign change, add a zero point to both series to connect at zero
                if (prevPositive.HasValue && currPositive != prevPositive.Value) {
                    var zeroPoint = new DateTimePoint(pt.DateTime, 0);
                    positiveSeries.Values.Add(zeroPoint);
                    negativeSeries.Values.Add(zeroPoint);
                }

                // Add the real point and gap opposite series
                if (currPositive) {
                    positiveSeries.Values.Add(new DateTimePoint(pt.DateTime, cumulative));
                    negativeSeries.Values.Add(new DateTimePoint(pt.DateTime, double.NaN));
                } else {
                    negativeSeries.Values.Add(new DateTimePoint(pt.DateTime, cumulative));
                    positiveSeries.Values.Add(new DateTimePoint(pt.DateTime, double.NaN));
                }

                prevPositive = currPositive;
            }

            // Plateau out to the "until" date
            if (combined.Any()) {
                var lastDate = combined.Last().DateTime;
                if (lastDate < selectedUntilDate.Value) {
                    bool lastPositive = cumulative >= 0;
                    if (prevPositive.HasValue && lastPositive != prevPositive.Value) {
                        var zeroPoint = new DateTimePoint(selectedUntilDate.Value, 0);
                        positiveSeries.Values.Add(zeroPoint);
                        negativeSeries.Values.Add(zeroPoint);
                    }

                    var plateau = new DateTimePoint(selectedUntilDate.Value, cumulative);
                    if (cumulative >= 0)
                        positiveSeries.Values.Add(plateau);
                    else
                        negativeSeries.Values.Add(plateau);
                }
            }
        }

        private void BindDataToUI() {
            dashboardPage.DataContext = this;
            dashboardPage.NetChart.Series = SeriesCollection;
            dashboardPage.NetChart.AxisX[0].LabelFormatter = DateFormatter;
            dashboardPage.NetChart.AxisY[0].LabelFormatter = NetFormatter;
            dashboardPage.NetChart.LegendLocation = LegendLocation.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));




        public class RevenueCalculator {
            private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

            public decimal CalculateDailyRevenue(DateTime date) {
                var hasRecords = context.Invoices
                    .Any(e => e.Date.HasValue && e.Date.Value.Date == date.Date);

                if (!hasRecords) return 0;

                var revenue = context.Invoices
                    .Where(e => e.Date.HasValue && e.Date.Value.Date == date.Date)
                    .Sum(e => e.TotalAmount ?? 0);

                return revenue;
            }
        }

        public class TotalProductSold {
            private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

            public int TotalDailySales(DateTime date) {
                var hasRecords = context.Invoices
                    .Any(e => e.Date.HasValue && e.Date.Value.Date == date.Date);

                if (!hasRecords) return 0;

                var total = context.InvoiceDetails
                    .Where(e => e.Invoice.Date.HasValue && e.Invoice.Date.Value.Date == date.Date)
                    .Sum(e => e.Quantity ?? 0);

                return total;
            }
        }

        public DashboardPageLogic() {
            totalProductSold = new TotalProductSold();
            revenueCalculator = new RevenueCalculator();
        }

        public void UpdateDailyCountTextBox(DateTime date, TextBlock textBlock) {
            var dailyCount = totalProductSold.TotalDailySales(date);
            Application.Current.Dispatcher.Invoke(() => {
                textBlock.Text = dailyCount == 0 ? "No sales today." : $"{dailyCount} PRODUCTS";
            });
        }

        public void UpdateDailyRevenueTextBox(DateTime date, TextBlock textBlock) {
            var dailyRevenue = revenueCalculator.CalculateDailyRevenue(date);
            Application.Current.Dispatcher.Invoke(() => {
                textBlock.Text = dailyRevenue == 0 ? "No sales today." : $"{dailyRevenue:N} VND";
            });
        }

        public class MostConsumedProducts {
            private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

            public void UpdateTopProductsTextBlocks(TextBlock firstTextBlock, TextBlock secondTextBlock, TextBlock thirdTextBlock, TextBlock fourthTextBlock, TextBlock firstProduct, TextBlock secondProduct, TextBlock thirdProduct, TextBlock fourthProduct) {
                var topProducts = context.InvoiceDetails
                    .GroupBy(e => e.ProductId)
                    .Select(g => new {
                        Product = g.FirstOrDefault().Product,
                        TotalSales = g.Sum(e => e.Quantity),
                        Revenue = g.Sum(e => e.Quantity * e.Product.Price)
                    })
                    .OrderByDescending(result => result.TotalSales)
                    .Take(4)
                    .ToList();

                if (topProducts.Count > 0) {
                    firstProduct.Text = $"{topProducts[0]?.Product?.Name}";
                    firstTextBlock.Text = $"{topProducts[0]?.Revenue}";
                }

                if (topProducts.Count > 1) {
                    secondProduct.Text = $"{topProducts[1]?.Product?.Name}";
                    secondTextBlock.Text = $"{topProducts[1]?.Revenue}";
                }

                if (topProducts.Count > 2) {
                    thirdProduct.Text = $"{topProducts[2]?.Product?.Name}";
                    thirdTextBlock.Text = $"{topProducts[2]?.Revenue}";
                }

                if (topProducts.Count > 3) {
                    fourthProduct.Text = $"{topProducts[3]?.Product?.Name}";
                    fourthTextBlock.Text = $"{topProducts[3]?.Revenue}";
                }
            }
        }

        public class MostRecentCustomers {
            private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

            public void UpdateRecentCustomerTextBlocks(TextBlock fifthTextBlock, TextBlock sixthTextBlock, TextBlock seventhTextBlock, TextBlock eighthTextBlock) {
                var recentExports = context.Invoices
                    .OrderByDescending(e => e.Date)
                    .ToList();

                var recentCustomers = recentExports
                    .GroupBy(e => e.Customer)
                    .Select(g => g.FirstOrDefault())
                    .Take(4)
                    .ToList();

                if (recentCustomers.Count > 0)
                    fifthTextBlock.Text = $"{recentCustomers[0]?.Customer?.Name}";
                if (recentCustomers.Count > 1)
                    sixthTextBlock.Text = $"{recentCustomers[1]?.Customer?.Name}";
                if (recentCustomers.Count > 2)
                    seventhTextBlock.Text = $"{recentCustomers[2]?.Customer?.Name}";
                if (recentCustomers.Count > 3)
                    eighthTextBlock.Text = $"{recentCustomers[3]?.Customer?.Name}";
            }
        }

    }
    public class ZeroOrNaNToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is double d) {
                // collapse if NaN or exactly zero
                return (double.IsNaN(d) || d == 0.0)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            // fallback: visible
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
