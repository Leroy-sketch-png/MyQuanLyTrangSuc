using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ItemPropertiesPageLogic : INotifyPropertyChanged
    {
        private readonly MainNavigationWindowLogic _mainNavigationWindowLogic;
        private readonly MyQuanLyTrangSucContext _context;
        private readonly NotificationWindowLogic _notificationWindowLogic;

        private ItemPropertiesPage _itemPropertiesPageUI;
        private bool _isEditing;

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ItemPropertiesPageLogic()
        {
            _mainNavigationWindowLogic = MainNavigationWindowLogic.Instance ?? throw new ArgumentNullException(nameof(MainNavigationWindowLogic));
            _context = MyQuanLyTrangSucContext.Instance ?? throw new ArgumentNullException(nameof(MyQuanLyTrangSucContext));
            _notificationWindowLogic = new NotificationWindowLogic();
        }

        public ItemPropertiesPageLogic(ItemPropertiesPage itemPropertiesPageUI) : this()
        {
            _itemPropertiesPageUI = itemPropertiesPageUI ?? throw new ArgumentNullException(nameof(itemPropertiesPageUI));
            LoadComboBoxSources();
        }

        private void LoadComboBoxSources()
        {
            try
            {
                _itemPropertiesPageUI.inputItemCategory.ItemsSource = _context.ProductCategories
                    .AsNoTracking()
                    .Select(pc => pc.CategoryName)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading category/unit data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadItemListPage()
        {
            if (_isEditing)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Apply or discard before proceeding?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    EditItem();
                else if (result == MessageBoxResult.No)
                    ResetItemChanges();
                else
                    return;

                _isEditing = false;
            }

            _mainNavigationWindowLogic.NavigateToPage(typeof(ItemListPage), "ItemListPage");
        }

        private void ResetItemChanges()
        {
            if (SelectedProduct == null) return;

            try
            {
                var reloaded = _context.Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Unit)
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ProductId == SelectedProduct.ProductId);

                if (reloaded != null)
                    SelectedProduct = reloaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void EditItem()
        {
            ToggleEditMode(_isEditing);
            _isEditing = !_isEditing;

            if (!_isEditing)
            {
                var product = SelectedProduct;
                if (product == null)
                {
                    MessageBox.Show("No product loaded!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                product.Name = _itemPropertiesPageUI.inputItemName.Text;
                product.Price = decimal.TryParse(_itemPropertiesPageUI.inputItemPrice.Text, out var p) ? p : product.Price;
                product.Quantity = int.TryParse(_itemPropertiesPageUI.inputItemStock.Text, out var q) ? q : product.Quantity;
                product.MoreInfo = _itemPropertiesPageUI.itemDescription.Text;

                var catName = _itemPropertiesPageUI.inputItemCategory.Text?.Trim().ToLower();

                var category = _context.ProductCategories
                    .Include(c => c.Unit)
                    .AsEnumerable() // enables use of ToLower(), Trim()
                    .FirstOrDefault(c =>
                        c.CategoryName?.Trim().ToLower() == catName);

                if (category == null)
                {
                    MessageBox.Show("Invalid category or unit!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                product.Category = category;

                try
                {
                    var local = _context.Products.Local.FirstOrDefault(x => x.ProductId == product.ProductId);
                    if (local != null)
                        _context.Entry(local).State = EntityState.Detached;

                    _context.Attach(product);
                    _context.Entry(product).State = EntityState.Modified;
                    _context.SaveChanges();

                    _notificationWindowLogic.LoadNotification("Success", "Edited product successfully", "BottomRight");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ToggleEditMode(bool editing)
        {
            var viewMode = editing ? Visibility.Visible : Visibility.Collapsed;
            var editMode = editing ? Visibility.Collapsed : Visibility.Visible;

            _itemPropertiesPageUI.itemName.Visibility = viewMode;
            _itemPropertiesPageUI.itemCategory.Visibility = viewMode;
            _itemPropertiesPageUI.itemPrice.Visibility = viewMode;
            _itemPropertiesPageUI.itemStock.Visibility = viewMode;
            _itemPropertiesPageUI.itemImage.Visibility = viewMode;
            _itemPropertiesPageUI.itemStatus.Visibility = viewMode;

            _itemPropertiesPageUI.inputItemName.Visibility = editMode;
            _itemPropertiesPageUI.inputItemCategory.Visibility = editMode;
            _itemPropertiesPageUI.inputItemPrice.Visibility = editMode;
            _itemPropertiesPageUI.inputItemStock.Visibility = editMode;
            _itemPropertiesPageUI.inputItemImage.Visibility = editMode;

            _itemPropertiesPageUI.editButton.Content = editing ? "Edit?" : "Apply!";
        }

        public void EditItemImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true && SelectedProduct != null)
                SelectedProduct.ImagePath = openFileDialog.FileName;
        }

        public void OnCategoryChanged(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return;

            var category = _context.ProductCategories
                .Include(pc => pc.Unit)
                .FirstOrDefault(pc => pc.CategoryName == categoryName);

            if (category == null) return;

            SelectedProduct.Category = category;
            OnPropertyChanged(nameof(SelectedProduct));
        }

        public void LoadProductDetails(string productId)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Unit)
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ProductId == productId);

                if (product != null)
                {
                    SelectedProduct = product;
                    OnPropertyChanged(nameof(SelectedProduct));
                }
                else
                {
                    MessageBox.Show($"No product found with ID: {productId}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading product details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }
    }
}
