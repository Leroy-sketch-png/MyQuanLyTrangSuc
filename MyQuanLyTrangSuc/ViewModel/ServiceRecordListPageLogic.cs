using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceRecordListPageLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceRecordService serviceRecordService;
        private readonly ServiceRecordListPage serviceRecordPageUI;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public ObservableCollection<ServiceRecord> ServiceRecords { get; set; }
        public ServiceRecord SelectedServiceRecord { get; set; }

        public ServiceRecordListPageLogic(ServiceRecordListPage page)
        {
            serviceRecordPageUI = page;
            ServiceRecords = new ObservableCollection<ServiceRecord>();
            serviceRecordService = ServiceRecordService.Instance;
            serviceRecordService.OnServiceRecordAdded += HandleServiceRecordAdded;
            serviceRecordService.OnServiceRecordUpdated += HandleServiceRecordUpdated;

            notificationWindowLogic = new NotificationWindowLogic();
            LoadServiceRecordsFromDatabase();
        }

        private void LoadServiceRecordsFromDatabase()
        {
            try
            {
                var recordsFromDb = context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ServiceDetails) // Bao gồm ServiceDetails để tính toán GrandTotal nếu cần
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ServiceRecords.Clear();
                    foreach (var record in recordsFromDb)
                    {
                        ServiceRecords.Add(record);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading service records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleServiceRecordAdded(ServiceRecord newRecord)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceRecords.Add(newRecord);
            });
        }

        private void HandleServiceRecordUpdated(ServiceRecord updatedRecord)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = ServiceRecords.FirstOrDefault(r => r.ServiceRecordId == updatedRecord.ServiceRecordId);
                if (existing != null)
                {
                    // Cập nhật các thuộc tính của đối tượng hiện có
                    existing.Customer = updatedRecord.Customer;
                    existing.Employee = updatedRecord.Employee;
                    existing.CreateDate = updatedRecord.CreateDate;
                    existing.GrandTotal = updatedRecord.GrandTotal;
                    existing.TotalPaid = updatedRecord.TotalPaid;
                    existing.TotalUnpaid = updatedRecord.TotalUnpaid;
                    existing.Status = updatedRecord.Status; // Cập nhật trạng thái
                    // Nếu ServiceRecord triển khai INotifyPropertyChanged, UI sẽ tự động cập nhật
                }
            });
        }

        public void HandleServiceRecordDeleted()
        {
            if (SelectedServiceRecord == null)
            {
                MessageBox.Show("Please select a record to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this service record?",
                                          "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                context.Entry(SelectedServiceRecord).Collection(sr => sr.ServiceDetails).Load();

                foreach (var detail in SelectedServiceRecord.ServiceDetails.ToList())
                {
                    serviceRecordService.DeleteServiceDetail(detail);
                }

                serviceRecordService.DeleteServiceRecord(SelectedServiceRecord);
                context.SaveChanges();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ServiceRecords.Remove(SelectedServiceRecord);
                });

                MessageBox.Show("Service record and related details deleted successfully.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SearchServiceRecords(string keyword, string category)
        {
            var recordsFromDb = context.ServiceRecords
                .Include(sr => sr.Customer)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceRecords.Clear();

                foreach (var record in recordsFromDb)
                {
                    bool match = category switch
                    {
                        "Name" => record.Customer?.Name?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0,
                        "ID" => record.ServiceRecordId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0,
                        _ => false
                    };

                    if (match)
                    {
                        ServiceRecords.Add(record);
                    }
                }
            });
        }

        public void LoadAddServiceRecordWindow()
        {
            AddServiceRecordWindow addWindow = new AddServiceRecordWindow();
            addWindow.ShowDialog();
        }

        public void LoadServiceRecordDetailsWindow()
        {
            if (SelectedServiceRecord != null)
            {
                ServiceRecordDetailWindow detailsWindow = new ServiceRecordDetailWindow(SelectedServiceRecord);

                // Đăng ký lắng nghe sự kiện từ ViewModel của cửa sổ chi tiết
                if (detailsWindow.DataContext is ServiceRecordDetailLogic detailLogic)
                {
                    detailLogic.ServiceRecordCompleted += HandleServiceRecordCompleted;
                }

                detailsWindow.ShowDialog();

                // Hủy đăng ký sự kiện khi cửa sổ đóng để tránh rò rỉ bộ nhớ
                if (detailsWindow.DataContext is ServiceRecordDetailLogic closedDetailLogic)
                {
                    closedDetailLogic.ServiceRecordCompleted -= HandleServiceRecordCompleted;
                }
            }
        }

        // Phương thức xử lý sự kiện khi ServiceRecord được đánh dấu là hoàn tất
        private void HandleServiceRecordCompleted(ServiceRecord completedRecord)
        {
            // Đảm bảo cập nhật trên UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = ServiceRecords.FirstOrDefault(r => r.ServiceRecordId == completedRecord.ServiceRecordId);
                if (existing != null)
                {
                    // Cập nhật các thuộc tính của đối tượng 'existing' trong ObservableCollection
                    // Vì ServiceRecord đã triển khai INotifyPropertyChanged, UI sẽ tự động cập nhật
                    existing.Status = completedRecord.Status;
                    existing.TotalPaid = completedRecord.TotalPaid;
                    existing.TotalUnpaid = completedRecord.TotalUnpaid;
                }
            });
        }

        public void PrintServiceRecord()
        {
            if (SelectedServiceRecord != null)
            {
                var printPage = new ServiceRecordPrint(SelectedServiceRecord);
                printPage.Show();
            }
            else
            {
                MessageBox.Show("Please select a record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void SearchServiceRecordsByNameOfCustomer(string name)
        {
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords
                .Include(i => i.Customer) // Ensure Customer data is included
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceRecords.Clear();
                foreach (ServiceRecord serviceRecord in serviceRecordsFromDb)
                {
                    if (serviceRecord.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }

        public void SearchServiceRecordsByID(string ID)
        {
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords.ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceRecords.Clear();
                foreach (ServiceRecord serviceRecord in serviceRecordsFromDb)
                {
                    if (serviceRecord.ServiceRecordId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }

        public void SearchServiceRecordsByDate(string date)
        {
            var dateParts = date.Split('/');
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ServiceRecords.Clear();

                foreach (var serviceRecord in serviceRecordsFromDb)
                {
                    bool match = true;

                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int day))
                    {
                        if (serviceRecord.CreateDate.Value.Day != day)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int month))
                    {
                        if (serviceRecord.CreateDate.Value.Month != month)
                        {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int year))
                    {
                        if (serviceRecord.CreateDate.Value.Year != year)
                        {
                            match = false;
                        }
                    }

                    if (match)
                    {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }
        public void ExportServiceRecordsToExcel()
        {
            if (ServiceRecords == null || !ServiceRecords.Any())
            {
                MessageBox.Show("No records available to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = "ServiceRecords.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("ServiceRecords");

                        // Headers
                        worksheet.Cells[1, 1].Value = "ID";
                        worksheet.Cells[1, 2].Value = "Customer Name";
                        worksheet.Cells[1, 3].Value = "Employee Name";
                        worksheet.Cells[1, 4].Value = "Create Date";
                        worksheet.Cells[1, 5].Value = "Total";
                        worksheet.Cells[1, 6].Value = "Paid";
                        worksheet.Cells[1, 7].Value = "Unpaid";
                        worksheet.Cells[1, 8].Value = "Status";

                        using (var range = worksheet.Cells[1, 1, 1, 8])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

                        // Data
                        int row = 2;
                        foreach (var record in ServiceRecords)
                        {
                            worksheet.Cells[row, 1].Value = record.ServiceRecordId;
                            worksheet.Cells[row, 2].Value = record.Customer?.Name;
                            worksheet.Cells[row, 3].Value = record.Employee?.Name;
                            worksheet.Cells[row, 4].Value = record.CreateDate?.ToString("dd/MM/yyyy");
                            worksheet.Cells[row, 5].Value = record.GrandTotal;
                            worksheet.Cells[row, 6].Value = record.TotalPaid;
                            worksheet.Cells[row, 7].Value = record.TotalUnpaid;
                            worksheet.Cells[row, 8].Value = record.Status;
                            row++;
                        }

                        // Auto-fit columns
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        // Save to file
                        var fileInfo = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(fileInfo);

                        MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

}