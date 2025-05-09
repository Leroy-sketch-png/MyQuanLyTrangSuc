using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel {
    public class DashboardPageLogic {

        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly DashboardPage dashboardPage;

        private readonly TotalProductSold totalProductSold;
        private readonly RevenueCalculator revenueCalculator;

        private Func<double, string> DateFormatter;
        private Func<double, string> NetFormatter;
        private SeriesCollection SeriesCollection;
        private LineSeries lineSeries;

        private DateTime? selectedFromDate;
        private DateTime? selectedUntilDate;

        //DataContext Zone
        public DateTime? SelectedFromDate {
            get => selectedFromDate;
            set {
                if (value != selectedFromDate) {
                    selectedFromDate = value;
                    LoadData();
                }
            }
        }

        public DateTime? SelectedUntilDate {
            get => selectedUntilDate;
            set {
                if (value != selectedUntilDate) {
                    selectedUntilDate = value;
                    LoadData();
                }
            }
        }

        //
        public DashboardPageLogic(DashboardPage dashboardPage) {
            this.dashboardPage = dashboardPage;

            totalProductSold = new TotalProductSold();
            revenueCalculator = new RevenueCalculator();

            DateFormatter = value => new DateTime((long)value).ToString("dd-MM");
            NetFormatter = value => $"{value:N}";

            var dayConfig = Mappers.Xy<DateTimePoint>()
                .X(dateModel => dateModel.DateTime.Ticks)
                .Y(dateModel => dateModel.Value);

            lineSeries = new LineSeries {
                Values = new ChartValues<DateTimePoint>(),
                Configuration = dayConfig
            };

            SeriesCollection = new SeriesCollection { lineSeries };

            selectedFromDate = DateTime.Now;
            selectedUntilDate = DateTime.Now;

            BindDataToUI();
        }

        private void LoadData() {
            lineSeries.Values.Clear();
            if (selectedFromDate == null || selectedUntilDate == null) return;

            var importData = context.Imports
                .Where(i => i.Date >= selectedFromDate && i.Date <= selectedUntilDate)
                .Select(i => new { i.Date, Value = -i.TotalAmount })
                .ToList();

            var exportData = context.Invoices
                .Where(e => e.Date >= selectedFromDate && e.Date <= selectedUntilDate)
                .Select(e => new { e.Date, Value = e.TotalAmount })
                .ToList();

            var combinedData = importData
                .Select(i => new DateTimePoint(i.Date ?? DateTime.MinValue, (double)i.Value))
                .Concat(exportData.Select(e => new DateTimePoint(e.Date ?? DateTime.MinValue, (double)e.Value)))
                .OrderBy(dp => dp.DateTime)
                .ToList();

            double cumulativeValue = 0;
            foreach (var dataPoint in combinedData) {
                cumulativeValue += dataPoint.Value;
                lineSeries.Values.Add(new DateTimePoint(dataPoint.DateTime, cumulativeValue));
            }
        }

        private void BindDataToUI() {
            dashboardPage.DataContext = this;

            dashboardPage.NetChart.Series = SeriesCollection;
            dashboardPage.NetChartAxisX.LabelFormatter = DateFormatter;
            dashboardPage.NetChartAxisY.LabelFormatter = NetFormatter;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
}
