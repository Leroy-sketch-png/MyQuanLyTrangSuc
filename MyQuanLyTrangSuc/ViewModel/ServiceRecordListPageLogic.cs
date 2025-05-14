using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace MyQuanLyTrangSuc.ViewModel {
    public class ServiceRecordListPageLogic {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceRecordListPage serviceRecordListPageUI;
        private readonly ServiceRecordService serviceRecordService;

        // DataContext Zone
        public ObservableCollection<ServiceRecord> ServiceRecords { get; set; }

        public ServiceRecordListPageLogic(ServiceRecordListPage serviceRecordListPageUI) {
            this.serviceRecordListPageUI = serviceRecordListPageUI;
            ServiceRecords = new ObservableCollection<ServiceRecord>();
            LoadRecordsFromDatabase();
            serviceRecordService = ServiceRecordService.Instance;
            serviceRecordService.OnServiceRecordAdded += Context_OnServiceRecordAdded;
        }

        private void Context_OnServiceRecordAdded(ServiceRecord record) {
            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Add(record);
            });
        }

        private void LoadRecordsFromDatabase() {
            try {
                List<ServiceRecord> recordsFromDb = context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() => {
                    ServiceRecords.Clear();
                    foreach (var record in recordsFromDb) {
                        ServiceRecords.Add(record);
                    }
                });
            } catch (Exception ex) {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadAddServiceRecordWindow() {
            //AddServiceRecordWindow addServiceRecordWindowUI = new AddServiceRecordWindow();
            //addServiceRecordWindowUI.ShowDialog();
        }

        public void LoadServiceDetailsWindow() {
            if (serviceRecordListPageUI.ServiceRecordsDataGrid.SelectedItem is ServiceRecord selectedRecord) {
                //ServiceDetailsWindow serviceDetailsWindowUI = new ServiceDetailsWindow(selectedRecord);
                //serviceDetailsWindowUI.ShowDialog();
            }
        }

        public void LoadServiceRecordWindow() {
            if (serviceRecordListPageUI.ServiceRecordsDataGrid.SelectedItem is ServiceRecord selectedRecord) {
                //ServiceReceiptWindow receiptWindowUI = new ServiceReceiptWindow(selectedRecord);
                //receiptWindowUI.ShowDialog();
            }
        }

        public void PrintServiceRecord() {
            if (serviceRecordListPageUI.ServiceRecordsDataGrid.SelectedItem is ServiceRecord selectedRecord) {
                //var printPage = new ServiceReceiptWindow(selectedRecord);
                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true) {
                    //printPage.ShowDialog();
                    //printDialog.PrintVisual(printPage, "Service Record");
                    //printPage.Close();
                }
            } else {
                MessageBox.Show("Please select a service record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SearchServiceRecordsByCustomerName(string name) {
            List<ServiceRecord> recordsFromDb = context.ServiceRecords
                .Include(sr => sr.Customer)
                .ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();
                foreach (ServiceRecord record in recordsFromDb) {
                    if (record.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) {
                        ServiceRecords.Add(record);
                    }
                }
            });
        }

        public void SearchServiceRecordsByID(string id) {
            List<ServiceRecord> recordsFromDb = context.ServiceRecords.ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();
                foreach (ServiceRecord record in recordsFromDb) {
                    if (record.ServiceRecordId.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0) {
                        ServiceRecords.Add(record);
                    }
                }
            });
        }

        public void SearchServiceRecordsByDate(string date) {
            var dateParts = date.Split('/');
            List<ServiceRecord> recordsFromDb = context.ServiceRecords.ToList();

            Application.Current.Dispatcher.Invoke(() => {
                ServiceRecords.Clear();

                foreach (var record in recordsFromDb) {
                    bool match = true;

                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int day)) {
                        if (record.CreateDate?.Day != day) {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int month)) {
                        if (record.CreateDate?.Month != month) {
                            match = false;
                        }
                    }

                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int year)) {
                        if (record.CreateDate?.Year != year) {
                            match = false;
                        }
                    }

                    if (match) {
                        ServiceRecords.Add(record);
                    }
                }
            });
        }
    }
}