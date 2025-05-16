using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using static MyQuanLyTrangSuc.ViewModel.MonthlyStockReportWindowLogic;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyStockReportPageLogic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ViewDetailCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ExportCommand { get; }
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private StockReport _selectedStockReport;
        public StockReport SelectedStockReport
        {
            get => _selectedStockReport;
            set
            {
                _selectedStockReport = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<StockReport> _stockReports;
        public ObservableCollection<StockReport> StockReports
        {
            get => _stockReports;
            set
            {
                _stockReports = value;
                OnPropertyChanged(nameof(StockReports));
            }
        }

        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MonthlyStockReportPage monthlyStockReportPage;

        public MonthlyStockReportPageLogic()
        {
            StockReports = new ObservableCollection<StockReport>();
            LoadReportsFromDatabase();
            Console.WriteLine($"Số lượng báo cáo tồn kho: {StockReports.Count}");
        }

        public MonthlyStockReportPageLogic(MonthlyStockReportPage monthlyStockReportPage)
        {
            this.monthlyStockReportPage = monthlyStockReportPage;
            StockReports = new ObservableCollection<StockReport>();
            LoadReportsFromDatabase();
        }

        private void LoadReportsFromDatabase()
        {
            // Lấy tất cả dữ liệu từ database trước
            var allReports = context.StockReports.ToList();
            Console.WriteLine($"Số lượng báo cáo lấy từ database: {allReports.Count}");

            foreach (var report in allReports)
            {
                Console.WriteLine($"Tháng: {report.MonthYear.Month}, Năm: {report.MonthYear.Year}, Tồn kho: {report.FinishStock}");
            }
            // Thực hiện group by trên client side
            var groupedReports = allReports
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1),
                    ProductId = "SUMMARY", // Sử dụng giá trị placeholder cho ProductId
                    BeginStock = g.Sum(r => r.BeginStock),
                    PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    SalesQuantity = g.Sum(r => r.SalesQuantity),
                    FinishStock = g.Sum(r => r.FinishStock)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            // Cập nhật UI thông qua Dispatcher
            Application.Current.Dispatcher.Invoke(() =>
            {
                StockReports.Clear();
                foreach (var report in groupedReports)
                {
                    StockReports.Add(report);
                }
            });
        }

        public void LoadReportDetailsWindow()
        {
            if (monthlyStockReportPage.StockReportDataGrid.SelectedItem is StockReport selectedReport)
            {
                Console.WriteLine($"SelectedStockReport: {selectedReport != null}");

                var month = selectedReport.MonthYear.Month;
                var year = selectedReport.MonthYear.Year;
                string monthYear = $"{month}/{year}";

                var detailedReports = context.StockReports
                    .Where(r => r.MonthYear.Month == month && r.MonthYear.Year == year)
                    .Join(context.Products,
                        report => report.ProductId,
                        product => product.ProductId,
                        (report, product) => new StockReport
                        {
                            ProductId = report.ProductId,
                            BeginStock = report.BeginStock,
                            PurchaseQuantity = report.PurchaseQuantity,
                            SalesQuantity = report.SalesQuantity,
                            FinishStock = report.FinishStock,
                            Product = product // Gán thông tin sản phẩm
                        })
                    .ToList();

                if (detailedReports.Count == 0)
                {
                    MessageBox.Show($"Không có báo cáo nào cho tháng {month}/{year}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Mở cửa sổ hiển thị chi tiết báo cáo tồn kho
                MonthlyStockReportWindow reportWindow = new MonthlyStockReportWindow(detailedReports, monthYear);
                reportWindow.ShowDialog();
            }
        }


        public void CreateOrUpdateCurrentMonthReport()
        {
            DateTime currentDate = DateTime.Now;
            var reportDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Kiểm tra và xác nhận với người dùng
            if (context.StockReports.Any(r => r.MonthYear.Month == reportDate.Month && r.MonthYear.Year == reportDate.Year))
            {
                var confirmResult = MessageBox.Show($"Đã có báo cáo tồn kho cho tháng {reportDate.Month}/{reportDate.Year}. Cập nhật lại?",
                                                 "Xác nhận",
                                                 MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);
                if (confirmResult != MessageBoxResult.Yes) return;
            }

            try
            {
                var products = context.Products.ToList();
                int reportCreated = 0;
                int reportUpdated = 0;

                foreach (var product in products)
                {
                    var existingReport = context.StockReports
                        .FirstOrDefault(r => r.MonthYear.Month == reportDate.Month &&
                                           r.MonthYear.Year == reportDate.Year &&
                                           r.ProductId == product.ProductId);

                    // Tính BeginStock từ tổng nhập kho (ImportDetails)
                    var beginStock = context.ImportDetails
                        .Where(pd => pd.Import.Date < reportDate && // Tất cả nhập kho trước tháng báo cáo
                                    pd.ProductId == product.ProductId)
                        .Sum(pd => pd.Quantity ?? 0);

                    // Tính tổng đã bán (SalesQuantity) từ InvoiceDetails
                    var salesQuantity = context.InvoiceDetails
                        .Where(od => od.Invoice.Date.Month == reportDate.Month &&
                                    od.Invoice.Date.Year == reportDate.Year &&
                                    od.ProductId == product.ProductId)
                        .Sum(od => od.Quantity ?? 0);

                    // Tính tổng nhập kho trong tháng (PurchaseQuantity)
                    var purchaseQuantity = context.ImportDetails
                        .Where(pd => pd.Import.Date.Month == reportDate.Month &&
                                    pd.Import.Date.Year == reportDate.Year &&
                                    pd.ProductId == product.ProductId)
                        .Sum(pd => pd.Quantity ?? 0);

                    // Tính tồn cuối kỳ
                    var finishStock = beginStock + purchaseQuantity - salesQuantity;

                    if (existingReport != null)
                    {
                        existingReport.BeginStock = beginStock;
                        existingReport.PurchaseQuantity = purchaseQuantity;
                        existingReport.SalesQuantity = salesQuantity;
                        existingReport.FinishStock = finishStock;
                        context.SaveChangesEdited(existingReport);
                        reportUpdated++;
                    }
                    else
                    {
                        var newReport = new StockReport
                        {
                            MonthYear = reportDate,
                            ProductId = product.ProductId,
                            BeginStock = beginStock,
                            PurchaseQuantity = purchaseQuantity,
                            SalesQuantity = salesQuantity,
                            FinishStock = finishStock,
                        };
                        context.StockReports.Add(newReport);
                        context.SaveChangesAdded(newReport);
                        reportCreated++;
                    }
                }

                LoadReportsFromDatabase();

                MessageBox.Show($"Đã lưu báo cáo tồn kho tháng {reportDate.Month}/{reportDate.Year}\n" +
                              $"Tạo mới: {reportCreated} | Cập nhật: {reportUpdated}",
                              "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu báo cáo: {ex.Message}\n{ex.InnerException?.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void DeleteStockReport()
        {
            if (monthlyStockReportPage == null || monthlyStockReportPage.StockReportDataGrid == null)
            {
                MessageBox.Show("LỖI: Trang báo cáo chưa được khởi tạo!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (monthlyStockReportPage.StockReportDataGrid.SelectedItem is not StockReport selectedReport)
            {
                MessageBox.Show("Vui lòng chọn một báo cáo để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tất cả báo cáo tồn kho của tháng {selectedReport.MonthYear.Month}/{selectedReport.MonthYear.Year}?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Tìm tất cả báo cáo của tháng/năm đó
                    var reportsToDelete = context.StockReports.Where(r => r.MonthYear.Month == selectedReport.MonthYear.Month &&
                                                                          r.MonthYear.Year == selectedReport.MonthYear.Year).ToList();

                    if (!reportsToDelete.Any())
                    {
                        MessageBox.Show("Không tìm thấy báo cáo nào để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Xóa tất cả báo cáo tìm thấy
                    context.StockReports.RemoveRange(reportsToDelete);
                    context.SaveChanges();

                    // Cập nhật danh sách hiển thị trên UI
                    for (int i = StockReports.Count - 1; i >= 0; i--)
                    {
                        if (StockReports[i].MonthYear.Month == selectedReport.MonthYear.Month &&
                            StockReports[i].MonthYear.Year == selectedReport.MonthYear.Year)
                        {
                            StockReports.RemoveAt(i);
                        }
                    }
                    OnPropertyChanged(nameof(StockReports));

                    MessageBox.Show("Đã xóa tất cả báo cáo tồn kho của tháng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (DbUpdateConcurrencyException)
                {
                    MessageBox.Show("Dữ liệu đã bị thay đổi hoặc xóa trước đó. Hãy tải lại danh sách báo cáo!", "Lỗi đồng bộ dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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