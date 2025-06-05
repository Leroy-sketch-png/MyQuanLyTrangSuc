using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.View.Windows; // Assuming AddImportWindow, ImportDetailsWindow are here
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
    public class ImportPageLogic : INotifyPropertyChanged
    {
        private readonly ImportPage importPage;
        private readonly MyQuanLyTrangSucContext context;
        private readonly ImportService importService;

        private ObservableCollection<Import> _importRecords;
        /// <summary>
        /// Collection of import records displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<Import> ImportRecords
        {
            get => _importRecords;
            set
            {
                _importRecords = value;
                OnPropertyChanged();
            }
        }

        private Import _selectedImportRecord;
        /// <summary>
        /// The currently selected import record in the DataGrid.
        /// </summary>
        public Import SelectedImportRecord
        {
            get => _selectedImportRecord;
            set
            {
                _selectedImportRecord = value;
                OnPropertyChanged();
                // Re-evaluate CanExecute for commands that depend on a selected item
                ((RelayCommand<Import>)LoadImportDetailsWindowCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand<Import>)PrintImportRecordCommand)?.RaiseCanExecuteChanged();
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
        /// Text bound to the search TextBox for filtering import records.
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
        /// The selected item from the search ComboBox (e.g., "Date", "ID", "Supplier").
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
        public ICommand LoadAddImportWindowCommand { get; private set; }
        public ICommand LoadImportDetailsWindowCommand { get; private set; } // Takes Import parameter
        public ICommand PrintImportRecordCommand { get; private set; }       // Takes Import parameter

        public ImportPageLogic()
        {
            context = MyQuanLyTrangSucContext.Instance;
            importService = ImportService.Instance;
            ImportRecords = new ObservableCollection<Import>();
            LoadRecordsFromDatabase();
            importService.OnImportAdded += ImportService_OnImportAdded;
            InitializeCommands();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Supplier" };
        }
        public ImportPageLogic(ImportPage importPage)
        {
            this.importPage = importPage;
            context = MyQuanLyTrangSucContext.Instance;
            importService = ImportService.Instance;
            ImportRecords = new ObservableCollection<Import>();
            LoadRecordsFromDatabase();
            importService.OnImportAdded += ImportService_OnImportAdded;
            InitializeCommands();

            // Set default search criteria
            SelectedSearchCriteria = new ComboBoxItem { Content = "Supplier" };
            importService.OnImportUpdated += ImportService_OnImportUpdated;
        }

        private void ImportService_OnImportUpdated(Import obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LoadRecordsFromDatabase();
            });
        }

        private void ImportService_OnImportAdded(Import import)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImportRecords.Add(import);
            });
        }
        private void InitializeCommands()
        {
            LoadAddImportWindowCommand = new RelayCommand(LoadAddImportWindow, CanLoadAddImportWindow);
            LoadImportDetailsWindowCommand = new RelayCommand<Import>(LoadImportDetailsWindow, CanLoadImportDetailsWindow);
            PrintImportRecordCommand = new RelayCommand<Import>(PrintImportRecord, CanPrintImportRecord);
        }

        private void LoadRecordsFromDatabase()
        {
            try
            {
                // Ensure Supplier data is eager-loaded with the import record
                List<Import> importRecordsFromDb = context.Imports
                    .Include(i => i.Supplier)
                    .ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ImportRecords.Clear();
                    foreach (Import ir in importRecordsFromDb)
                    {
                        ImportRecords.Add(ir);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Add Import Logic ---
        private void LoadAddImportWindow()
        {
            AddImportWindow addImportRecordWindowUI = new AddImportWindow();
            addImportRecordWindowUI.ShowDialog();
            LoadRecordsFromDatabase(); // Refresh data after the window closes
        }

        private bool CanLoadAddImportWindow()
        {
            return CurrentUserPrincipal?.HasPermission("AddImport") == true;
        }

        // --- View Import Details Logic ---
        private void LoadImportDetailsWindow(Import selectedImportRecord)
        {
            if (selectedImportRecord != null)
            {
                ImportDetailsWindow importDetailsWindowUI = new ImportDetailsWindow(selectedImportRecord);
                importDetailsWindowUI.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select an import record to view details.", "No Import Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadImportDetailsWindow(Import selectedImportRecord)
        {
            return CurrentUserPrincipal?.HasPermission("ViewImportDetails") == true && selectedImportRecord != null;
        }

        // --- Print Import Logic ---
        public void PrintImportRecord(Import selectedImportRecord)
        {
            if (selectedImportRecord != null)
            {
                var printPage = new ImportDetailsWindow(selectedImportRecord); // Assuming ImportDetailsWindow is your printable view
                var printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    printPage.Show(); // Must be shown to be printable by PrintVisual
                    printPage.UpdateLayout(); // Ensure layout is updated before printing
                    printDialog.PrintVisual(printPage, $"Import Record - {selectedImportRecord.ImportId}");
                    printPage.Close(); // Close the window after printing
                }
            }
            else
            {
                MessageBox.Show("Please select an import record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanPrintImportRecord(Import selectedImportRecord)
        {
            return CurrentUserPrincipal?.HasPermission("PrintImport") == true && selectedImportRecord != null;
        }

        // --- Search/Filter Logic ---
        /// <summary>
        /// Applies the search filter based on the current SearchText and SelectedSearchCriteria.
        /// </summary>
        private void ApplySearchFilter()
        {
            // If search text is empty, reload all import records
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadRecordsFromDatabase();
                return;
            }

            // Perform search based on selected criteria
            List<Import> filteredImports;
            string searchBy = SelectedSearchCriteria?.Content.ToString();

            switch (searchBy)
            {
                case "ID":
                    filteredImports = ImportsSearchByID(SearchText);
                    break;
                case "Supplier":
                    filteredImports = ImportsSearchByNameOfSupplier(SearchText);
                    break;
                case "Date":
                    filteredImports = ImportsSearchByDate(SearchText);
                    break;
                default: // Default to Supplier search if criteria is null/unrecognized
                    filteredImports = ImportsSearchByNameOfSupplier(SearchText);
                    break;
            }

            UpdateImportRecordsDisplay(filteredImports);
        }

        /// <summary>
        /// Searches import records by supplier name.
        /// </summary>
        public List<Import> ImportsSearchByNameOfSupplier(string name_supplier)
        {
            // Include Supplier navigation property for filtering by supplier name
            return context.Imports
                          .Include(i => i.Supplier)
                          .Where(i => i.Supplier != null && i.Supplier.Name.IndexOf(name_supplier, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches import records by Import ID.
        /// </summary>
        public List<Import> ImportsSearchByID(string ID)
        {
            return context.Imports
                          .Include(i => i.Supplier) // Still include supplier for consistent data display
                          .Where(i => i.ImportId.IndexOf(ID, StringComparison.OrdinalIgnoreCase) >= 0)
                          .ToList();
        }

        /// <summary>
        /// Searches import records by date. Handles partial date inputs (day, month, year).
        /// </summary>
        public List<Import> ImportsSearchByDate(string date)
        {
            List<Import> matchingImports = new List<Import>();
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                matchingImports = context.Imports
                                          .Include(i => i.Supplier)
                                          .Where(i => i.Date.HasValue &&
                                                      i.Date.Value.Date == parsedDate.Date)
                                          .ToList();
            }
            else
            {
                var dateParts = date.Split(new char[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                var allImports = context.Imports.Include(i => i.Supplier).ToList();

                foreach (var import in allImports)
                {
                    if (!import.Date.HasValue) continue;

                    bool match = true;
                    if (dateParts.Length >= 1 && int.TryParse(dateParts[0], out int part1))
                    {
                        if (import.Date.Value.Day != part1 && import.Date.Value.Month != part1)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length >= 2 && int.TryParse(dateParts[1], out int part2))
                    {
                        if (import.Date.Value.Month != part2 && import.Date.Value.Year != part2)
                        {
                            match = false;
                        }
                    }
                    if (dateParts.Length >= 3 && int.TryParse(dateParts[2], out int part3))
                    {
                        if (import.Date.Value.Year != part3)
                        {
                            match = false;
                        }
                    }

                    if (match && dateParts.Length > 0)
                    {
                        matchingImports.Add(import);
                    }
                }
            }
            return matchingImports;
        }

        /// <summary>
        /// Updates the ImportRecords ObservableCollection with the new filtered list.
        /// </summary>
        /// <param name="newImports">The list of imports that should currently be displayed.</param>
        private void UpdateImportRecordsDisplay(List<Import> newImports)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var newImportIds = new HashSet<string>(newImports.Select(i => i.ImportId));

                for (int i = ImportRecords.Count - 1; i >= 0; i--)
                {
                    if (!newImportIds.Contains(ImportRecords[i].ImportId))
                    {
                        ImportRecords.RemoveAt(i);
                    }
                }

                foreach (var newImport in newImports)
                {
                    if (!ImportRecords.Any(ir => ir.ImportId == newImport.ImportId))
                    {
                        ImportRecords.Add(newImport);
                    }
                }
            });
        }

        public void LoadEditImportWindow(Import selectedItem)
        {
            var temp = new EditImportWindow(selectedItem);
            temp.ShowDialog();
        }

        public void DeleteImport(Import selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this import?", "Delete Import", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                importService.DeleteImport(selectedItem);
                ImportRecords.Remove(selectedItem);
            }
        }
    }
}