using MahApps.Metro.Controls; // Keeping this from the merged version
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic; // Added for HashSet
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
    public class SupplierListPageLogic : INotifyPropertyChanged
    {
        private readonly SupplierService supplierService;

        private ObservableCollection<Supplier> suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get => suppliers;
            set
            {
                suppliers = value;
                OnPropertyChanged();
            }
        }

        // Added: CurrentUserPrincipal property for permission binding in XAML
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // Added: SelectedSupplier property to bind to DataGrid.SelectedItem
        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                // When selection changes, inform commands that their CanExecute might need re-evaluation
                ((RelayCommand<Supplier>)LoadEditSupplierWindowCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand<Supplier>)DeleteSupplierCommand)?.RaiseCanExecuteChanged();
            }
        }

        // New: HashSet to track selected suppliers for multiple deletion, following UnitListPageLogic design.
        private readonly HashSet<Supplier> _selectedSuppliers = new HashSet<Supplier>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Updated: Commands now use the correct RelayCommand version
        public ICommand LoadAddSupplierWindowCommand { get; private set; }
        public ICommand LoadEditSupplierWindowCommand { get; private set; } // Will be RelayCommand<Supplier>
        public ICommand DeleteSupplierCommand { get; private set; }          // Will be RelayCommand<Supplier>
        public RelayCommand ImportExcelCommand { get; private set; }
        public RelayCommand<DataGrid> ExportExcelCommand { get; private set; } // Generic as it takes DataGrid parameter
        public RelayCommand DeleteMultipleSuppliersCommand { get; private set; } // New command for multiple deletion

        public SupplierListPageLogic()
        {
            this.supplierService = SupplierService.Instance;
            Suppliers = new ObservableCollection<Supplier>();
            LoadSuppliersFromDatabase();
            supplierService.OnSupplierAdded += SupplierService_OnSupplierAdded;
            InitializeCommands();
        }

        // --- Initialize Commands ---
        private void InitializeCommands()
        {
            LoadAddSupplierWindowCommand = new RelayCommand(LoadAddSupplierWindow, CanLoadAddSupplierWindow);
            LoadEditSupplierWindowCommand = new RelayCommand<Supplier>(LoadEditSupplierWindow, CanLoadEditSupplierWindow);
            DeleteSupplierCommand = new RelayCommand<Supplier>(DeleteSupplier, CanDeleteSupplier);
            ImportExcelCommand = new RelayCommand(ImportExcelFile, CanImportExcel);
            ExportExcelCommand = new RelayCommand<DataGrid>(ExportExcelFile, CanExportExcel);
            // Initialize the new multiple delete command
            DeleteMultipleSuppliersCommand = new RelayCommand(DeleteMultipleSuppliers, CanDeleteMultipleSuppliers);
        }

        /// <summary>
        /// Loads all non-deleted suppliers from the database into the observable collection.
        /// </summary>
        private void LoadSuppliersFromDatabase()
        {
            var suppliersFromDb = supplierService.GetListOfSuppliers().Where(s => !s.IsDeleted).ToList();
            Application.Current.Dispatcher.Invoke(() => // Ensure UI update on dispatcher thread
            {
                Suppliers.Clear();
                foreach (var supplier in suppliersFromDb)
                {
                    Suppliers.Add(supplier);
                }
                // Clear selected items as the underlying collection has changed
                _selectedSuppliers.Clear();
                DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged(); // Update the command state
            });
        }

        /// <summary>
        /// Handles the event when a new supplier is added via the service.
        /// Ensures the UI is updated on the correct dispatcher thread.
        /// </summary>
        private void SupplierService_OnSupplierAdded(Supplier obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Suppliers.Add(obj);
                // No need to call RaiseCanExecuteChanged for DeleteMultipleSuppliersCommand here
                // as adding a single item doesn't directly affect its executability based on count.
            });
        }

        // --- Permission-Controlled Methods and Commands ---

        // Add Supplier
        private void LoadAddSupplierWindow()
        {
            Window addWindow = new AddSupplierWindow();
            addWindow.ShowDialog();
            LoadSuppliersFromDatabase(); // Refresh after add
        }

        private bool CanLoadAddSupplierWindow()
        {
            return CurrentUserPrincipal?.HasPermission("AddSupplier") == true;
        }

        // Edit Supplier
        private void LoadEditSupplierWindow(Supplier selectedItem)
        {
            if (selectedItem != null)
            {
                var editWindow = new EditSupplierWindow(selectedItem);
                editWindow.ShowDialog();
                LoadSuppliersFromDatabase(); // Refresh after edit
            }
            else
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select a supplier to edit.", "No Supplier Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadEditSupplierWindow(Supplier selectedItem)
        {
            return CurrentUserPrincipal?.HasPermission("EditSupplier") == true && selectedItem != null;
        }

        // Delete Single Supplier
        private void DeleteSupplier(Supplier selectedItem)
        {
            if (selectedItem != null)
            {
                // TODO: Replace with custom modal UI for confirmation
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete supplier '{selectedItem.Name}'?", "Delete Supplier", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    supplierService.DeleteSupplier(selectedItem);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Suppliers.Remove(selectedItem);
                        // Also remove from multiple selection HashSet if it was selected
                        _selectedSuppliers.Remove(selectedItem);
                        DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged(); // Update button state for multi-delete
                    });
                }
            }
            else
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select a supplier to delete.", "No Supplier Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanDeleteSupplier(Supplier selectedItem)
        {
            return CurrentUserPrincipal?.HasPermission("DeleteSupplier") == true && selectedItem != null;
        }

        // Import Excel
        public void ImportExcelFile()
        {
            supplierService.ImportExcelFile();
            LoadSuppliersFromDatabase(); // Refresh after import
        }

        private bool CanImportExcel()
        {
            return CurrentUserPrincipal?.HasPermission("ImportSupplierExcel") == true;
        }

        // Export Excel
        public void ExportExcelFile(DataGrid supplierDataGrid)
        {
            supplierService.ExportExcelFile(supplierDataGrid);
        }

        private bool CanExportExcel(DataGrid parameter)
        {
            return CurrentUserPrincipal?.HasPermission("ExportSupplierExcel") == true;
        }

        // New: Delete Multiple Suppliers Logic (now command-driven)
        public void DeleteMultipleSuppliers()
        {
            if (_selectedSuppliers.Count == 0)
            {
                // TODO: Replace with custom modal UI for user notification
                MessageBox.Show("Please select at least one supplier to delete.", "Delete Suppliers", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Replace with custom modal UI for confirmation
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete these {_selectedSuppliers.Count} suppliers?", "Delete Suppliers", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Create a temporary list to avoid modification during enumeration
                var suppliersToDelete = _selectedSuppliers.ToList();
                foreach (var supplier in suppliersToDelete)
                {
                    supplierService.DeleteSupplier(supplier);
                    Application.Current.Dispatcher.Invoke(() => Suppliers.Remove(supplier));
                }
                _selectedSuppliers.Clear(); // Clear the tracking HashSet after successful deletion
                DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged(); // Update button state
            }
        }

        /// <summary>
        /// Determines if the DeleteMultipleSuppliersCommand can be executed.
        /// </summary>
        /// <returns>True if the user has permission and at least one supplier is selected, false otherwise.</returns>
        private bool CanDeleteMultipleSuppliers()
        {
            return CurrentUserPrincipal?.HasPermission("DeleteMultipleSupplier") == true && _selectedSuppliers.Count > 0;
        }


        // Refactored: Checkbox event handlers to update _selectedSuppliers and command state
        // These methods are still directly hooked in XAML but now they correctly update the command's state
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Supplier supplier)
            {
                _selectedSuppliers.Add(supplier);
                DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Supplier supplier)
            {
                _selectedSuppliers.Remove(supplier);
                DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }


        // --- Search Methods (unchanged, as they don't directly use commands for permission) ---
        public void SuppliersSearchByName(string name)
        {
            var res = supplierService.SuppliersSearchByName(name);
            UpdateSuppliers(res);
        }

        public void SuppliersSearchByID(string id)
        {
            var res = supplierService.SuppliersSearchByID(id);
            UpdateSuppliers(res);
        }

        private void UpdateSuppliers(System.Collections.Generic.List<Supplier> newSuppliers)
        {
            // The original implementation was comparing SequenceEqual which can be inefficient
            // Re-implementing based on the UnitListPageLogic's more robust UpdateUnitsDisplay
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newSupplierIds = new HashSet<string>(newSuppliers.Select(s => s.SupplierId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = Suppliers.Count - 1; i >= 0; i--)
                {
                    if (!newSupplierIds.Contains(Suppliers[i].SupplierId))
                    {
                        Suppliers.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newSupplier in newSuppliers)
                {
                    if (!Suppliers.Any(s => s.SupplierId == newSupplier.SupplierId))
                    {
                        Suppliers.Add(newSupplier);
                    }
                }
                // After updating the display, ensure the multi-delete command reflects the current selection state
                DeleteMultipleSuppliersCommand?.RaiseCanExecuteChanged();
            });
        }

        public void SearchSupplierOnGoogle(Supplier supplier)
        {
            supplierService.SearchSupplierOnGoogle(supplier);
        }
    }
}
