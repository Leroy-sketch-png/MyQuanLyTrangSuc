using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class MonthlyRevenueReportPageLogic : INotifyPropertyChanged
    {
        private ObservableCollection<RevenueReport> _revenueReports;
        public ObservableCollection<RevenueReport> RevenueReports
        {
            get => _revenueReports;
            set
            {
                if (_revenueReports != value)
                {
                    _revenueReports = value;
                    OnPropertyChanged(nameof(RevenueReports));
                }
            }
        }
        private ObservableCollection<RevenueReport> _allRevenueReports;
        public ObservableCollection<RevenueReport> AllRevenueReports
        {
            get => _allRevenueReports;
            set
            {
                if (_allRevenueReports != value)
                {
                    _allRevenueReports = value;
                    OnPropertyChanged(nameof(AllRevenueReports));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private MonthlyRevenueReportPage monthlyRevenueReportPage;

        public MonthlyRevenueReportPageLogic(MonthlyRevenueReportPage page)
        {
            monthlyRevenueReportPage = page;
            LoadReportsFromDatabase();
        }

        public void LoadReportsFromDatabase()
        {
            try
            {
                RevenueReports = new ObservableCollection<RevenueReport>(
                    context.RevenueReports
                        .Where(r => !r.IsDeleted && r.MonthYear.HasValue)
                        .OrderByDescending(r => r.MonthYear)
                        .ToList()
                );

                AllRevenueReports = new ObservableCollection<RevenueReport>(RevenueReports);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while load reports: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SearchComboBox_SelectionChanged()
        {
            string selected = monthlyRevenueReportPage.searchComboBox.SelectedItem as string;
            string keyword = monthlyRevenueReportPage.searchTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(keyword))
            {
                RevenueReports = new ObservableCollection<RevenueReport>(AllRevenueReports);
                return;
            }

            if (selected == "Month")
            {
                ReportsSearchByMonth(keyword);
            }
            else if (selected == "Year")
            {
                ReportsSearchByYear(keyword);
            }
            else
            {
                MessageBox.Show("Please select search criteria: Month or Year.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void ReportsSearchByMonth(string monthStr)
        {
            if (int.TryParse(monthStr, out int month) && month >= 1 && month <= 12)
            {
                RevenueReports = new ObservableCollection<RevenueReport>(
                    AllRevenueReports.Where(r => r.MonthYear.HasValue && r.MonthYear.Value.Month == month));
            }
            else
            {
                MessageBox.Show("Please enter a valid month number (1 - 12).", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void ReportsSearchByYear(string yearStr)
        {
            if (int.TryParse(yearStr, out int year))
            {
                RevenueReports = new ObservableCollection<RevenueReport>(
                    AllRevenueReports.Where(r => r.MonthYear.HasValue && r.MonthYear.Value.Year == year));
            }
            else
            {
                MessageBox.Show("Please enter a valid year (e.g. 2025).", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void LoadReportDetailsWindow()
        {
            if (monthlyRevenueReportPage.RevenueReportDataGrid.SelectedItem is RevenueReport selectedReport)
            {
                if (selectedReport.MonthYear == null)
                {
                    MessageBox.Show("The time of the report is invalid.", "Data error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var reportId = selectedReport.RevenueReportId;
                var month = selectedReport.MonthYear.Value.Month;
                var year = selectedReport.MonthYear.Value.Year;
                string monthYear = $"{month:D2}/{year}";

                try
                {
                    var productDetails = context.RevenueReportProductDetails
                        .Where(d => d.RevenueReportId == reportId)
                        .Include(d => d.Product)
                        .ToList();

                    var serviceDetails = context.RevenueReportServiceDetails
                        .Where(d => d.RevenueReportId == reportId)
                        .Include(d => d.Service)
                        .ToList();

                    if (productDetails.Count == 0 && serviceDetails.Count == 0)
                    {
                        MessageBox.Show($"No detailed data for month {monthYear}.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var detailWindow = new MonthlyRevenueReportWindow(productDetails, serviceDetails, monthYear);
                    detailWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading report details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a report to view details!", "Notification", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void CreateOrUpdateCurrentMonthReport()
        {
            var now = DateTime.Now;
            var reportDate = new DateTime(now.Year, now.Month, 1);

            var existingReport = context.RevenueReports
                .FirstOrDefault(r => r.MonthYear.HasValue &&
                                     r.MonthYear.Value.Month == reportDate.Month &&
                                     r.MonthYear.Value.Year == reportDate.Year &&
                                     !r.IsDeleted);

            var confirmUpdate = existingReport != null &&
                MessageBox.Show($"There is a report for {reportDate:MM/yyyy}, update?",
                                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (existingReport != null && !confirmUpdate)
                return;

            try
            {
                using var transaction = context.Database.BeginTransaction();

                var report = existingReport ?? new RevenueReport
                {
                    RevenueReportId = GenerateRevenueReportId(),
                    MonthYear = reportDate,
                    IsDeleted = false
                };

                if (existingReport == null)
                    context.RevenueReports.Add(report);
                else
                {
                    // Xoá detail cũ để làm mới
                    context.RevenueReportProductDetails.RemoveRange(
                        context.RevenueReportProductDetails.Where(x => x.RevenueReportId == report.RevenueReportId));
                    context.RevenueReportServiceDetails.RemoveRange(
                        context.RevenueReportServiceDetails.Where(x => x.RevenueReportId == report.RevenueReportId));
                }

                // Tính doanh thu sản phẩm bán ra trong tháng
                var productDetailsList = new List<RevenueReportProductDetail>();
                decimal totalProductRevenue = 0;

                var products = context.Products.ToList();

                foreach (var product in products)
                {
                    int soldQuantity = context.InvoiceDetails
                        .Where(i => i.ProductId == product.ProductId &&
                                    i.Invoice.Date.HasValue &&
                                    i.Invoice.Date.Value.Month == reportDate.Month &&
                                    i.Invoice.Date.Value.Year == reportDate.Year)
                        .Sum(i => i.Quantity) ?? 0;

                    decimal revenue = (product.Price ?? 0) * soldQuantity;

                    productDetailsList.Add(new RevenueReportProductDetail
                    {
                        RevenueReportId = report.RevenueReportId,
                        ProductId = product.ProductId,
                        Quantity = soldQuantity,
                        Revenue = revenue,
                        Percentage = 0 // tạm thời để 0, sẽ tính tổng chung sau
                    });

                    totalProductRevenue += revenue;
                }

                // Tính doanh thu dịch vụ sử dụng trong tháng
                var serviceDetailsList = new List<RevenueReportServiceDetail>();
                decimal totalServiceRevenue = 0;

                var services = context.Services.ToList();

                foreach (var service in services)
                {
                    int usedQuantity = context.ServiceDetails
                        .Where(d => d.ServiceId == service.ServiceId &&
                                    d.ServiceRecord.CreateDate.HasValue &&
                                    d.ServiceRecord.CreateDate.Value.Month == reportDate.Month &&
                                    d.ServiceRecord.CreateDate.Value.Year == reportDate.Year)
                        .Sum(d => d.Quantity) ?? 0;

                    decimal revenue = (service.ServicePrice ?? 0) * usedQuantity;

                    serviceDetailsList.Add(new RevenueReportServiceDetail
                    {
                        RevenueReportId = report.RevenueReportId,
                        ServiceId = service.ServiceId,
                        Quantity = usedQuantity,
                        Revenue = revenue,
                        Percentage = 0 // cũng để 0 tạm
                    });

                    totalServiceRevenue += revenue;
                }

                // Tính tổng tiền nhập hàng trong tháng
                decimal totalImportCost = context.ImportDetails
                    .Where(d => d.Import.Date.HasValue &&
                                d.Import.Date.Value.Month == reportDate.Month &&
                                d.Import.Date.Value.Year == reportDate.Year)
                    .Sum(d => (d.Quantity ?? 0) * (d.Price ?? 0));

                // Tính tổng doanh thu cuối cùng
                decimal totalRevenue = totalProductRevenue + totalServiceRevenue - totalImportCost;

                // Tính phần trăm trên tổng doanh thu chung (hàng + dịch vụ)
                decimal totalSoldRevenue = totalProductRevenue + totalServiceRevenue;

                if (totalSoldRevenue > 0)
                {
                    foreach (var pd in productDetailsList)
                    {
                        pd.Percentage = Math.Round((pd.Revenue ?? 0) / totalSoldRevenue * 100, 2);
                    }

                    foreach (var sd in serviceDetailsList)
                    {
                        sd.Percentage = Math.Round((sd.Revenue ?? 0) / totalSoldRevenue * 100, 2);
                    }
                }

                report.TotalRevenue = totalRevenue;

                context.RevenueReportProductDetails.AddRange(productDetailsList);
                context.RevenueReportServiceDetails.AddRange(serviceDetailsList);

                context.SaveChanges();
                transaction.Commit();

                MessageBox.Show($"Report for {reportDate:MM/yyyy} {(existingReport != null ? "updated" : "created")} successfully.\n" +
                                $"Total revenue: {totalRevenue:N0}\nImport cost deducted: {totalImportCost:N0}",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadReportsFromDatabase();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "(no inner exception)";
                MessageBox.Show($"Error processing report:\n{ex.Message}\n\n→ Inner Exception:\n{inner}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateRevenueReportId()
        {
            var now = DateTime.Now;
            return "RR" + now.ToString("MMyy");
        }
    }
}
