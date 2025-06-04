using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View.Windows; // Assuming AddUnitWindow and EditUnitWindow are in this namespace
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
    public class UnitListPageLogic : INotifyPropertyChanged
    {
        private readonly UnitService unitService;

        private ObservableCollection<Unit> units;
        /// <summary>
        /// Collection of units displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<Unit> Units
        {
            get => units;
            set
            {
                units = value;
                OnPropertyChanged();
            }
        }

        private Unit _selectedUnit;
        /// <summary>
        /// The currently selected unit in the DataGrid.
        /// Used for single-item operations like Edit and Delete.
        /// </summary>
        public Unit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                OnPropertyChanged();
                // Re-evaluate CanExecute for commands that depend on a selected item
                ((RelayCommand<Unit>)LoadEditUnitWindowCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand<Unit>)DeleteUnitCommand)?.RaiseCanExecuteChanged();
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
        /// Text bound to the search TextBox for filtering units.
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
        /// The selected item from the search ComboBox (e.g., "Name", "ID").
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

        // Using a HashSet to efficiently manage selected items for multiple deletion
        private readonly HashSet<Unit> _selectedUnits = new HashSet<Unit>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Commands ---
        public ICommand LoadAddUnitWindowCommand { get; private set; }
        public ICommand LoadEditUnitWindowCommand { get; private set; } // Takes Unit parameter
        public ICommand DeleteUnitCommand { get; private set; }          // Takes Unit parameter
        public RelayCommand DeleteMultipleUnitsCommand { get; private set; } // A simple RelayCommand as it doesn't take a parameter from XAML binding

        public UnitListPageLogic()
        {
            this.unitService = UnitService.Instance;
            Units = new ObservableCollection<Unit>();
            LoadUnitsFromDatabase();
            // Subscribe to the service event for real-time updates after adding
            unitService.OnUnitAdded += UnitService_OnUnitAdded;
            InitializeCommands();

            // Set default search criteria
            // Ensure this matches one of the ComboBoxItem contents in XAML
            SelectedSearchCriteria = new ComboBoxItem { Content = "Name" };
        }

        private void InitializeCommands()
        {
            LoadAddUnitWindowCommand = new RelayCommand(LoadAddUnitWindow, CanLoadAddUnitWindow);
            LoadEditUnitWindowCommand = new RelayCommand<Unit>(LoadEditUnitWindow, CanLoadEditUnitWindow);
            DeleteUnitCommand = new RelayCommand<Unit>(DeleteUnit, CanDeleteUnit);
            DeleteMultipleUnitsCommand = new RelayCommand(DeleteMultipleUnits, CanDeleteMultipleUnits);
        }

        /// <summary>
        /// Loads all marketable units from the database into the observable collection.
        /// </summary>
        private void LoadUnitsFromDatabase()
        {
            var unitsFromDb = unitService.GetListOfUnits().Where(u => !u.IsNotMarketable).ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Units.Clear();
                foreach (var unit in unitsFromDb)
                {
                    Units.Add(unit);
                }
            });
            // After loading, the state of multi-select might change, so re-evaluate CanExecute
            DeleteMultipleUnitsCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Handles the event when a new unit is added via the service.
        /// Ensures the UI is updated on the correct dispatcher thread.
        /// </summary>
        private void UnitService_OnUnitAdded(Unit unit)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Units.Add(unit);
            });
        }

        // --- Add Unit Logic ---
        private void LoadAddUnitWindow()
        {
            var addWindow = new AddUnitWindow();
            addWindow.ShowDialog();
            LoadUnitsFromDatabase(); // Refresh data after the window closes
        }

        private bool CanLoadAddUnitWindow()
        {
            // Check if the current user has permission to add units
            return CurrentUserPrincipal?.HasPermission("AddUnit") == true;
        }

        // --- Edit Unit Logic ---
        private void LoadEditUnitWindow(Unit selectedItem)
        {
            if (selectedItem != null)
            {
                var editWindow = new EditUnitWindow(selectedItem);
                editWindow.ShowDialog();
                LoadUnitsFromDatabase(); // Refresh data after the window closes
            }
            else
            {
                MessageBox.Show("Please select a unit to edit.", "No Unit Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadEditUnitWindow(Unit selectedItem)
        {
            // Check permission and ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("EditUnit") == true && selectedItem != null;
        }

        // --- Delete Single Unit Logic ---
        private void DeleteUnit(Unit selectedItem)
        {
            if (selectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete unit '{selectedItem.UnitName}'?", "Delete Unit", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    unitService.DeleteUnit(selectedItem);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Units.Remove(selectedItem);
                        // Also remove from multiple selection HashSet if it was selected
                        _selectedUnits.Remove(selectedItem);
                        DeleteMultipleUnitsCommand?.RaiseCanExecuteChanged(); // Update button state
                    });
                }
            }
            else
            {
                MessageBox.Show("Please select a unit to delete.", "No Unit Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanDeleteUnit(Unit selectedItem)
        {
            // Check permission and ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("DeleteUnit") == true && selectedItem != null;
        }

        // --- Delete Multiple Units Logic ---
        public void DeleteMultipleUnits()
        {
            if (_selectedUnits.Count == 0)
            {
                MessageBox.Show("Please select units to delete!", "Delete Units", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete these {_selectedUnits.Count} units?", "Delete Units", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Create a temporary list to avoid modification during enumeration
                var unitsToDelete = _selectedUnits.ToList();
                foreach (var unit in unitsToDelete)
                {
                    unitService.DeleteUnit(unit);
                    Application.Current.Dispatcher.Invoke(() => Units.Remove(unit));
                }
                _selectedUnits.Clear(); // Clear the tracking HashSet after successful deletion
                DeleteMultipleUnitsCommand?.RaiseCanExecuteChanged(); // Update button state
            }
        }

        private bool CanDeleteMultipleUnits()
        {
            // Enable only if user has permission AND there are items selected in the HashSet
            return CurrentUserPrincipal?.HasPermission("DeleteMultipleUnit") == true && _selectedUnits.Count > 0;
        }

        // --- Checkbox Event Handlers (Called directly from XAML's DataGridTemplateColumn CheckBox) ---
        // These methods update the internal _selectedUnits HashSet.
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is Unit unit)
            {
                _selectedUnits.Add(unit);
                DeleteMultipleUnitsCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is Unit unit)
            {
                _selectedUnits.Remove(unit);
                DeleteMultipleUnitsCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        // --- Search/Filter Logic ---
        /// <summary>
        /// Applies the search filter based on the current SearchText and SelectedSearchCriteria.
        /// </summary>
        private void ApplySearchFilter()
        {
            // If search text is empty, reload all units
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadUnitsFromDatabase();
                return;
            }

            // Perform search based on selected criteria
            List<Unit> filteredUnits;
            string searchBy = SelectedSearchCriteria?.Content.ToString();

            if (searchBy == "ID")
            {
                filteredUnits = UnitsSearchByID(SearchText);
            }
            else // Default to Name search if "Name" is selected or criteria is null/unrecognized
            {
                filteredUnits = UnitsSearchByName(SearchText);
            }

            UpdateUnitsDisplay(filteredUnits);
        }

        /// <summary>
        /// Searches units by name using the UnitService.
        /// This is a separate helper function for the search logic.
        /// </summary>
        public List<Unit> UnitsSearchByName(string name)
        {
            // Assuming UnitService.UnitsSearchByName already handles NotMarketable filter
            return unitService.UnitsSearchByName(name);
        }

        /// <summary>
        /// Searches units by ID using the UnitService.
        /// This is a separate helper function for the search logic.
        /// </summary>
        public List<Unit> UnitsSearchByID(string id)
        {
            // Assuming UnitService.UnitsSearchByID already handles NotMarketable filter
            return unitService.UnitsSearchByID(id);
        }
            if (_selectedUnits.Count == 0)
            {
                MessageBox.Show("Please select at least one unit to delete", "Delete Units", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

        /// <summary>
        /// Updates the Units ObservableCollection with the new filtered list.
        /// This method is designed to minimize UI updates by only adding/removing items as needed.
        /// </summary>
        /// <param name="newUnits">The list of units that should currently be displayed.</param>
        private void UpdateUnitsDisplay(List<Unit> newUnits)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a temporary set of IDs from the new filtered list for efficient lookup
                var newUnitIds = new HashSet<string>(newUnits.Select(u => u.UnitId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = Units.Count - 1; i >= 0; i--)
                {
                    if (!newUnitIds.Contains(Units[i].UnitId))
                    {
                        Units.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newUnit in newUnits)
                {
                    if (!Units.Any(u => u.UnitId == newUnit.UnitId))
                    {
                        Units.Add(newUnit);
                    }
                }
            });
        }
    }
}