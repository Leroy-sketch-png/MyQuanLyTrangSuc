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
            try
            {
                // Sửa lại phần này: Load trực tiếp các báo cáo từ database không qua group by
                var allReports = context.StockReports
                    .OrderByDescending(r => r.MonthYear)
                    .ToList();

                StockReports.Clear();
                foreach (var report in allReports)
                {
                    StockReports.Add(new StockReport
                    {
                        StockReportId = report.StockReportId, // Giữ nguyên ID
                        MonthYear = report.MonthYear,
                        TotalBeginStock = report.TotalBeginStock,
                        TotalFinishStock = report.TotalFinishStock
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải báo cáo: {ex.Message}");
                MessageBox.Show($"Lỗi khi tải báo cáo: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadReportDetailsWindow()
        {
            if (monthlyStockReportPage.StockReportDataGrid.SelectedItem is StockReport selectedReport)
            {
                Console.WriteLine($"SelectedStockReport: {selectedReport != null}");
                var stockReportId = selectedReport.StockReportId;
                var month = selectedReport.MonthYear.Value.Month;
                var year = selectedReport.MonthYear.Value.Year;

                string monthYear = $"{month}/{year}";

                // Lấy thông tin chi tiết từ bảng StockReportDetail
                var detailedReports = context.StockReportDetails
                    .Where(detail => detail.StockReportId == stockReportId)
                    .Join(context.Products,
                        detail => detail.ProductId,
                        product => product.ProductId,
                        (detail, product) => new
                        {
                            ProductId = detail.ProductId,
                            BeginStock = detail.BeginStock,
                            PurchaseQuantity = detail.ImportQuantity,
                            SalesQuantity = detail.SaleQuantity,
                            FinishStock = detail.FinishStock,
                            Product = product
                        })
                    .ToList();

                if (detailedReports.Count == 0)
                {
                    MessageBox.Show($"Không có chi tiết báo cáo nào cho tháng {month}/{year}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Chuyển đổi sang dạng dữ liệu mà MonthlyStockReportWindow có thể hiển thị
                var reportViewModels = detailedReports.Select(detail => new StockReportDetail
                {
                    ProductId = detail.ProductId,
                    BeginStock = detail.BeginStock,
                    ImportQuantity = detail.PurchaseQuantity,
                    SaleQuantity = detail.SalesQuantity,
                    FinishStock = detail.FinishStock,
                    Product = detail.Product
                }).ToList();

                // Mở cửa sổ hiển thị chi tiết báo cáo tồn kho
                MonthlyStockReportWindow reportWindow = new MonthlyStockReportWindow(reportViewModels, monthYear);
                reportWindow.ShowDialog();
            }
        }


        public void CreateOrUpdateCurrentMonthReport()
        {
            DateTime currentDate = DateTime.Now;
            var reportDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Kiểm tra và xác nhận với người dùng
            bool isExistingReport = context.StockReports
                .Any(r => r.MonthYear.Value.Month == reportDate.Month && r.MonthYear.Value.Year == reportDate.Year);

            if (isExistingReport)
            {
                var confirmResult = MessageBox.Show(
                    $"Đã có báo cáo tồn kho cho tháng {reportDate.Month}/{reportDate.Year}. Cập nhật lại?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult != MessageBoxResult.Yes) return;
            }

            try
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var products = context.Products.AsNoTracking().ToList();
                        int totalBeginStock = 0;
                        int totalFinishStock = 0;
                        int detailCount = 0;

                        // Xử lý báo cáo chính
                        var stockReport = isExistingReport
                            ? context.StockReports
                                .Include(r => r.StockReportDetails)
                                .First(r => r.MonthYear.Value.Month == reportDate.Month &&
                                           r.MonthYear.Value.Year == reportDate.Year)
                            : new StockReport
                            {
                                StockReportId = GenerateStockReportId(),
                                MonthYear = reportDate,
                                TotalBeginStock = 0,
                                TotalFinishStock = 0,
                                StockReportDetails = new List<StockReportDetail>()
                            };

                        // Xóa chi tiết cũ nếu là cập nhật
                        if (isExistingReport)
                        {
                            context.StockReportDetails.RemoveRange(stockReport.StockReportDetails);
                        }
                        else
                        {
                            context.StockReports.Add(stockReport);
                        }

                        // Tạo chi tiết mới cho từng sản phẩm
                        foreach (var product in products)
                        {
                            var beginStock = CalculateBeginStock(product.ProductId, reportDate);
                            var salesQty = CalculateSalesQuantity(product.ProductId, reportDate);
                            var purchaseQty = CalculatePurchaseQuantity(product.ProductId, reportDate);
                            var finishStock = beginStock + purchaseQty - salesQty;

                            stockReport.StockReportDetails.Add(new StockReportDetail
                            {
                                StockReportId = stockReport.StockReportId,
                                ProductId = product.ProductId,
                                BeginStock = beginStock,
                                FinishStock = finishStock,
                                ImportQuantity = purchaseQty,
                                SaleQuantity = salesQty
                            });

                            totalBeginStock += beginStock;
                            totalFinishStock += finishStock;
                            detailCount++;
                        }

                        // Cập nhật tổng số liệu
                        stockReport.TotalBeginStock = totalBeginStock;
                        stockReport.TotalFinishStock = totalFinishStock;

                        context.SaveChanges();
                        transaction.Commit();

                        // Hiển thị kết quả
                        string action = isExistingReport ? "cập nhật" : "tạo mới";
                        MessageBox.Show(
                            $"Đã {action} thành công báo cáo tồn kho tháng {reportDate.Month}/{reportDate.Year}\n" +
                            $"• Số sản phẩm: {detailCount}\n" +
                            $"• Tồn đầu kỳ: {totalBeginStock}\n" +
                            $"• Tồn cuối kỳ: {totalFinishStock}",
                            "Thành công",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        LoadReportsFromDatabase();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show(
                            $"Lỗi khi xử lý báo cáo: {ex.Message}\n" +
                            $"Vui lòng thử lại hoặc liên hệ quản trị hệ thống.",
                            "Lỗi xử lý dữ liệu",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        // Ghi log lỗi nếu cần
                        // Logger.Error(ex, "Lỗi khi tạo/cập nhật báo cáo tồn kho");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Lỗi hệ thống: {ex.Message}\n" +
                    $"Không thể kết nối đến cơ sở dữ liệu.",
                    "Lỗi nghiêm trọng",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Các phương thức hỗ trợ tính toán
        private int CalculateBeginStock(string productId, DateTime reportDate)
        {
            return context.ImportDetails
                .Join(context.Imports,
                    detail => detail.ImportId,
                    import => import.ImportId,
                    (detail, import) => new { Detail = detail, Import = import })
                .Where(joined => joined.Import.Date < reportDate && joined.Detail.ProductId == productId)
                .Select(joined => joined.Detail.Quantity ?? 0)
                .FirstOrDefault(); // Tránh lỗi null
        }

        private int CalculateSalesQuantity(string productId, DateTime reportDate)
        {
            return context.InvoiceDetails
                .Join(context.Invoices,
                    detail => detail.InvoiceId,
                    invoice => invoice.InvoiceId,
                    (detail, invoice) => new { Detail = detail, Invoice = invoice })
                .Where(joined => joined.Invoice.Date.Value.Month == reportDate.Month &&
                               joined.Invoice.Date.Value.Year == reportDate.Year &&
                               joined.Detail.ProductId == productId)
                .Select(joined => joined.Detail.Quantity ?? 0)
                .DefaultIfEmpty(0) // Tránh lỗi nếu không có dữ liệu
                .Sum();
        }

        private int CalculatePurchaseQuantity(string productId, DateTime reportDate)
        {
            return context.ImportDetails
                .Join(context.Imports,
                    detail => detail.ImportId,
                    import => import.ImportId,
                    (detail, import) => new { Detail = detail, Import = import })
                .Where(joined => joined.Import.Date.Value.Month == reportDate.Month &&
                               joined.Import.Date.Value.Year == reportDate.Year &&
                               joined.Detail.ProductId == productId)
                .Select(joined => joined.Detail.Quantity ?? 0)
                .DefaultIfEmpty(0) // Tránh lỗi nếu không có dữ liệu
                .Sum();
        }

        private string GenerateStockReportId()
        {
            var maxId = context.StockReports.Max(r => (int?)Convert.ToInt32(r.StockReportId)) ?? 0;
            return (maxId + 1).ToString("D6");
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

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tất cả báo cáo tồn kho của tháng {selectedReport.MonthYear.Value.Month}/{selectedReport.MonthYear.Value.Year}?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Tìm tất cả báo cáo của tháng/năm đó
                    var reportsToDelete = context.StockReports.Where(r => r.MonthYear.Value.Month == selectedReport.MonthYear.Value.Month &&
                                                                          r.MonthYear.Value.Year == selectedReport.MonthYear.Value.Year).ToList();

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
                        if (StockReports[i].MonthYear.Value.Month == selectedReport.MonthYear.Value.Month &&
                            StockReports[i].MonthYear.Value.Year == selectedReport.MonthYear.Value.Year)
                        {
                            StockReports.RemoveAt(i);
                        }
                    }

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
                .Where(r => r.MonthYear.Value.Month == monthNumber)
                .GroupBy(r => new { r.MonthYear.Value.Month, r.MonthYear.Value.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1)
                    //ProductId = "SUMMARY",
                    //BeginStock = g.Sum(r => r.BeginStock),
                    //PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    //SalesQuantity = g.Sum(r => r.SalesQuantity),
                    //FinishStock = g.Sum(r => r.FinishStock)
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
                .Where(r => r.MonthYear.Value.Year == yearNumber)
                .GroupBy(r => new { r.MonthYear.Value.Month, r.MonthYear.Value.Year })
                .Select(g => new StockReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1)
                    //ProductId = "SUMMARY",
                    //BeginStock = g.Sum(r => r.BeginStock),
                    //PurchaseQuantity = g.Sum(r => r.PurchaseQuantity),
                    //SalesQuantity = g.Sum(r => r.SalesQuantity),
                    //FinishStock = g.Sum(r => r.FinishStock)
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