using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.Security;
using System;
using System.Collections.Generic; // Required for HashSet
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
                ((RelayCommand<Customer>)LoadEditCustomerWindowCommand)?.RaiseCanExecuteChanged(); // Using ?. for null safety
                ((RelayCommand<Customer>)DeleteCustomerCommand)?.RaiseCanExecuteChanged();      // Using ?. for null safety
            }
        }

        // New: HashSet to efficiently manage selected customers for multiple deletion
        private readonly HashSet<Customer> _selectedCustomers = new HashSet<Customer>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Updated: Commands now use the correct RelayCommand version
        public ICommand LoadAddCustomerWindowCommand { get; private set; }
        public ICommand LoadEditCustomerWindowCommand { get; private set; } // Will be RelayCommand<Customer>
        public ICommand DeleteCustomerCommand { get; private set; }         // Will be RelayCommand<Customer>
        public RelayCommand ImportExcelCommand { get; private set; }
        public RelayCommand<DataGrid> ExportExcelCommand { get; private set; } // Generic as it takes DataGrid parameter
        public RelayCommand DeleteMultipleCustomersCommand { get; private set; } // New command for multiple deletion

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
            LoadAddCustomerWindowCommand = new RelayCommand(LoadAddCustomerWindow, CanLoadAddCustomerWindow);
            LoadEditCustomerWindowCommand = new RelayCommand<Customer>(LoadEditCustomerWindow, CanLoadEditCustomerWindow);
            DeleteCustomerCommand = new RelayCommand<Customer>(DeleteCustomer, CanDeleteCustomer);
            // Initialize the new multiple delete command
            DeleteMultipleCustomersCommand = new RelayCommand(DeleteMultipleCustomers, CanDeleteMultipleCustomers);
        }

        /// <summary>
        /// Loads all non-deleted customers from the database into the observable collection.
        /// </summary>
        private void LoadCustomersFromDatabase()
        {
            var customersFromDb = customerService.GetListOfCustomers().Where(c => !c.IsDeleted).ToList();
            Application.Current.Dispatcher.Invoke(() => // Ensure UI update on dispatcher thread
            {
                Customers.Clear();
                foreach (var customer in customersFromDb)
                {
                    Customers.Add(customer);
                }
                // Clear selected items as the underlying collection has changed
                _selectedCustomers.Clear();
                DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged(); // Update the command state
            });
        }

        /// <summary>
        /// Handles the event when a new customer is added via the service.
        /// Ensures the UI is updated on the correct dispatcher thread.
        /// </summary>
        private void CustomerService_OnCustomerAdded(Customer obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Customers.Add(obj);
                // No need to call RaiseCanExecuteChanged for DeleteMultipleCustomersCommand here
                // as adding a single item doesn't directly affect its executability based on count.
            });
        }

        // --- Permission-Controlled Methods and Commands ---

        // Add Customer
        private void LoadAddCustomerWindow()
        {
            Window addWindow = new AddCustomerWindow();
            addWindow.ShowDialog();
            LoadCustomersFromDatabase(); // Refresh after add
        }

        private bool CanLoadAddCustomerWindow()
        {
            return CurrentUserPrincipal?.HasPermission("AddCustomer") == true;
        }

        // Edit Customer
        private void LoadEditCustomerWindow(Customer selectedItem)
        {
            if (selectedItem != null)
            {
                var editWindow = new EditCustomerWindow(selectedItem);
                editWindow.ShowDialog();
                LoadCustomersFromDatabase(); // Refresh after edit
            }
            else
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select a customer to edit.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadEditCustomerWindow(Customer selectedItem)
        {
            // Check permission AND ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("EditCustomer") == true && selectedItem != null;
        }

        // Delete Single Customer
        private void DeleteCustomer(Customer selectedItem)
        {
            if (selectedItem != null)
            {
                // TODO: Replace with custom modal UI for confirmation
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete customer '{selectedItem.Name}'?", "Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    customerService.DeleteCustomer(selectedItem);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Customers.Remove(selectedItem);
                        // Also remove from multiple selection HashSet if it was selected
                        _selectedCustomers.Remove(selectedItem);
                        DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged(); // Update button state for multi-delete
                    });
                }
            }
            else
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select a customer to delete.", "No Customer Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanDeleteCustomer(Customer selectedItem)
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

        // New: Delete Multiple Customers Logic (now command-driven)
        public void DeleteMultipleCustomers()
        {
            if (_selectedCustomers.Count == 0)
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select at least one customer to delete.", "Delete Customers", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Replace with custom modal UI for confirmation
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete these {_selectedCustomers.Count} customers?", "Delete Customers", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Create a temporary list to avoid modification during enumeration
                var customersToDelete = _selectedCustomers.ToList();
                foreach (var customer in customersToDelete)
                {
                    customerService.DeleteCustomer(customer);
                    Application.Current.Dispatcher.Invoke(() => Customers.Remove(customer));
                }
                _selectedCustomers.Clear(); // Clear the tracking HashSet after successful deletion
                DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged(); // Update button state
            }
        }

        /// <summary>
        /// Determines if the DeleteMultipleCustomersCommand can be executed.
        /// </summary>
        /// <returns>True if the user has permission and at least one customer is selected, false otherwise.</returns>
        private bool CanDeleteMultipleCustomers()
        {
            return CurrentUserPrincipal?.HasPermission("DeleteMultipleCustomer") == true && _selectedCustomers.Count > 0;
        }

        // Refactored: Checkbox event handlers to update _selectedCustomers and command state
        // These methods are still directly hooked in XAML but now they correctly update the command's state
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Customer customer)
            {
                _selectedCustomers.Add(customer);
                DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        public void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Customer customer)
            {
                _selectedCustomers.Remove(customer);
                DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        // --- Search Methods (unchanged, as they don't directly use commands for permission) ---
        public void CustomersSearchByName(string name)
        {
            var res = customerService.GetListOfCustomers()
                                     .Where(c => !c.IsDeleted && c.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                                     .ToList();
            UpdateCustomers(res);
        }

        public void CustomersSearchByID(string id)
        {
            var res = customerService.GetListOfCustomers()
                                     .Where(c => !c.IsDeleted && c.CustomerId.ToString().Contains(id))
                                     .ToList();
            UpdateCustomers(res);
        }

        /// <summary>
        /// Updates the Customers ObservableCollection with the new filtered list.
        /// This method is designed to minimize UI updates by only adding/removing items as needed,
        /// similar to how UnitListPageLogic's UpdateUnitsDisplay works.
        /// </summary>
        /// <param name="newCustomers">The list of customers that should currently be displayed.</param>
        private void UpdateCustomers(List<Customer> newCustomers)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newCustomerIds = new HashSet<string>(newCustomers.Select(c => c.CustomerId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = Customers.Count - 1; i >= 0; i--)
                {
                    if (!newCustomerIds.Contains(Customers[i].CustomerId))
                    {
                        Customers.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newCustomer in newCustomers)
                {
                    if (!Customers.Any(c => c.CustomerId == newCustomer.CustomerId))
                    {
                        Customers.Add(newCustomer);
                    }
                }
                // After updating the display, ensure the multi-delete command reflects the current selection state
                DeleteMultipleCustomersCommand?.RaiseCanExecuteChanged();
            });
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
