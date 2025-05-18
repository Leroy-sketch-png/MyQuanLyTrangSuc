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
using static MyQuanLyTrangSuc.ViewModel.MonthlyRevenueReportWindowLogic;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyRevenueReportPageLogic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand ViewDetailCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ExportCommand { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private RevenueReport _selectedRevenueReport;
        public RevenueReport SelectedRevenueReport
        {
            get => _selectedRevenueReport;
            set
            {
                _selectedRevenueReport = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<RevenueReport> _revenueReports;
        public ObservableCollection<RevenueReport> RevenueReports
        {
            get => _revenueReports;
            set
            {
                _revenueReports = value;
                OnPropertyChanged(nameof(RevenueReports));
            }
        }

        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MonthlyRevenueReportPage monthlyRevenueReportPage;

        public MonthlyRevenueReportPageLogic()
        {
            RevenueReports = new ObservableCollection<RevenueReport>();
            LoadReportsFromDatabase();
            Console.WriteLine($"Số lượng báo cáo tồn kho: {RevenueReports.Count}");
        }

        public MonthlyRevenueReportPageLogic(MonthlyRevenueReportPage monthlyRevenueReportPage)
        {
            this.monthlyRevenueReportPage = monthlyRevenueReportPage;
            RevenueReports = new ObservableCollection<RevenueReport>();
            LoadReportsFromDatabase();
        }

        private void LoadReportsFromDatabase()
        {
            try
            {
                // Load trực tiếp các báo cáo từ database không qua group by
                var allReports = context.RevenueReports
                    .OrderByDescending(r => r.MonthYear)
                    .ToList();

                RevenueReports.Clear();
                foreach (var report in allReports)
                {
                    RevenueReports.Add(new RevenueReport
                    {
                        RevenueReportId = report.RevenueReportId, // Giữ nguyên ID
                        MonthYear = report.MonthYear,
                        TotalRevenue = report.TotalRevenue,
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
            if (monthlyRevenueReportPage.RevenueReportDataGrid.SelectedItem is RevenueReport selectedReport)
            {
                Console.WriteLine($"SelectedRevenueReport: {selectedReport != null}");
                var revenueReportId = selectedReport.RevenueReportId;
                var month = selectedReport.MonthYear.Month;
                var year = selectedReport.MonthYear.Year;

                string monthYear = $"{month}/{year}";

                // Lấy thông tin chi tiết từ bảng RevenueReportDetail
                var detailedReports = context.RevenueReportDetails
                    .Where(detail => detail.RevenueReportId == revenueReportId)
                    .Join(context.Products,
                        detail => detail.ProductId,
                        product => product.ProductId,
                        (detail, product) => new
                        {
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            Revenue = detail.Revenue,
                            Percentage = detail.Percentage,
                            Product = product
                        })
                    .ToList();

                if (detailedReports.Count == 0)
                {
                    MessageBox.Show($"Không có chi tiết báo cáo nào cho tháng {month}/{year}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Chuyển đổi sang dạng dữ liệu mà MonthlyRevenueReportWindow có thể hiển thị
                var reportViewModels = detailedReports.Select(detail => new RevenueReportDetail
                {
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Revenue = detail.Revenue,
                    Percentage = detail.Percentage,
                    Product = detail.Product
                }).ToList();

                // Mở cửa sổ hiển thị chi tiết báo cáo tồn kho
                MonthlyRevenueReportWindow reportWindow = new MonthlyRevenueReportWindow(reportViewModels, monthYear);
                reportWindow.ShowDialog();
            }
        }

        public void CreateOrUpdateCurrentMonthReport()
        {
            DateTime currentDate = DateTime.Now;
            var reportDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Kiểm tra và xác nhận với người dùng
            bool isExistingReport = context.RevenueReports
                .Any(r => r.MonthYear.Month == reportDate.Month && r.MonthYear.Year == reportDate.Year);

            if (isExistingReport)
            {
                var confirmResult = MessageBox.Show(
                    $"Đã có báo cáo doanh thu cho tháng {reportDate.Month}/{reportDate.Year}. Cập nhật lại?",
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
                        decimal totalRevenue = 0;
                        int detailCount = 0;

                        // Xử lý báo cáo chính
                        var revenueReport = isExistingReport
                            ? context.RevenueReports
                                .Include(r => r.RevenueReportDetails)
                                .First(r => r.MonthYear.Month == reportDate.Month &&
                                           r.MonthYear.Year == reportDate.Year)
                            : new RevenueReport
                            {
                                RevenueReportId = GenerateRevenueReportId(),
                                MonthYear = reportDate,
                                TotalRevenue = 0,
                                RevenueReportDetails = new List<RevenueReportDetail>()
                            };

                        // Xóa chi tiết cũ nếu là cập nhật
                        if (isExistingReport)
                        {
                            context.RevenueReportDetails.RemoveRange(revenueReport.RevenueReportDetails);
                        }
                        else
                        {
                            context.RevenueReports.Add(revenueReport);
                        }

                        // Tạo chi tiết mới cho từng sản phẩm
                        foreach (var product in products)
                        {
                            var quantity = CalculateQuantity(product.ProductId, reportDate);
                            var revenue = CalculateRevenue(product.ProductId, quantity);
                            var percentage = 0m; // Gán trước bằng 0 rồi cập nhật sau khi vòng foreach hoàn tất

                            revenueReport.RevenueReportDetails.Add(new RevenueReportDetail
                            {
                                RevenueReportId = revenueReport.RevenueReportId,
                                ProductId = product.ProductId,
                                Quantity = quantity,
                                Revenue = revenue,
                                Percentage = percentage
                            });

                            totalRevenue += revenue;
                            detailCount++;
                        }

                        // Cập nhật tổng số liệu
                        revenueReport.TotalRevenue = totalRevenue;

                        // Cập nhật lại percentage
                        if (revenueReport.TotalRevenue > 0)
                        {
                            foreach (var detail in revenueReport.RevenueReportDetails)
                            {
                                // Làm tròn 2 chữ số thập phân
                                detail.Percentage = Math.Round((detail.Revenue ?? 0) / revenueReport.TotalRevenue * 100, 2);
                            }
                        }


                        context.SaveChanges();
                        transaction.Commit();

                        // Hiển thị kết quả
                        string action = isExistingReport ? "cập nhật" : "tạo mới";
                        MessageBox.Show(
                            $"Đã {action} thành công báo cáo doanh thu tháng {reportDate.Month}/{reportDate.Year}\n" +
                            $"• Số sản phẩm: {detailCount}\n" +
                            $"• Doanh thu tháng: {totalRevenue}",
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
                        // Logger.Error(ex, "Lỗi khi tạo/cập nhật báo cáo doanh thu");
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
        private int CalculateQuantity(string productId, DateTime reportDate)
        {
            int imported = context.ImportDetails
                .Where(d => d.ProductId == productId && d.Import.Date.Month == reportDate.Month && d.Import.Date.Year == reportDate.Year)
                .Sum(d => d.Quantity ?? 0);

            int sold = context.InvoiceDetails
                .Where(d => d.ProductId == productId && d.Invoice.Date.Month == reportDate.Month && d.Invoice.Date.Year == reportDate.Year)
                .Sum(d => d.Quantity ?? 0);

            return sold - imported;
        }

        private decimal CalculateRevenue(string productId, int quantity)
        {
            var product = context.Products.FirstOrDefault(p => p.ProductId == productId);
            return product != null ? product.Price * quantity : 0;
        }

        private string GenerateRevenueReportId()
        {
            var maxId = context.RevenueReports.Max(r => (int?)Convert.ToInt32(r.RevenueReportId)) ?? 0;
            return (maxId + 1).ToString("D6");
        }

        public void DeleteRevenueReport()
        {
            if (monthlyRevenueReportPage == null || monthlyRevenueReportPage.RevenueReportDataGrid == null)
            {
                MessageBox.Show("LỖI: Trang báo cáo chưa được khởi tạo!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (monthlyRevenueReportPage.RevenueReportDataGrid.SelectedItem is not RevenueReport selectedReport)
            {
                MessageBox.Show("Vui lòng chọn một báo cáo để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tất cả báo cáo doanh thu của tháng {selectedReport.MonthYear.Month}/{selectedReport.MonthYear.Year}?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Tìm tất cả báo cáo của tháng/năm đó
                    var reportsToDelete = context.RevenueReports.Where(r => r.MonthYear.Month == selectedReport.MonthYear.Month &&
                                                                          r.MonthYear.Year == selectedReport.MonthYear.Year).ToList();

                    if (!reportsToDelete.Any())
                    {
                        MessageBox.Show("Không tìm thấy báo cáo nào để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Xóa tất cả báo cáo tìm thấy
                    context.RevenueReports.RemoveRange(reportsToDelete);
                    context.SaveChanges();

                    // Cập nhật danh sách hiển thị trên UI
                    for (int i = RevenueReports.Count - 1; i >= 0; i--)
                    {
                        if (RevenueReports[i].MonthYear.Month == selectedReport.MonthYear.Month &&
                            RevenueReports[i].MonthYear.Year == selectedReport.MonthYear.Year)
                        {
                            RevenueReports.RemoveAt(i);
                        }
                    }

                    MessageBox.Show("Đã xóa tất cả báo cáo doanh thu của tháng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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

            var reportsFromDb = context.RevenueReports
                .Where(r => r.MonthYear.Month == monthNumber)
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new RevenueReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1)
                    //ProductId = "SUMMARY",
                    //Quantity = g.Sum(r => r.Quantity),
                    //Revenue = g.Sum(r => r.Revenue),
                    //Percentage = g.Sum(r => r.Percentage)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                RevenueReports.Clear();
                foreach (var report in reportsFromDb)
                {
                    RevenueReports.Add(report);
                }
            });
        }

        public void ReportsSearchByYear(string year)
        {
            if (!int.TryParse(year, out int yearNumber)) return;

            var reportsFromDb = context.RevenueReports
                .Where(r => r.MonthYear.Year == yearNumber)
                .GroupBy(r => new { r.MonthYear.Month, r.MonthYear.Year })
                .Select(g => new RevenueReport
                {
                    MonthYear = new DateTime(g.Key.Year, g.Key.Month, 1)
                    //ProductId = "SUMMARY",
                    //Quantity = g.Sum(r => r.Quantity),
                    //Revenue = g.Sum(r => r.Revenue),
                    //Percentage = g.Sum(r => r.Percentage)
                })
                .OrderByDescending(r => r.MonthYear)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                RevenueReports.Clear();
                foreach (var report in reportsFromDb)
                {
                    RevenueReports.Add(report);
                }
            });
        }

    }
}
