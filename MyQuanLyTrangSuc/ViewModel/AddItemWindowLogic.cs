using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddItemWindowLogic : INotifyPropertyChanged
    {
        private const string NumericPattern = "[^0-9]+";
        private const string ImageFilter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
        private const string ImageDialogTitle = "Chọn ảnh sản phẩm";
        private const string ErrorTitle = "Lỗi";
        private const string SuccessTitle = "Thông báo";
        private const string ValidationErrorMessage = "Vui lòng nhập đầy đủ thông tin sản phẩm!";
        private const string SuccessMessage = "Sản phẩm đã được thêm thành công!";

        private readonly NotificationWindowLogic notificationWindowLogic;
        private readonly AddItemWindow _window;
        private readonly Regex _numericRegex;

        public event PropertyChangedEventHandler PropertyChanged;

        private Product _product;
        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        private ObservableCollection<ProductCategory> _categories;
        public ObservableCollection<ProductCategory> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        private ProductCategory _selectedCategory;
        public ProductCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                if (_selectedCategory != null)
                {
                    Product.CategoryId = _selectedCategory.CategoryId;
                }
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        public AddItemWindowLogic(AddItemWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _numericRegex = new Regex(NumericPattern);
            notificationWindowLogic = new NotificationWindowLogic();
            Product = new Product();
            LoadCategories();
            GenerateProductId();
        }

        private void LoadCategories()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            Categories = new ObservableCollection<ProductCategory>(db.ProductCategories.ToList());

            if (Categories.Any())
            {
                SelectedCategory = Categories.First();
            }
        }
        public void RefreshListOfCategories()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            Categories.Clear();
            foreach (var category in db.ProductCategories.ToList())
            {
                Categories.Add(category);
            }
        }

        private void GenerateProductId()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            string newId = "SP001";

            var lastProductId = db.Products
                                .Where(p => p.ProductId.StartsWith("SP"))
                                .OrderByDescending(p => p.ProductId)
                                .Select(p => p.ProductId)
                                .FirstOrDefault();

            if (!string.IsNullOrEmpty(lastProductId) && lastProductId.Length > 2)
            {
                string lastIdNumericPart = lastProductId.Substring(2);
                if (int.TryParse(lastIdNumericPart, out int parsedNumber))
                {
                    newId = $"SP{(parsedNumber + 1):D3}";
                }
            }

            Product.ProductId = newId;
            OnPropertyChanged(nameof(Product));
        }

        private bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !Regex.IsMatch(name, @"\d");
        }

        private bool IsValidPrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price)) return false;

            return decimal.TryParse(price, out decimal parsedPrice) && parsedPrice > 0;
        }

        private bool IsValidQuantity(string quantity)
        {
            if (string.IsNullOrWhiteSpace(quantity)) return false;

            return int.TryParse(quantity, out int parsedQuantity) && parsedQuantity > 0;
        }

        public bool AddProduct()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            if (!IsValidName(Product.Name))
            {
                notificationWindowLogic.LoadNotification("Error", "Tên không hợp lệ!", "BottomRight");
                return false;
            }
            if (!IsValidPrice(Product.Price.ToString()))
            {
                notificationWindowLogic.LoadNotification("Error", "Giá phải lớn hơn 0!", "BottomRight");
                return false;
            }
            if (!IsValidQuantity(Product.Quantity.ToString()))
            {
                notificationWindowLogic.LoadNotification("Error", "Số lượng phải lớn hơn 0!", "BottomRight");
                return false;
            }

            if (db.Products.Any(p => p.Name == Product.Name))
            {
                notificationWindowLogic.LoadNotification("Warning", "Tên sản phẩm đã tồn tại!", "BottomRight");
                return false;
            }

            var newProduct = new Product
            {
                ProductId = Product.ProductId,
                Name = Product.Name,
                Price = Product.Price,
                Quantity = Product.Quantity,
                CategoryId = Product.CategoryId,
                ImagePath = Product.ImagePath
            };
            db.Products.Add(newProduct);
            db.Products.Entry(newProduct).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            db.SaveChangesAdded(newProduct);

            notificationWindowLogic.LoadNotification("Success", "Thêm sản phẩm thành công!", "BottomRight");
            _window.Close();
            return true;
        }

        public void RemoveProduct()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            var productToRemove = db.Products.FirstOrDefault(p => p.ProductId == Product.ProductId);

            if (productToRemove != null)
            {
                db.Products.Remove(productToRemove);
                db.SaveChangesRemoved(productToRemove);
                MessageBox.Show("Sản phẩm đã bị xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _window.Close();
            }
            else
            {
                MessageBox.Show("Không tìm thấy sản phẩm để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Cancel()
        {
            _window.Close();
        }

        public void ChooseImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = ImageFilter,
                Title = ImageDialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Product.ImagePath = openFileDialog.FileName;
                OnPropertyChanged(nameof(Product));
            }
        }

        public void ValidateNumericInput(TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        public void ValidatePastedNumericContent(DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (!pastedText.All(char.IsDigit))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        public void ResetProducts()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            db.ResetProducts();
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}