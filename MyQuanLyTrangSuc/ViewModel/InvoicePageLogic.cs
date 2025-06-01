using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.View.Windows; // Assuming AddInvoiceWindow, InvoiceDetailsWindow, ReceiptWindow are here
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Required for ICommand

namespace MyQuanLyTrangSuc.ViewModel
{
    public class InvoicePageLogic : INotifyPropertyChanged
    {
        private readonly InvoicePage invoicePage;
        private readonly MyQuanLyTrangSucContext context; // Use the context directly for loading with Includes if needed
        private readonly InvoiceService invoiceService;

        private ObservableCollection<Invoice> _invoices;
        /// <summary>
        /// Collection of invoices displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set
            {
                _invoices = value;
                OnPropertyChanged();
            }
        }

        private Invoice _selectedInvoice;
        /// <summary>
        /// The currently selected invoice in the DataGrid.
        /// </summary>
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged();
                // Re-evaluate CanExecute for commands that depend on a selected item
                ((RelayCommand<Invoice>)LoadInvoiceDetailsWindowCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand<Invoice>)PrintInvoiceRecordCommand)?.RaiseCanExecuteChanged();
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
        /// Text bound to the search TextBox for filtering invoices.
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
        public ICommand LoadAddInvoiceWindowCommand { get; private set; }
        public ICommand LoadInvoiceDetailsWindowCommand { get; private set; } // Takes Invoice parameter
        public ICommand PrintInvoiceRecordCommand { get; private set; }       // Takes Invoice parameter

        public InvoicePageLogic()
        {
            context = MyQuanLyTrangSucContext.Instance;
            invoiceService = InvoiceService.Instance;
            Invoices = new ObservableCollection<Invoice>();
            LoadRecordsFromDatabase();
            invoiceService.OnInvoiceAdded += Context_OnInvoiceAdded;
            InitializeCommands();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Customer" };
        }
        public InvoicePageLogic(InvoicePage invoicePage)
        {
            this.invoicePage = invoicePage;
            context = MyQuanLyTrangSucContext.Instance;
            invoiceService = InvoiceService.Instance;
            Invoices = new ObservableCollection<Invoice>();
            LoadRecordsFromDatabase();
            invoiceService.OnInvoiceAdded += Context_OnInvoiceAdded;
            InitializeCommands();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Customer" };
        }

        private void InitializeCommands()
        {
            LoadAddInvoiceWindowCommand = new RelayCommand(LoadAddInvoiceWindow, CanLoadAddInvoiceWindow);
            LoadInvoiceDetailsWindowCommand = new RelayCommand<Invoice>(LoadInvoiceDetailsWindow, CanLoadInvoiceDetailsWindow);
            PrintInvoiceRecordCommand = new RelayCommand<Invoice>(PrintInvoiceRecord, CanPrintInvoiceRecord);
        }

        private void LoadRecordsFromDatabase()
        {
            try
            {
                // Ensure Customer data is eager-loaded with the invoice
                List<Invoice> invoicesFromDb = context.Invoices
                    .Include(i => i.Customer)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Invoices.Clear();
                    foreach (Invoice inv in invoicesFromDb)
                    {
                        Invoices.Add(inv);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Context_OnInvoiceAdded(Invoice invoice)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Invoices.Add(invoice);
            });
        }

        // --- Add Invoice Logic ---
        private void LoadAddInvoiceWindow()
        {
            AddInvoiceWindow addInvoiceWindowUI = new AddInvoiceWindow();
            addInvoiceWindowUI.ShowDialog();
            LoadRecordsFromDatabase(); // Refresh data after the window closes
        }

        private bool CanLoadAddInvoiceWindow()
        {
            return CurrentUserPrincipal?.HasPermission("AddInvoice") == true;
        }

        // --- View Invoice Details Logic ---
        private void LoadInvoiceDetailsWindow(Invoice selectedInvoice)
        {
            if (selectedInvoice != null)
            {
                InvoiceDetailsWindow invoiceDetailsWindowUI = new InvoiceDetailsWindow(selectedInvoice);
                invoiceDetailsWindowUI.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an invoice record to view details.", "No Invoice Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadInvoiceDetailsWindow(Invoice selectedInvoice)
        {
            return CurrentUserPrincipal?.HasPermission("ViewInvoiceDetails") == true && selectedInvoice != null;
        }

        // --- Print Invoice Logic ---
        public void PrintInvoiceRecord(Invoice selectedInvoice)
        {
            if (selectedInvoice != null)
            {
                var printPage = new ReceiptWindow(selectedInvoice); // Assuming ReceiptWindow is your printable view
                var printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    printPage.Show(); // Must be shown to be printable by PrintVisual
                    printPage.UpdateLayout(); // Ensure layout is updated before printing
                    printDialog.PrintVisual(printPage, $"Invoice Record - {selectedInvoice.InvoiceId}");
                    printPage.Close(); // Close the window after printing
                }
            }
            else
            {
                MessageBox.Show("Please select an invoice record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanPrintInvoiceRecord(Invoice selectedInvoice)
        {
            return CurrentUserPrincipal?.HasPermission("PrintInvoice") == true && selectedInvoice != null;
        }

        // --- Search/Filter Logic ---
        /// <summary>
        /// Applies the search filter based on the current SearchText and SelectedSearchCriteria.
        /// </summary>
        private void ApplySearchFilter()
        {
            // If search text is empty, reload all invoices
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadRecordsFromDatabase();
                return;
            }

            // Perform search based on selected criteria
            List<Invoice> filteredInvoices;
            string searchBy = SelectedSearchCriteria?.Content.ToString();

            switch (searchBy)
            {
                case "ID":
                    filteredInvoices = SearchInvoicesByID(SearchText);
                    break;
                case "Customer":
                    filteredInvoices = SearchInvoicesByNameOfCustomer(SearchText);
                    break;
                case "Date":
                    filteredInvoices = SearchInvoicesByDate(SearchText);
                    break;
                default: // Default to Customer search if criteria is null/unrecognized
                    filteredInvoices = SearchInvoicesByNameOfCustomer(SearchText);
                    break;
            }

            UpdateInvoicesDisplay(filteredInvoices);
        }

        /// <summary>
        /// Searches invoices by customer name.
        /// </summary>
        public List<Invoice> SearchInvoicesByNameOfCustomer(string name)
        {
            // Include Customer navigation property for filtering by customer name
            return context.Invoices
                          .Include(i => i.Customer)
                          .Where(i => i.Customer != null && i.Customer.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches invoices by Invoice ID.
        /// </summary>
        public List<Invoice> SearchInvoicesByID(string id)
        {
            return context.Invoices
                          .Include(i => i.Customer) // Still include customer for consistent data display
                          .Where(i => i.InvoiceId.IndexOf(id, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches invoices by date. Handles partial date inputs (day, month, year).
        /// </summary>
        public List<Invoice> SearchInvoicesByDate(string date)
        {
            List<Invoice> matchingInvoices = new List<Invoice>();
            // Parse date parts for flexible searching (e.g., "12" for day, "05" for month, "2023" for year)
            // This is a basic implementation; robust date parsing might need more sophisticated logic
            // or a dedicated date picker in the UI.
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                matchingInvoices = context.Invoices
                                          .Include(i => i.Customer)
                                          .Where(i => i.Date.HasValue &&
                                                      i.Date.Value.Date == parsedDate.Date)
                                          .ToList();
            }
            else
            {
                // Attempt to match by parts if full parsing fails
                var dateParts = date.Split(new char[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                var allInvoices = context.Invoices.Include(i => i.Customer).ToList();

                foreach (var invoice in allInvoices)
                {
                    if (!invoice.Date.HasValue) continue;

                    bool match = true;
                    if (dateParts.Length >= 1 && int.TryParse(dateParts[0], out int part1))
                    {
                        // Assume first part can be day or month
                        if (invoice.Date.Value.Day != part1 && invoice.Date.Value.Month != part1)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length >= 2 && int.TryParse(dateParts[1], out int part2))
                    {
                        // Assume second part can be month or year
                        if (invoice.Date.Value.Month != part2 && invoice.Date.Value.Year != part2)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length >= 3 && int.TryParse(dateParts[2], out int part3))
                    {
                        // Assume third part is year
                        if (invoice.Date.Value.Year != part3)
                        {
                            match = false;
                        }
                    }

                    if (match && dateParts.Length > 0) // Only add if at least one part matched
                    {
                        matchingInvoices.Add(invoice);
                    }
                }
            }
            return matchingInvoices;
        }


        /// <summary>
        /// Updates the Invoices ObservableCollection with the new filtered list.
        /// This method is designed to minimize UI updates by only adding/removing items as needed.
        /// </summary>
        /// <param name="newInvoices">The list of invoices that should currently be displayed.</param>
        private void UpdateInvoicesDisplay(List<Invoice> newInvoices)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a temporary set of IDs from the new filtered list for efficient lookup
                var newInvoiceIds = new HashSet<string>(newInvoices.Select(i => i.InvoiceId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = Invoices.Count - 1; i >= 0; i--)
                {
                    if (!newInvoiceIds.Contains(Invoices[i].InvoiceId))
                    {
                        Invoices.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newInvoice in newInvoices)
                {
                    if (!Invoices.Any(inv => inv.InvoiceId == newInvoice.InvoiceId))
                    {
                        Invoices.Add(newInvoice);
                    }
                }
            });
        }
    }
}