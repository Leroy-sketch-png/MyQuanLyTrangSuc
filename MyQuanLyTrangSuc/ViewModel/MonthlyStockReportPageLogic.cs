using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

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
                        (report, product) => new
                        {
                            ProductName = product.Name,
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

        public void ExportToExcel()
        {
            try
            {
                // Implement Excel export using EPPlus or similar library
                // Example structure:
                /*
                var excelPackage = new ExcelPackage();
                var worksheet = excelPackage.Workbook.Worksheets.Add("Stock Reports");
                
                // Add headers
                worksheet.Cells[1, 1] = "Month/Year";
                worksheet.Cells[1, 2] = "Beginning Stock";
                worksheet.Cells[1, 3] = "Purchased";
                worksheet.Cells[1, 4] = "Sold";
                worksheet.Cells[1, 5] = "Ending Stock";
                
                // Add data
                int row = 2;
                foreach (var report in StockReports)
                {
                    worksheet.Cells[row, 1] = report.MonthYear?.ToString("MM/yyyy");
                    worksheet.Cells[row, 2] = report.BeginStock;
                    worksheet.Cells[row, 3] = report.PurchaseQuantity;
                    worksheet.Cells[row, 4] = report.SalesQuantity;
                    worksheet.Cells[row, 5] = report.FinishStock;
                    row++;
                }
                
                // Save the file
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                {
                    FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                    excelPackage.SaveAs(excelFile);
                }
                */

                MessageBox.Show("Excel export would be implemented here", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}