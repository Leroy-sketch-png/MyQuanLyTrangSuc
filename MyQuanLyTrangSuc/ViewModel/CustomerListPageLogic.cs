// MyQuanLyTrangSuc.ViewModel/CustomerListPageLogic.cs

using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class CustomerListPageLogic : INotifyPropertyChanged
    {
        private readonly CustomerService customerService;

        private ObservableCollection<Customer> customers;
        public ObservableCollection<Customer> Customers
        {
            get => customers;
            set
            {
                customers = value;
                OnPropertyChanged();
            }
        }

        // Added: CurrentUserPrincipal property for permission binding in XAML
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // Added: SelectedCustomer property to bind to DataGrid.SelectedItem
        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                // When selection changes, inform commands that their CanExecute might need re-evaluation
                ((RelayCommand<Customer>)LoadEditCustomerWindowCommand).RaiseCanExecuteChanged();
                ((RelayCommand<Customer>)DeleteCustomerCommand).RaiseCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // Updated: Commands now use the correct RelayCommand version
        public ICommand LoadAddCustomerWindowCommand { get; private set; }
        public ICommand LoadEditCustomerWindowCommand { get; private set; } // Will be RelayCommand<Customer>
        public ICommand DeleteCustomerCommand { get; private set; }       // Will be RelayCommand<Customer>
        public RelayCommand ImportExcelCommand { get; private set; }
        public RelayCommand<DataGrid> ExportExcelCommand { get; private set; } // Generic as it takes DataGrid parameter

        public CustomerListPageLogic()
        {
            this.customerService = CustomerService.Instance;
            Customers = new ObservableCollection<Customer>();
            LoadCustomersFromDatabase();
            customerService.OnCustomerAdded += CustomerService_OnCustomerAdded;
            InitializeCommands();
        }
        // --- Initialize Commands ---
        private void InitializeCommands()
        {

            ImportExcelCommand = new RelayCommand(ImportExcelFile, CanImportExcel);
            ExportExcelCommand = new RelayCommand<DataGrid>(ExportExcelFile, CanExportExcel);

            // Initialize commands with permission checks, using the correct RelayCommand version
            LoadAddCustomerWindowCommand = new RelayCommand(LoadAddCustomerWindow, CanLoadAddCustomerWindow); // No parameter needed for CanExecute
            LoadEditCustomerWindowCommand = new RelayCommand<Customer>(LoadEditCustomerWindow, CanLoadEditCustomerWindow); // Customer parameter needed
            DeleteCustomerCommand = new RelayCommand<Customer>(DeleteCustomer, CanDeleteCustomer); // Customer parameter needed
        }
        private void LoadCustomersFromDatabase()
        {
            var customers = customerService.GetListOfCustomers().Where(c => !c.IsDeleted).ToList();
            Customers = new ObservableCollection<Customer>(customers);
        }

        private void CustomerService_OnCustomerAdded(Customer obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Customers.Add(obj);
            });
        }

        // --- Permission-Controlled Methods and Commands ---

        // Add Customer
        private void LoadAddCustomerWindow() // No parameter needed for execute
        {
            Window addWindow = new AddCustomerWindow();
            addWindow.ShowDialog();
            LoadCustomersFromDatabase();
        }

        private bool CanLoadAddCustomerWindow() // No parameter needed for canExecute
        {
            return CurrentUserPrincipal?.HasPermission("AddCustomer") == true;
        }

        // Edit Customer
        private void LoadEditCustomerWindow(Customer selectedItem) // Expects Customer parameter
        {
            if (selectedItem != null)
            {
                var editWindow = new EditCustomerWindow(selectedItem);
                editWindow.ShowDialog();
                LoadCustomersFromDatabase();
            }
            else
            {
                MessageBox.Show("Please select a customer to edit.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        // Delete Customer
        private void DeleteCustomer(Customer selectedItem) // Expects Customer parameter
        {
            if (selectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete customer '{selectedItem.Name}'?", "Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    customerService.DeleteCustomer(selectedItem);
                    Customers.Remove(selectedItem);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer to delete.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private bool CanLoadEditCustomerWindow(Customer selectedItem) // Expects Customer parameter
        {
            // Check permission AND ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("EditCustomer") == true && selectedItem != null;
        }

        private bool CanDeleteCustomer(Customer selectedItem) // Expects Customer parameter
        {
            // Check permission AND ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("DeleteCustomer") == true && selectedItem != null;
        }
        private bool CanImportExcel()
        {
            // Permission for importing data.
            return CurrentUserPrincipal?.HasPermission("ImportCustomerExcel") == true;
        }

        private bool CanExportExcel(DataGrid parameter)
        {
            // Permission for exporting data.
            return CurrentUserPrincipal?.HasPermission("ExportCustomerExcel") == true;
        }
        // --- Original Search and Import/Export Methods ---
        // (No changes to these for permission, as they don't use commands or direct UI interaction)

        public void CustomersSearchByName(string name)
        {
            var res = customerService.GetListOfCustomers() // Changed from SuppliersSearchByName
                                     .Where(c => !c.IsDeleted && c.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                                     .ToList();
            UpdateCustomers(res);
        }

        public void CustomersSearchByID(string id)
        {
            // Assuming customerService has a method to search by ID directly or you need to filter from a list
            var res = customerService.GetListOfCustomers() // Changed from SuppliersSearchByID
                                     .Where(c => !c.IsDeleted && c.CustomerId.ToString().Contains(id)) // Example: assuming CustomerID is int/string
                                     .ToList();
            UpdateCustomers(res);
        }

        private void UpdateCustomers(List<Customer> newCustomers)
        {
            // Only update if there are actual changes to prevent unnecessary UI updates
            if (!Customers.SequenceEqual(newCustomers))
            {
                Customers.Clear();
                foreach (var customer in newCustomers)
                {
                    Customers.Add(customer);
                }
                OnPropertyChanged(nameof(Customers));
            }
        }

        // Import excel file
        public void ImportExcelFile()
        {
            customerService.ImportExcelFile();
            LoadCustomersFromDatabase(); // Refresh after import
        }

        // Export excel file
        public void ExportExcelFile(DataGrid customersDataGrid)
        {
            customerService.ExportExcelFile(customersDataGrid);
        }
    }
}