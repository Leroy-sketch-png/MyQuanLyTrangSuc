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
        private readonly Regex _numericRegex;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public AddItemWindowLogic(AddItemWindow window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
            _numericRegex = new Regex(NumericPattern);

            // Tạo sản phẩm mới & sinh ID khi mở cửa sổ
            Product = new Product();
            GenerateProductId();
        }

        /// <summary>
        /// Sinh ID sản phẩm mới khi mở cửa sổ
        /// </summary>
        private void GenerateProductId()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            string newId = "SP001"; // Giá trị mặc định

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
                    newId = $"SP{(parsedNumber + 1):D3}"; // Định dạng 3 chữ số
                }
            }

            // Cập nhật ID vào Product
            Product = new Product { ProductId = newId };
            OnPropertyChanged(nameof(Product));
        }

        /// <summary>
        /// Thêm sản phẩm vào database
        /// </summary>
        public void AddProduct()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            // Kiểm tra dữ liệu nhập
            if (string.IsNullOrWhiteSpace(Product.Name) || Product.Price == null || Product.Quantity == null)
            {
                MessageBox.Show(ValidationErrorMessage, ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Product.Price <= 0 || Product.Quantity <= 0)
            {
                MessageBox.Show("Giá và số lượng phải lớn hơn 0!", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra trùng tên sản phẩm
            if (db.Products.Any(p => p.Name == Product.Name))
            {
                MessageBox.Show("Tên sản phẩm đã tồn tại!", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Tạo bản sao để lưu vào database
            var newProduct = new Product
            {
                ProductId = Product.ProductId,
                Name = Product.Name,
                Price = Product.Price,
                Quantity = Product.Quantity,
                ImagePath = Product.ImagePath
            };

            // Thêm vào database
            db.Products.Add(newProduct);
            db.SaveChangesAdded(newProduct); // Gọi phương thức SaveChangesAdded

            MessageBox.Show(SuccessMessage, SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            _window.Close();
        }

        /// <summary>
        /// Xóa sản phẩm khỏi database
        /// </summary>
        public void RemoveProduct()
        {
            var db = MyQuanLyTrangSucContext.Instance;

            var productToRemove = db.Products.FirstOrDefault(p => p.ProductId == Product.ProductId);
            if (productToRemove != null)
            {
                db.Products.Remove(productToRemove);
                db.SaveChangesRemoved(productToRemove); // Gọi phương thức SaveChangesRemoved

                MessageBox.Show("Sản phẩm đã bị xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                _window.Close();
            }
            else
            {
                MessageBox.Show("Không tìm thấy sản phẩm để xóa!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Hủy thêm sản phẩm và đóng cửa sổ
        /// </summary>
        public void Cancel()
        {
            _window.Close();
        }

        /// <summary>
        /// Chọn ảnh sản phẩm
        /// </summary>
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
                OnPropertyChanged(nameof(Product.ImagePath)); // Cập nhật UI
            }
        }

        /// <summary>
        /// Kiểm tra đầu vào của textbox có phải số không
        /// </summary>
        public void ValidateNumericInput(TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Kiểm tra dữ liệu dán vào textbox có phải số không
        /// </summary>
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

        /// <summary>
        /// Reset danh sách sản phẩm
        /// </summary>
        public void ResetProducts()
        {
            var db = MyQuanLyTrangSucContext.Instance;
            db.ResetProducts();
        }

        /// <summary>
        /// Thông báo thay đổi để cập nhật UI
        /// </summary>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
