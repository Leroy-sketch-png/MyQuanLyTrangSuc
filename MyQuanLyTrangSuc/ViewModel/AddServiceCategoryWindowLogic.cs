using System;
using System.Windows.Input;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.DataAccess;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.View.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddServiceCategoryWindowLogic : INotifyPropertyChanged
    {
        private readonly AddServiceCategoryWindow _view;

        public AddServiceCategoryWindowLogic(AddServiceCategoryWindow view)
        {
            _view = view;
            CategoryName = string.Empty;
            CategoryDescription = string.Empty;
        }

        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }

        public ICommand AddCategoryCommand => new RelayCommand(AddCategory, CanAddCategory);
        public ICommand CancelCommand => new RelayCommand(Cancel);

        // Kiểm tra xem có thể thêm category không (ví dụ: tên không được để trống)
        private bool CanAddCategory(object parameter)
        {
            return !string.IsNullOrWhiteSpace(CategoryName);
        }

        // Thêm Category vào database
        private void AddCategory(object parameter)
        {
            try
            {
                var newCategory = new ServiceCategory
                {
                    Name = CategoryName,
                    Description = CategoryDescription
                };

                var dataContext = new MyDbContext();
                dataContext.ServiceCategories.Add(newCategory);
                dataContext.SaveChanges();

                // Hiển thị thông báo thành công
                _view.DialogResult = true;
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ (Có thể thêm thông báo lỗi ở đây)
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Hủy bỏ thao tác
        private void Cancel(object parameter)
        {
            _view.DialogResult = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
