using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel {
    public class ServiceRecordListPageLogic {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceRecordService serviceRecordService;
        private readonly ServiceRecordListPage serviceRecordPageUI;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public ObservableCollection<ServiceRecord> ServiceRecords { get; set; }
        public ServiceRecord SelectedServiceRecord { get; set; }

        public ServiceRecordListPageLogic(ServiceRecordListPage page) {
            serviceRecordPageUI = page;
            ServiceRecords = new ObservableCollection<ServiceRecord>();
            serviceRecordService = ServiceRecordService.Instance;
            serviceRecordService.OnServiceRecordAdded += HandleServiceRecordAdded;
            serviceRecordService.OnServiceRecordUpdated += HandleServiceRecordUpdated;

            notificationWindowLogic = new NotificationWindowLogic();
            LoadServiceRecordsFromDatabase();
        }

        private void LoadServiceRecordsFromDatabase() {
            try {
                var recordsFromDb = context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ServiceDetails)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() => {
                    ServiceRecords.Clear();
                    foreach (var record in recordsFromDb) {
                        ServiceRecords.Add(record);
                    }
                });
            } catch (Exception ex) {
                MessageBox.Show($"Error loading service records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleServiceRecordAdded(ServiceRecord newRecord) {
            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Add(newRecord);
            });
        }

        private void HandleServiceRecordUpdated(ServiceRecord updatedRecord) {
            Application.Current.Dispatcher.Invoke(() => {
                var existing = ServiceRecords.FirstOrDefault(r => r.ServiceRecordId == updatedRecord.ServiceRecordId);
                if (existing != null) {
                    var index = ServiceRecords.IndexOf(existing);
                    ServiceRecords[index] = updatedRecord;
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
                // Load related ServiceDetails if not already loaded
                context.Entry(SelectedServiceRecord).Collection(sr => sr.ServiceDetails).Load();

                // Delete child ServiceDetails
                foreach (var detail in SelectedServiceRecord.ServiceDetails.ToList())
                {
                    serviceRecordService.DeleteServiceDetail(detail);
                    //context.ServiceDetails.Remove(detail);
                }

                // Delete the main ServiceRecord
                serviceRecordService.DeleteServiceRecord(SelectedServiceRecord);
                //context.ServiceRecords.Remove(SelectedServiceRecord);
                context.SaveChanges();

                // Remove from ObservableCollection so UI updates
                Application.Current.Dispatcher.Invoke(() => {
                    ServiceRecords.Remove(SelectedServiceRecord);
                });

                //notificationWindowLogic.LoadNotification("Success", "Service record and related details deleted successfully.", "BottomRight");
                MessageBox.Show("Service record and related details deleted successfully.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void SearchServiceRecords(string keyword, string category) {
            var recordsFromDb = context.ServiceRecords
                .Include(sr => sr.Customer)
                .ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();

                foreach (var record in recordsFromDb) {
                    bool match = category switch {
                        "Name" => record.Customer?.Name?.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0,
                        "ID" => record.ServiceRecordId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0,
                        _ => false
                    };

                    if (match) {
                        ServiceRecords.Add(record);
                    }
                }
            });
        }

        public void LoadAddServiceRecordWindow() {
            AddServiceRecordWindow addWindow = new AddServiceRecordWindow();
            addWindow.ShowDialog();
        }

        public void LoadServiceRecordDetailsWindow() {
            if (SelectedServiceRecord != null) {
                //ServiceRecordDetailsWindow detailsWindow = new ServiceRecordDetailsWindow(SelectedServiceRecord);
                //detailsWindow.ShowDialog();
            }
        }

        public void PrintServiceRecord() {
            if (SelectedServiceRecord != null) {
                //var printPage = new ReceiptWindow(SelectedServiceRecord);
                var printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true) {
                    //printPage.ShowDialog(); // Optional: preview
                    //printDialog.PrintVisual(printPage, "Service Record");
                    //printPage.Close();
                }
            } else {
                MessageBox.Show("Please select a record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void SearchServiceRecordsByNameOfCustomer(string name) {
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords
                .Include(i => i.Customer) // Ensure Customer data is included
                .ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();
                foreach (ServiceRecord serviceRecord in serviceRecordsFromDb) {
                    if (serviceRecord.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }

        public void SearchServiceRecordsByID(string ID) {
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords.ToList();
            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();
                foreach (ServiceRecord serviceRecord in serviceRecordsFromDb) {
                    if (serviceRecord.ServiceRecordId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0) {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }

        public void SearchServiceRecordsByDate(string date) {
            var dateParts = date.Split('/');
            List<ServiceRecord> serviceRecordsFromDb = context.ServiceRecords.ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();

                foreach (var serviceRecord in serviceRecordsFromDb) {
                    bool match = true;

                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int day)) {
                        if (serviceRecord.CreateDate.Value.Day != day) {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int month)) {
                        if (serviceRecord.CreateDate.Value.Month != month) {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int year)) {
                        if (serviceRecord.CreateDate.Value.Year != year) {
                            match = false;
                        }
                    }

                    if (match) {
                        ServiceRecords.Add(serviceRecord);
                    }
                }
            });
        }

    }
}
