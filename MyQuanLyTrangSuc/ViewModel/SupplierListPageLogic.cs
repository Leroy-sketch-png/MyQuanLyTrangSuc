using MahApps.Metro.Controls;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
using System;
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
        // If you need a command for SearchSupplierOnGoogle, you'd add it here too:
        // public ICommand SearchSupplierOnGoogleCommand { get; private set; }

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
            // If you added SearchSupplierOnGoogleCommand, initialize it here:
            // SearchSupplierOnGoogleCommand = new RelayCommand<Supplier>(SearchSupplierOnGoogle, CanSearchSupplierOnGoogle);
        }

        private void LoadSuppliersFromDatabase()
        {
            var suppliers = supplierService.GetListOfSuppliers().Where(s => !s.IsDeleted).ToList();
            Suppliers = new ObservableCollection<Supplier>(suppliers);
        }

        private void SupplierService_OnSupplierAdded(Supplier obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Suppliers.Add(obj);
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
                MessageBox.Show("Please select a supplier to edit.", "No Supplier Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadEditSupplierWindow(Supplier selectedItem)
        {
            return CurrentUserPrincipal?.HasPermission("EditSupplier") == true && selectedItem != null;
        }

        // Delete Supplier
        private void DeleteSupplier(Supplier selectedItem)
        {
            if (selectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete supplier '{selectedItem.Name}'?", "Delete Supplier", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    supplierService.DeleteSupplier(selectedItem);
                    Suppliers.Remove(selectedItem);
                }
            }
            else
            {
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
            if (!Suppliers.SequenceEqual(newSuppliers))
            {
                Suppliers.Clear();
                foreach (var supplier in newSuppliers)
                {
                    Suppliers.Add(supplier);
                }
            }
        }

        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Supplier supplier)
            {
                _selectedSuppliers.Add(supplier);
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Supplier supplier)
            {
                _selectedSuppliers.Remove(supplier);
            }
        }

        public void DeleteMultipleSuppliers()
        {
            if (_selectedSuppliers.Count == 0)
            {
                MessageBox.Show("Please select at least one supplier to delete", "Delete Suppliers", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete these suppliers?", "Delete Suppliers", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var supplier in _selectedSuppliers)
                {
                    supplierService.DeleteSupplier(supplier);
                    Suppliers.Remove(supplier);
                }
                _selectedSuppliers.Clear();
            }
        }
    }
}