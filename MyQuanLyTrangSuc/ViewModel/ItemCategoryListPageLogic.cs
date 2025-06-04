using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Assuming CustomPrincipal is here
using MyQuanLyTrangSuc.View;
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
    public class ItemCategoryListPageLogic : INotifyPropertyChanged
    {
        private readonly ItemCategoryService itemCategoryService;

        private ObservableCollection<ProductCategory> itemCategories;
        /// <summary>
        /// Collection of item categories displayed in the DataGrid.
        /// </summary>
        public ObservableCollection<ProductCategory> ItemCategories
        {
            get => itemCategories;
            set
            {
                itemCategories = value;
                OnPropertyChanged();
            }
        }

        private ProductCategory _selectedItemCategory;
        /// <summary>
        /// The currently selected item category in the DataGrid.
        /// Used for single-item operations like Edit and Delete.
        /// </summary>
        public ProductCategory SelectedItemCategory
        {
            get => _selectedItemCategory;
            set
            {
                _selectedItemCategory = value;
                OnPropertyChanged();
                // Re-evaluate CanExecute for commands that depend on a selected item
                ((RelayCommand<ProductCategory>)LoadEditItemCategoryWindowCommand)?.RaiseCanExecuteChanged();
                ((RelayCommand<ProductCategory>)DeleteItemCategoryCommand)?.RaiseCanExecuteChanged();
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
        /// Text bound to the search TextBox for filtering item categories.
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
        private readonly HashSet<ProductCategory> _selectedItemCategories = new HashSet<ProductCategory>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Commands ---
        public ICommand LoadAddItemCategoryWindowCommand { get; private set; }
        public ICommand LoadEditItemCategoryWindowCommand { get; private set; } // Takes ProductCategory parameter
        public ICommand DeleteItemCategoryCommand { get; private set; }          // Takes ProductCategory parameter
        public RelayCommand DeleteMultipleItemCategoriesCommand { get; private set; } // A simple RelayCommand as it doesn't take a parameter from XAML binding

        public ItemCategoryListPageLogic()
        {
            itemCategoryService = ItemCategoryService.Instance;
            ItemCategories = new ObservableCollection<ProductCategory>();
            LoadItemCategoriesFromDatabase();
            // Subscribe to the service event for real-time updates after adding
            itemCategoryService.OnItemCategoryAdded += ItemCategoryService_OnItemCategoryAdded;
            InitializeCommands();

            // Set default search criteria
            // Ensure this matches one of the ComboBoxItem contents in XAML
            SelectedSearchCriteria = new ComboBoxItem { Content = "Name" };
            itemCategoryService.OnItemCategoryUpdated += ItemCategoryService_OnItemCategoryUpdated;
        }

        private void ItemCategoryService_OnItemCategoryUpdated(ProductCategory updatedCategory)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var index = ItemCategories.IndexOf(ItemCategories.FirstOrDefault(c => c.CategoryId == updatedCategory.CategoryId));
                if (index != -1)
                {
                    ItemCategories[index] = updatedCategory;
                }
                else
                {
                    LoadItemCategoriesFromDatabase();
                }
            });
            

        }

        private void InitializeCommands()
        {
            LoadAddItemCategoryWindowCommand = new RelayCommand(LoadAddItemCategoryWindow, CanLoadAddItemCategoryWindow);
            LoadEditItemCategoryWindowCommand = new RelayCommand<ProductCategory>(LoadEditItemCategoryWindow, CanLoadEditItemCategoryWindow);
            DeleteItemCategoryCommand = new RelayCommand<ProductCategory>(DeleteItemCategory, CanDeleteItemCategory);
            DeleteMultipleItemCategoriesCommand = new RelayCommand(DeleteMultipleItemCategories, CanDeleteMultipleItemCategories);
        }

        /// <summary>
        /// Loads all marketable item categories from the database into the observable collection.
        /// </summary>
        private void LoadItemCategoriesFromDatabase()
        {
            var itemCategoriesFromDb = itemCategoryService.GetListOfItemCategories().Where(c => !c.IsNotMarketable).ToList();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ItemCategories.Clear();
                foreach (var category in itemCategoriesFromDb)
                {
                    ItemCategories.Add(category);
                }
            });
            // After loading, the state of multi-select might change, so re-evaluate CanExecute
            DeleteMultipleItemCategoriesCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Handles the event when a new item category is added via the service.
        /// Ensures the UI is updated on the correct dispatcher thread.
        /// </summary>
        private void ItemCategoryService_OnItemCategoryAdded(ProductCategory category)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ItemCategories.Add(category);
            });
        }

        // --- Add Category Logic ---
        private void LoadAddItemCategoryWindow()
        {
            var addWindow = new AddItemCategoryWindow();
            addWindow.ShowDialog();
            LoadItemCategoriesFromDatabase(); // Refresh data after the window closes
        }

        private bool CanLoadAddItemCategoryWindow()
        {
            // Check if the current user has permission to add item categories
            return CurrentUserPrincipal?.HasPermission("AddItemCategory") == true;
        }

        // --- Edit Category Logic ---
        private void LoadEditItemCategoryWindow(ProductCategory selectedItem)
        {
            if (selectedItem != null)
            {
                var editWindow = new EditItemCategoryWindow(selectedItem);
                editWindow.ShowDialog();
                LoadItemCategoriesFromDatabase(); // Refresh data after the window closes
            }
            else
            {
                MessageBox.Show("Please select an item category to edit.", "No Category Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanLoadEditItemCategoryWindow(ProductCategory selectedItem)
        {
            // Check permission and ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("EditItemCategory") == true && selectedItem != null;
        }

        // --- Delete Single Category Logic ---
        private void DeleteItemCategory(ProductCategory selectedItem)
        {
            if (selectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete category '{selectedItem.CategoryName}'?", "Delete Item Category", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    itemCategoryService.DeleteItemCategory(selectedItem);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ItemCategories.Remove(selectedItem);
                        // Also remove from multiple selection HashSet if it was selected
                        _selectedItemCategories.Remove(selectedItem);
                        DeleteMultipleItemCategoriesCommand?.RaiseCanExecuteChanged(); // Update button state
                    });
                }
            }
            else
            {
                MessageBox.Show("Please select an item category to delete.", "No Category Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool CanDeleteItemCategory(ProductCategory selectedItem)
        {
            // Check permission and ensure an item is selected
            return CurrentUserPrincipal?.HasPermission("DeleteItemCategory") == true && selectedItem != null;
        }

        // --- Delete Multiple Categories Logic ---
        public void DeleteMultipleItemCategories()
        {
            if (_selectedItemCategories.Count == 0)
            {
                MessageBox.Show("Please select item categories to delete!", "Delete Item Categories", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete these {_selectedItemCategories.Count} item categories?", "Delete Item Categories", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Create a temporary list to avoid modification during enumeration
                var categoriesToDelete = _selectedItemCategories.ToList();
                foreach (var itemCategory in categoriesToDelete)
                {
                    itemCategoryService.DeleteItemCategory(itemCategory);
                    Application.Current.Dispatcher.Invoke(() => ItemCategories.Remove(itemCategory));
                }
                _selectedItemCategories.Clear(); // Clear the tracking HashSet after successful deletion
                DeleteMultipleItemCategoriesCommand?.RaiseCanExecuteChanged(); // Update button state
            }
        }

        private bool CanDeleteMultipleItemCategories()
        {
            // Enable only if user has permission AND there are items selected in the HashSet
            return CurrentUserPrincipal?.HasPermission("DeleteMultipleItemCategory") == true && _selectedItemCategories.Count > 0;
        }

        // --- Checkbox Event Handlers (Called directly from XAML's DataGridTemplateColumn CheckBox) ---
        // These methods update the internal _selectedItemCategories HashSet.
        public void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is ProductCategory itemCategory)
            {
                _selectedItemCategories.Add(itemCategory);
                DeleteMultipleItemCategoriesCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        public void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox && checkbox.DataContext is ProductCategory itemCategory)
            {
                _selectedItemCategories.Remove(itemCategory);
                DeleteMultipleItemCategoriesCommand?.RaiseCanExecuteChanged(); // Re-evaluate CanExecute for the delete multiple button
            }
        }

        // --- Search/Filter Logic ---
        /// <summary>
        /// Applies the search filter based on the current SearchText and SelectedSearchCriteria.
        /// </summary>
        private void ApplySearchFilter()
        {
            // If search text is empty, reload all categories
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadItemCategoriesFromDatabase();
                return;
            }

            // Perform search based on selected criteria
            List<ProductCategory> filteredCategories;
            string searchBy = SelectedSearchCriteria?.Content.ToString();

            if (searchBy == "ID")
            {
                filteredCategories = ItemCategoriesSearchByID(SearchText);
            }
            else // Default to Name search if "Name" is selected or criteria is null/unrecognized
            {
                filteredCategories = ItemCategoriesSearchByName(SearchText);
            }

            UpdateItemCategoriesDisplay(filteredCategories);
        }

        /// <summary>
        /// Searches item categories by name using the ItemCategoryService.
        /// This is a separate helper function for the search logic.
        /// </summary>
        public List<ProductCategory> ItemCategoriesSearchByName(string name)
        {
            // Assuming ItemCategoryService.ItemCategoriesSearchByName already handles NotMarketable filter
            return itemCategoryService.ItemCategoriesSearchByName(name);
        }

        /// <summary>
        /// Searches item categories by ID using the ItemCategoryService.
        /// This is a separate helper function for the search logic.
        /// </summary>
        public List<ProductCategory> ItemCategoriesSearchByID(string id)
        {
            // Assuming ItemCategoryService.ItemCategoriesSearchByID already handles NotMarketable filter
            return itemCategoryService.ItemCategoriesSearchByID(id);
        }

        /// <summary>
        /// Updates the ItemCategories ObservableCollection with the new filtered list.
        /// This method is designed to minimize UI updates by only adding/removing items as needed.
        /// </summary>
        /// <param name="newCategories">The list of categories that should currently be displayed.</param>
        private void UpdateItemCategoriesDisplay(List<ProductCategory> newCategories)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a temporary set of IDs from the new filtered list for efficient lookup
                var newCategoryIds = new HashSet<string>(newCategories.Select(c => c.CategoryId));

                // Remove items from the current display that are no longer in the filtered list
                for (int i = ItemCategories.Count - 1; i >= 0; i--)
                {
                    if (!newCategoryIds.Contains(ItemCategories[i].CategoryId))
                    {
                        ItemCategories.RemoveAt(i);
                    }
                }

                // Add items to the current display that are in the filtered list but not yet present
                foreach (var newCategory in newCategories)
                {
                    if (!ItemCategories.Any(ic => ic.CategoryId == newCategory.CategoryId))
                    {
                        ItemCategories.Add(newCategory);
                    }
                }
                // Note: When filtering, the checkboxes will uncheck if items are re-added.
                // If you need to preserve checkbox state across filters, you'd need the IsSelected property
                // on ProductCategory, or more complex logic to check/uncheck checkboxes in the DataGrid's
                // LoadingRow event based on the _selectedItemCategories HashSet.
            });
        }
    }
}