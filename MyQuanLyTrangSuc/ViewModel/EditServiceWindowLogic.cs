using MyQuanLyTrangSuc.Model;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.View.Windows
{
    public class EditServiceWindowLogic
    {
        private readonly EditServiceCategoryWindow _window;
        private readonly Service _originalService;

        public EditServiceWindowLogic(EditServiceCategoryWindow window, Service serviceToEdit)
        {
            _window = window;
            _originalService = serviceToEdit; // Làm việc trực tiếp với service gốc

            InitializeEvents();
            SetupInitialData();
        }

        private void InitializeEvents()
        {
            _window.saveButton.Click += SaveButton_Click;
            _window.nameTextBox.LostFocus += ValidateNameField;
            _window.unitPriceTextBox.LostFocus += ValidatePriceField;
        }

        private void SetupInitialData()
        {
            // Hiển thị dữ liệu ban đầu
            _window.IDTextBlock.Text = _originalService.ServiceId;
            _window.nameTextBox.Text = _originalService.ServiceName;
            _window.unitPriceTextBox.Text = _originalService.ServicePrice.ToString();
            _window.moreInfoTextBox.Text = _originalService.MoreInfo;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAllFields())
            {
                // Cập nhật giá trị từ UI vào service gốc
                _originalService.ServiceName = _window.nameTextBox.Text;

                if (decimal.TryParse(_window.unitPriceTextBox.Text, out decimal price))
                {
                    _originalService.ServicePrice = price;
                }

                _originalService.MoreInfo = _window.moreInfoTextBox.Text;

                _window.DialogResult = true;
                _window.Close();
            }
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;

            if (!ValidateName())
            {
                _window.nameTextBox.Focus();
                isValid = false;
            }

            if (!ValidatePrice())
            {
                _window.unitPriceTextBox.Focus();
                isValid = false;
            }

            return isValid;
        }

        private void ValidateNameField(object sender, RoutedEventArgs e)
        {
            ValidateName();
        }

        private bool ValidateName()
        {
            string name = _window.nameTextBox.Text;

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowErrorMessage("Tên dịch vụ không được để trống.");
                return false;
            }

            if (Regex.IsMatch(name, @"\d"))
            {
                ShowErrorMessage("Tên dịch vụ không được chứa số.");
                return false;
            }

            return true;
        }

        private void ValidatePriceField(object sender, RoutedEventArgs e)
        {
            ValidatePrice();
        }

        private bool ValidatePrice()
        {
            if (!decimal.TryParse(_window.unitPriceTextBox.Text, out decimal price))
            {
                ShowErrorMessage("Giá dịch vụ phải là số.");
                return false;
            }

            if (price <= 0)
            {
                ShowErrorMessage("Giá dịch vụ phải lớn hơn 0.");
                return false;
            }

            return true;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Lỗi nhập liệu",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
        }
    }
}