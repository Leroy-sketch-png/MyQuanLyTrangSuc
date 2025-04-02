using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddItemWindowLogic : INotifyPropertyChanged
    {
        // Constants
        private const string NumericPattern = "[^0-9]+";
        private const string ImageFilter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
        private const string ImageDialogTitle = "Chọn ảnh sản phẩm";

        // Error messages
        private const string ErrorTitle = "Lỗi";
        private const string SuccessTitle = "Thông báo";
        private const string ValidationErrorMessage = "Vui lòng nhập đầy đủ thông tin sản phẩm!";
        private const string SuccessMessage = "Sản phẩm đã được thêm thành công!";

        private readonly AddItemWindow _window;
        private Product _product;
        private readonly Regex _numericRegex;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Product Product
        {
            get => _product;
            set
            {
                _product = value;
                OnPropertyChanged(nameof(Product));
            }
        }

        public AddItemWindowLogic(AddItemWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _product = new Product();
            _numericRegex = new Regex(NumericPattern);

            // Generate automatic ID when window opens
            GenerateProductId();
        }

        /// <summary>
        /// Generates a unique product ID based on the highest existing ID in the database
        /// </summary>
        private void GenerateProductId()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            // Default ID if no products exist
            string newId = "SP001";  // Prefix for product ID (e.g., "SP" for sản phẩm)

            // Find the last product ID and generate a new one
            var lastProductId = db.Products
                                  .OrderByDescending(p => p.ProductId)
                                  .Select(p => p.ProductId)
                                  .FirstOrDefault();

            if (lastProductId != null)
            {
                // Extract numeric part from last product ID
                string lastIdNumericPart = lastProductId.Substring(2); // Assuming the first 2 characters are the prefix (e.g., "SP")
                if (int.TryParse(lastIdNumericPart, out int parsedNumber))
                {
                    int newNumber = parsedNumber + 1;
                    newId = $"SP{newNumber:D3}";  // Generate new ID with leading zeros (e.g., SP001, SP002, etc.)
                }
            }

            Product.ProductId = newId;  // Assign the generated ID to the Product
            OnPropertyChanged(nameof(Product.ProductId));
        }

        public void AddProduct()
        {
            if (string.IsNullOrWhiteSpace(Product.Name) || Product.Price == null || Product.Quantity == null)
            {
                MessageBox.Show(ValidationErrorMessage, ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var db = MyQuanLyTrangSucContext.Instance;
            db.Products.Add(Product);
            db.SaveChanges();

            MessageBox.Show(SuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            _window.Close();
        }

        public void Cancel()
        {
            _window.Close();
        }

        public void ChooseImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = ImageFilter,
                Title = ImageDialogTitle
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Product.ImagePath = openFileDialog.FileName;
                // Explicitly trigger property change for ImagePath
                OnPropertyChanged(nameof(Product));
                OnPropertyChanged(nameof(Product.ImagePath));
            }
        }

        public void ValidateNumericInput(TextCompositionEventArgs e)
        {
            e.Handled = _numericRegex.IsMatch(e.Text);
        }

        public void ValidatePastedNumericContent(DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                if (_numericRegex.IsMatch(pastedText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
