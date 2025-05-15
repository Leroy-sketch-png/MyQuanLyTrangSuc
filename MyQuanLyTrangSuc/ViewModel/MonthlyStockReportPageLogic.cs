using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using static MyQuanLyTrangSuc.ViewModel.MonthlyStockReportWindowLogic;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportPageLogic
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MonthlyStockReportPage monthlyStockReportPage;

        public ObservableCollection<StockReport> StockReports { get; set; }

        public MonthlyStockReportPageLogic()
        {
            StockReports = new ObservableCollection<StockReport>();
            LoadReportsFromDatabase();
        }

        public MonthlyStockReportPageLogic(MonthlyStockReportPage monthlyStockReportPage)
        {
            this.monthlyStockReportPage = monthlyStockReportPage;
            StockReports = new ObservableCollection<StockReport>();
            LoadReportsFromDatabase();
        }

        private void LoadReportsFromDatabase()
        {
            // Group by month and year since the StockReport entity has composite key (MonthYear, ProductId)
            var reportsFromDb = context.StockReports
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1),
                    ProductId = "SUMMARY", // Using a placeholder since we need to group
                    BeginStock = g.Sum(r => r.BeginStock),
                    PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    SalesQuantity = g.Sum(r => r.SalesQuantity),
                    FinishStock = g.Sum(r => r.FinishStock)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                StockReports.Clear();
                foreach (var report in reportsFromDb)
                {
                    StockReports.Add(report);
                }
            });
        }

        public void LoadReportDetailsWindow()
        {
            if (monthlyStockReportPage.servicesDataGrid.SelectedItem is StockReport selectedReport)
            {
                // Get all reports for the selected month/year
                var month = selectedReport.MonthYear.Month;
                var year = selectedReport.MonthYear.Year;

                var detailedReports = context.StockReports
                    .Where(r => r.MonthYear.Month == month && r.MonthYear.Year == year)
                    .Join(context.Products,
                            report => report.ProductId,
                            product => product.ProductId,
                            (report, product) => new StockReport // ← Sửa thành class cụ thể
                    {
                        BeginStock = report.BeginStock,
                        PurchaseQuantity = report.PurchaseQuantity,
                        SalesQuantity = report.SalesQuantity,
                        FinishStock = report.FinishStock
                    })
                    .ToList();

                MonthlyStockReportWindow MonthlyStockReportWindow = new MonthlyStockReportWindow(detailedReports, $"{month}/{year}");
                MonthlyStockReportWindow.ShowDialog();
            }
        }

        public void CreateOrUpdateCurrentMonthReport()
        {
            try
            {
                // Lấy tháng hiện tại
                DateTime currentDate = DateTime.Now;
                var reportDate = new DateTime(currentDate.Year, currentDate.Month, 1);

                // Lấy danh sách tất cả sản phẩm từ database
                var products = context.Products.ToList();
                int reportCreated = 0;
                int reportUpdated = 0;

                foreach (var product in products)
                {
                    // Kiểm tra xem báo cáo đã tồn tại cho sản phẩm này trong tháng hiện tại chưa
                    var existingReport = context.StockReports
                        .FirstOrDefault(r => r.MonthYear.Month == reportDate.Month &&
                                             r.MonthYear.Year == reportDate.Year &&
                                             r.ProductId == product.ProductId);

                    // Lấy báo cáo tháng trước để lấy số tồn đầu kỳ
                    var lastMonth = reportDate.AddMonths(-1);
                    var lastMonthReport = context.StockReports
                        .FirstOrDefault(r => r.MonthYear.Month == lastMonth.Month &&
                                             r.MonthYear.Year == lastMonth.Year &&
                                             r.ProductId == product.ProductId);

                    // Tính toán các giá trị cho báo cáo
                    var beginStock = lastMonthReport?.FinishStock ?? 0;

                    // Tính tổng số lượng nhập trong tháng
                    var purchaseQuantity = context.ImportDetails
                        .Where(pd => pd.Import.Date.Month == reportDate.Month &&
                                     pd.Import.Date.Year == reportDate.Year &&
                                     pd.ProductId == product.ProductId)
                        .Sum(pd => pd.Quantity ?? 0);

                    // Tính tổng số lượng bán trong tháng
                    var salesQuantity = context.InvoiceDetails
                        .Where(od => od.Invoice.Date.Month == reportDate.Month &&
                                     od.Invoice.Date.Year == reportDate.Year &&
                                     od.ProductId == product.ProductId)
                        .Sum(od => od.Quantity ?? 0);

                    // Tính số tồn cuối kỳ
                    var finishStock = beginStock + purchaseQuantity - salesQuantity;

                    if (existingReport != null)
                    {
                        // Cập nhật báo cáo nếu đã tồn tại
                        existingReport.BeginStock = beginStock;
                        existingReport.PurchaseQuantity = purchaseQuantity;
                        existingReport.SalesQuantity = salesQuantity;
                        existingReport.FinishStock = finishStock;
                        reportUpdated++;
                    }
                    else
                    {
                        // Tạo báo cáo mới nếu chưa tồn tại
                        var newReport = new StockReport
                        {
                            MonthYear = reportDate,
                            ProductId = product.ProductId,
                            BeginStock = beginStock,
                            PurchaseQuantity = purchaseQuantity,
                            SalesQuantity = salesQuantity,
                            FinishStock = finishStock,
                        };

                        // Thêm báo cáo mới vào database
                        context.StockReports.Add(newReport);
                        reportCreated++;
                    }
                }

                // Lưu tất cả thay đổi vào database
                context.SaveChanges();

                // Cập nhật lại dữ liệu hiển thị
                LoadReportsFromDatabase();

                // Hiển thị thông báo
                string message = $"Hoàn thành tạo báo cáo tồn kho tháng {reportDate.Month}/{reportDate.Year}:\n" +
                                $"- Tạo mới: {reportCreated} báo cáo\n" +
                                $"- Cập nhật: {reportUpdated} báo cáo";

                MessageBox.Show(message, "Tạo báo cáo tồn kho", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo báo cáo tồn kho: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ReportsSearchByMonth(string month)
        {
            if (!int.TryParse(month, out int monthNumber)) return;

            var reportsFromDb = context.StockReports
                .Where(r => r.MonthYear.Month == monthNumber)
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1),
                    ProductId = "SUMMARY",
                    BeginStock = g.Sum(r => r.BeginStock),
                    PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    SalesQuantity = g.Sum(r => r.SalesQuantity),
                    FinishStock = g.Sum(r => r.FinishStock)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                StockReports.Clear();
                foreach (var report in reportsFromDb)
                {
                    StockReports.Add(report);
                }
            });
        }

        public void ReportsSearchByYear(string year)
        {
            if (!int.TryParse(year, out int yearNumber)) return;

            var reportsFromDb = context.StockReports
                .Where(r => r.MonthYear.Year == yearNumber)
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1),
                    ProductId = "SUMMARY",
                    BeginStock = g.Sum(r => r.BeginStock),
                    PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    SalesQuantity = g.Sum(r => r.SalesQuantity),
                    FinishStock = g.Sum(r => r.FinishStock)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                StockReports.Clear();
                foreach (var report in reportsFromDb)
                {
                    StockReports.Add(report);
                }
            });
        }  
    }
}