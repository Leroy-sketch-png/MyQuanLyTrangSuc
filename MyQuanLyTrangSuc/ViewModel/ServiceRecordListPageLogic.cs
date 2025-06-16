using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.View.Windows; // Assuming AddServiceRecordWindow, ServiceRecordDetailWindow, ServiceRecordPrint are here
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceRecordListPageLogic : INotifyPropertyChanged
    {
        private readonly ServiceRecordListPage serviceRecordListPage;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceRecordService serviceRecordService;
        private readonly NotificationWindowLogic notificationWindowLogic; // Still here, consider abstracting into a service/interface

        private ObservableCollection<ServiceRecord> _serviceRecords;
        /// <summary>
        /// Collection of service records displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<ServiceRecord> ServiceRecords
        {
            get => _serviceRecords;
            set
            {
                _serviceRecords = value;
                OnPropertyChanged();
            }
        }

        private ServiceRecord _selectedServiceRecord;
        /// <summary>
        /// The currently selected service record in the DataGrid.
        /// </summary>
        public ServiceRecord SelectedServiceRecord
        {
            get => _selectedServiceRecord;
            set
            {
                _selectedServiceRecord = value;
                OnPropertyChanged();
                // Re-evaluate CanExecute for commands that depend on a selected item
                ((RelayCommand)ViewServiceRecordDetailsCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)DeleteServiceRecordCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand)PrintServiceRecordCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Represents the current user principal for permission checks.
        /// </summary>
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        private string _searchText;
        /// <summary>
        /// Text bound to the search TextBox for filtering service records.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplySearchFilter(); // Apply filter whenever search text changes
            }
        }

        private ComboBoxItem _selectedSearchCriteria;
        /// <summary>
        /// The selected item from the search ComboBox (e.g., "Date", "ID", "Customer").
        /// </summary>
        public ComboBoxItem SelectedSearchCriteria
        {
            get => _selectedSearchCriteria;
            set
            {
                _selectedSearchCriteria = value;
                OnPropertyChanged();
                ApplySearchFilter(); // Apply filter whenever search criteria changes
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Commands ---
        public ICommand AddServiceRecordCommand { get; private set; }
        public ICommand ViewServiceRecordDetailsCommand { get; private set; }
        public ICommand DeleteServiceRecordCommand { get; private set; }
        public ICommand PrintServiceRecordCommand { get; private set; }
        public ICommand ImportServiceRecordsCommand { get; private set; }
        public ICommand ExportServiceRecordsCommand { get; private set; }


        public ServiceRecordListPageLogic()
        {
            ServiceRecords = new ObservableCollection<ServiceRecord>();
            serviceRecordService = ServiceRecordService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();

            // Subscribe to events from the service
            serviceRecordService.OnServiceRecordAdded += HandleServiceRecordAdded;
            serviceRecordService.OnServiceRecordUpdated += HandleServiceRecordUpdated;
            // Assuming ServiceRecordService also has an OnServiceRecordDeleted event
            // If not, you might need to manually handle the ObservableCollection removal after successful DB delete.
            // For now, I'll add a placeholder if it doesn't exist yet.
            serviceRecordService.OnServiceRecordDeleted += HandleServiceRecordDeleted;

            InitializeCommands();
            LoadServiceRecordsFromDatabase();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Customer" };
        }
        public ServiceRecordListPageLogic(ServiceRecordListPage serviceRecordListPage)
        {
            this.serviceRecordListPage = serviceRecordListPage;
            ServiceRecords = new ObservableCollection<ServiceRecord>();
            serviceRecordService = ServiceRecordService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();

            // Subscribe to events from the service
            serviceRecordService.OnServiceRecordAdded += HandleServiceRecordAdded;
            serviceRecordService.OnServiceRecordUpdated += HandleServiceRecordUpdated;
            // Assuming ServiceRecordService also has an OnServiceRecordDeleted event
            // If not, you might need to manually handle the ObservableCollection removal after successful DB delete.
            // For now, I'll add a placeholder if it doesn't exist yet.
            serviceRecordService.OnServiceRecordDeleted += HandleServiceRecordDeleted;

            InitializeCommands();
            LoadServiceRecordsFromDatabase();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Customer" };
        }

        private void InitializeCommands()
        {
            AddServiceRecordCommand = new RelayCommand(ExecuteAddServiceRecord, CanExecuteAddServiceRecord);
            ViewServiceRecordDetailsCommand = new RelayCommand(ExecuteViewServiceRecordDetails, CanViewServiceRecordDetails);
            DeleteServiceRecordCommand = new RelayCommand(ExecuteDeleteServiceRecord, CanDeleteServiceRecord);
            PrintServiceRecordCommand = new RelayCommand(ExecutePrintServiceRecord, CanPrintServiceRecord);
            ImportServiceRecordsCommand = new RelayCommand(ExecuteImportServiceRecords, CanExecuteImport);
            ExportServiceRecordsCommand = new RelayCommand(ExecuteExportServiceRecords, CanExecuteExport);
        }

        private bool CanViewServiceRecordDetails()
        {
            return SelectedServiceRecord != null && CurrentUserPrincipal?.HasPermission("EditServiceRecord") == true;
        }
         
        private bool CanDeleteServiceRecord()
        {
            return SelectedServiceRecord != null && CurrentUserPrincipal?.HasPermission("DeleteServiceRecord") == true;
        }

        private bool CanPrintServiceRecord()
        {
            return SelectedServiceRecord != null && CurrentUserPrincipal?.HasPermission("PrintServiceRecord") == true;
        }

        private bool CanExecuteAddServiceRecord()
        {
            return CurrentUserPrincipal?.HasPermission("AddServiceRecord") == true && AuthenticationService.Instance.GetAccountWithGroupByUsername((string)WpfApplication.Current.Resources["CurrentUsername"]).Employee != null;
        }

        private bool CanExecuteImport()
        {
            return CurrentUserPrincipal?.HasPermission("ImportServiceRecordExcel") == true;
        }
        private bool CanExecuteExport()
        {
            return CurrentUserPrincipal?.HasPermission("ExportServiceRecordExcel") == true;
        }

        private void LoadServiceRecordsFromDatabase()
        {
            try
            {
                var recordsFromDb = context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ServiceDetails)
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

        // --- Event Handlers from Service ---
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
                    existing.Customer = updatedRecord.Customer;
                    existing.Employee = updatedRecord.Employee;
                    existing.CreateDate = updatedRecord.CreateDate;
                    existing.GrandTotal = updatedRecord.GrandTotal;
                    existing.TotalPaid = updatedRecord.TotalPaid;
                    existing.TotalUnpaid = updatedRecord.TotalUnpaid;
                    existing.Status = updatedRecord.Status;
                }
            });
        }

        private void HandleServiceRecordDeleted(ServiceRecord deletedRecord)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = ServiceRecords.FirstOrDefault(r => r.ServiceRecordId == deletedRecord.ServiceRecordId);
                if (existing != null)
                {
                    ServiceRecords.Remove(existing);
                }
            });
        }

        private void HandleServiceRecordCompleted(ServiceRecord completedRecord)
        {
            // This handler is for updates coming from the detail window, particularly status/payment changes
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existing = ServiceRecords.FirstOrDefault(r => r.ServiceRecordId == completedRecord.ServiceRecordId);
                if (existing != null)
                {
                    existing.Status = completedRecord.Status;
                    existing.TotalPaid = completedRecord.TotalPaid;
                    existing.TotalUnpaid = completedRecord.TotalUnpaid;
                }
            });
        }

        // --- Command Execution Methods ---
        private void ExecuteAddServiceRecord()
        {
            AddServiceRecordWindow addWindow = new AddServiceRecordWindow();
            addWindow.ShowDialog();
        }

        private void ExecuteViewServiceRecordDetails()
        {
            if (SelectedServiceRecord != null)
            {
                ServiceRecordDetailWindow detailsWindow = new ServiceRecordDetailWindow(SelectedServiceRecord);

                if (detailsWindow.DataContext is ServiceRecordDetailLogic detailLogic)
                {
                    detailLogic.ServiceRecordCompleted += HandleServiceRecordCompleted;
                }

                detailsWindow.ShowDialog();

                if (detailsWindow.DataContext is ServiceRecordDetailLogic closedDetailLogic)
                {
                    closedDetailLogic.ServiceRecordCompleted -= HandleServiceRecordCompleted;
                }
                // Optionally, refresh data if the detail window allows extensive edits not covered by events
                // LoadServiceRecordsFromDatabase();
            }
        }

        private void ExecuteDeleteServiceRecord()
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
                // Ensure ServiceDetails are loaded before trying to delete them
                context.Entry(SelectedServiceRecord).Collection(sr => sr.ServiceDetails).Load();

                foreach (var detail in SelectedServiceRecord.ServiceDetails.ToList())
                {
                    serviceRecordService.DeleteServiceDetail(detail);
                }

                serviceRecordService.DeleteServiceRecord(SelectedServiceRecord);
                context.SaveChanges(); // Save changes after all deletions

                MessageBox.Show("Service record and related details deleted successfully.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecutePrintServiceRecord()
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

        private void ExecuteImportServiceRecords()
        {
            var importedRecords = ImportServiceRecordsFromExcel();
            // After importing, if they are added to the database, reload the collection.
            if (importedRecords.Any())
            {
                LoadServiceRecordsFromDatabase();
            }
        }

        private void ExecuteExportServiceRecords()
        {
            ExportServiceRecordsToExcel();
        }

        // --- Search/Filter Logic ---
        /// <summary>
        /// Applies the search filter based on the current SearchText and SelectedSearchCriteria.
        /// </summary>
        private void ApplySearchFilter()
        {
            // If search text is empty, reload all service records
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadServiceRecordsFromDatabase();
                return;
            }

            // Perform search based on selected criteria
            List<ServiceRecord> filteredRecords;
            string searchBy = SelectedSearchCriteria?.Content.ToString();

            switch (searchBy)
            {
                case "ID":
                    filteredRecords = SearchServiceRecordsByID(SearchText);
                    break;
                case "Customer":
                    filteredRecords = SearchServiceRecordsByNameOfCustomer(SearchText);
                    break;
                case "Date":
                    filteredRecords = SearchServiceRecordsByDate(SearchText);
                    break;
                default: // Default to Customer search if criteria is null/unrecognized
                    filteredRecords = SearchServiceRecordsByNameOfCustomer(SearchText);
                    break;
            }

            UpdateServiceRecordsDisplay(filteredRecords);
        }

        /// <summary>
        /// Searches service records by customer name.
        /// </summary>
        public List<ServiceRecord> SearchServiceRecordsByNameOfCustomer(string name)
        {
            return context.ServiceRecords
                          .Include(sr => sr.Customer)
                          .Where(sr => sr.Customer != null && sr.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches service records by Service Record ID.
        /// </summary>
        public List<ServiceRecord> SearchServiceRecordsByID(string ID)
        {
            return context.ServiceRecords
                          .Include(sr => sr.Customer) // Keep includes for consistency in displayed data
                          .Include(sr => sr.Employee)
                          .Where(sr => sr.ServiceRecordId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches service records by date. Handles partial date inputs (day, month, year).
        /// </summary>
        public List<ServiceRecord> SearchServiceRecordsByDate(string date)
        {
            List<ServiceRecord> matchingRecords = new List<ServiceRecord>();
            // Attempt to parse the full date first
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                matchingRecords = context.ServiceRecords
                                          .Include(sr => sr.Customer)
                                          .Include(sr => sr.Employee)
                                          .Where(sr => sr.CreateDate.HasValue &&
                                                      sr.CreateDate.Value.Date == parsedDate.Date)
                                          .ToList();
            }
            else
            {
                // Fallback to searching by date parts if full parsing fails
                var dateParts = date.Split(new char[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                var allRecords = context.ServiceRecords.Include(sr => sr.Customer).Include(sr => sr.Employee).ToList();

                foreach (var record in allRecords)
                {
                    if (!record.CreateDate.HasValue) continue;

                    bool match = true;
                    // Check day, month, year based on available parts
                    if (dateParts.Length > 0 && int.TryParse(dateParts[0], out int part1))
                    {
                        if (record.CreateDate.Value.Day != part1 && record.CreateDate.Value.Month != part1 && record.CreateDate.Value.Year != part1)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length > 1 && int.TryParse(dateParts[1], out int part2))
                    {
                        if (record.CreateDate.Value.Month != part2 && record.CreateDate.Value.Year != part2)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length > 2 && int.TryParse(dateParts[2], out int part3))
                    {
                        if (record.CreateDate.Value.Year != part3)
                        {
                            match = false;
                        }
                    }

                    if (match && dateParts.Length > 0)
                    {
                        matchingRecords.Add(record);
                    }
                }
            }
            return matchingRecords;
        }

        /// <summary>
        /// Updates the ServiceRecords ObservableCollection with the new filtered list.
        /// This method is designed to minimize UI updates by only adding/removing items as needed.
        /// </summary>
        /// <param name="newRecords">The list of service records that should currently be displayed.</param>
        private void UpdateServiceRecordsDisplay(List<ServiceRecord> newRecords)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a temporary set of IDs from the new filtered list for efficient lookup
                var newRecordIds = new HashSet<string>(newRecords.Select(sr => sr.ServiceRecordId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = ServiceRecords.Count - 1; i >= 0; i--)
                {
                    if (!newRecordIds.Contains(ServiceRecords[i].ServiceRecordId))
                    {
                        ServiceRecords.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newRecord in newRecords)
                {
                    if (!ServiceRecords.Any(sr => sr.ServiceRecordId == newRecord.ServiceRecordId))
                    {
                        ServiceRecords.Add(newRecord);
                    }
                }
            });
        }

        // --- Excel Operations ---
        public List<ServiceRecord> ImportServiceRecordsFromExcel()
        {
            var importedRecords = new List<ServiceRecord>();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            MessageBox.Show("No worksheet found in the Excel file.", "Import Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return importedRecords;
                        }

                        int row = 2; // Assuming row 1 is the header
                        while (worksheet.Cells[row, 1].Value != null)
                        {
                            // Basic validation and parsing
                            string serviceRecordId = worksheet.Cells[row, 1].Text;
                            string customerName = worksheet.Cells[row, 2].Text;
                            string employeeName = worksheet.Cells[row, 3].Text;

                            DateTime? createDate = DateTime.TryParse(worksheet.Cells[row, 4].Text, out var date) ? date : (DateTime?)null;
                            decimal grandTotal = decimal.TryParse(worksheet.Cells[row, 5].Text, out var total) ? total : 0;
                            decimal totalPaid = decimal.TryParse(worksheet.Cells[row, 6].Text, out var paid) ? paid : 0;
                            decimal totalUnpaid = decimal.TryParse(worksheet.Cells[row, 7].Text, out var unpaid) ? unpaid : 0;
                            string status = worksheet.Cells[row, 8].Text;

                            // Look up existing Customer and Employee or create new ones
                            // This part ensures that imported records link to existing entities if names match
                            var customer = context.Customers.FirstOrDefault(c => c.Name == customerName);
                            if (customer == null)
                            {
                                customer = new Customer { Name = customerName };
                                context.Customers.Add(customer);
                            }

                            var employee = context.Employees.FirstOrDefault(e => e.Name == employeeName);
                            if (employee == null)
                            {
                                employee = new Employee { Name = employeeName };
                                context.Employees.Add(employee);
                            }

                            var record = new ServiceRecord
                            {
                                ServiceRecordId = serviceRecordId,
                                Customer = customer,
                                Employee = employee,
                                CreateDate = createDate,
                                GrandTotal = grandTotal,
                                TotalPaid = totalPaid,
                                TotalUnpaid = totalUnpaid,
                                Status = status
                            };

                            importedRecords.Add(record);
                            row++;
                        }
                        // Add imported records to the database
                        foreach (var record in importedRecords)
                        {
                            // Use the service to add, which should also raise the event
                            serviceRecordService.AddServiceRecord(record);
                        }
                        context.SaveChanges(); // Save all changes including new customers/employees if any

                        MessageBox.Show("Import successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import: {ex.Message}\nDetails: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return importedRecords;
        }

        public void ExportServiceRecordsToExcel()
        {
            //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial; // Set license context

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
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("ServiceRecords");

                        // Headers
                        worksheet.Cells[1, 1].Value = "ID";
                        worksheet.Cells[1, 2].Value = "Customer Name";
                        worksheet.Cells[1, 3].Value = "Employee Name";
                        worksheet.Cells[1, 4].Value = "Create Date";
                        worksheet.Cells[1, 5].Value = "Grand Total";
                        worksheet.Cells[1, 6].Value = "Total Paid";
                        worksheet.Cells[1, 7].Value = "Total Unpaid";
                        worksheet.Cells[1, 8].Value = "Status";

                        using (var range = worksheet.Cells[1, 1, 1, 8])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                        }

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

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

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

        // Add these methods to your ServiceRecordListPageLogic class

        /// <summary>
        /// Applies advanced filters to the ServiceRecords collection based on the provided filter criteria.
        /// </summary>
        public void ApplyAdvancedFilters(
            bool enableIdFilter, string idFrom, string idTo,
            bool enableDateFilter, DateTime? dateFrom, DateTime? dateTo,
            bool enableCustomerFilter, string customerText,
            bool enableTotalFilter, decimal? totalFrom, decimal? totalTo,
            bool enablePaidFilter, decimal? paidFrom, decimal? paidTo,
            bool enableRemainingFilter, decimal? remainingFrom, decimal? remainingTo,
            bool enableStatusFilter, string selectedStatus)
        {
            try
            {
                // Start with all service records from database
                var query = context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .AsQueryable();

                // Apply ID filter
                if (enableIdFilter)
                {
                    if (!string.IsNullOrWhiteSpace(idFrom))
                    {
                        query = query.Where(sr => string.Compare(sr.ServiceRecordId, "SRV" + idFrom, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    if (!string.IsNullOrWhiteSpace(idTo))
                    {
                        query = query.Where(sr => string.Compare(sr.ServiceRecordId, "SRV" + idTo, StringComparison.OrdinalIgnoreCase) <= 0);
                    }
                }

                // Apply Date filter
                if (enableDateFilter)
                {
                    if (dateFrom.HasValue)
                    {
                        var fromDate = dateFrom.Value.Date;
                        query = query.Where(sr => sr.CreateDate.HasValue && sr.CreateDate.Value.Date >= fromDate);
                    }
                    if (dateTo.HasValue)
                    {
                        var toDate = dateTo.Value.Date;
                        query = query.Where(sr => sr.CreateDate.HasValue && sr.CreateDate.Value.Date <= toDate);
                    }
                }

                // Apply Customer filter
                if (enableCustomerFilter && !string.IsNullOrWhiteSpace(customerText))
                {
                    var searchText = customerText.Trim().ToLower();
                    query = query.Where(sr => sr.Customer != null &&
                        sr.Customer.Name.ToLower().Contains(searchText));
                }

                // Apply Total Amount filter
                if (enableTotalFilter)
                {
                    if (totalFrom.HasValue)
                    {
                        query = query.Where(sr => sr.GrandTotal >= totalFrom.Value);
                    }
                    if (totalTo.HasValue)
                    {
                        query = query.Where(sr => sr.GrandTotal <= totalTo.Value);
                    }
                }

                // Apply Paid Amount filter
                if (enablePaidFilter)
                {
                    if (paidFrom.HasValue)
                    {
                        query = query.Where(sr => sr.TotalPaid >= paidFrom.Value);
                    }
                    if (paidTo.HasValue)
                    {
                        query = query.Where(sr => sr.TotalPaid <= paidTo.Value);
                    }
                }

                // Apply Remaining Amount filter
                if (enableRemainingFilter)
                {
                    if (remainingFrom.HasValue)
                    {
                        query = query.Where(sr => sr.TotalUnpaid >= remainingFrom.Value);
                    }
                    if (remainingTo.HasValue)
                    {
                        query = query.Where(sr => sr.TotalUnpaid <= remainingTo.Value);
                    }
                }

                // Apply Status filter
                if (enableStatusFilter && !string.IsNullOrWhiteSpace(selectedStatus))
                {
                    query = query.Where(sr => sr.Status == selectedStatus);
                }

                // Execute query and update the collection
                var filteredRecords = query.OrderByDescending(sr => sr.CreateDate).ToList();
                UpdateServiceRecordsDisplay(filteredRecords);

                // Show result count in notification
                WpfApplication.Current.Dispatcher.Invoke(() =>
                {
                    notificationWindowLogic.LoadNotification(
                        "Success",
                        $"Filter applied: {filteredRecords.Count} records found",
                        "BottomRight");
                });
            }
            catch (Exception ex)
            {
                WpfApplication.Current.Dispatcher.Invoke(() =>
                {
                    notificationWindowLogic.LoadNotification(
                        "Error",
                        $"Error applying filters: {ex.Message}",
                        "BottomRight");
                });
            }
        }

        /// <summary>
        /// Clears all applied filters and reloads all service records from the database.
        /// </summary>
        public void ClearAllFilters()
        {
            try
            {
                // Reload all records from database
                LoadServiceRecordsFromDatabase();

                WpfApplication.Current.Dispatcher.Invoke(() =>
                {
                    notificationWindowLogic.LoadNotification(
                        "Success",
                        "All filters cleared",
                        "BottomRight");
                });
            }
            catch (Exception ex)
            {
                WpfApplication.Current.Dispatcher.Invoke(() =>
                {
                    notificationWindowLogic.LoadNotification(
                        "Error",
                        $"Error clearing filters: {ex.Message}",
                        "BottomRight");
                });
            }
        }        // Dispose of event subscriptions to prevent memory leaks
        public void Dispose()
        {
            serviceRecordService.OnServiceRecordAdded -= HandleServiceRecordAdded;
            serviceRecordService.OnServiceRecordUpdated -= HandleServiceRecordUpdated;
            serviceRecordService.OnServiceRecordDeleted -= HandleServiceRecordDeleted;
        }
    }
}