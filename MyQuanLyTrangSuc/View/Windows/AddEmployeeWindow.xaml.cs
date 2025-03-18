using MyQuanLyTrangSuc.ViewModel;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    
    public partial class AddEmployeeWindow : Window
    {
        private readonly AddEmployeeWindowLogic addEmployeeWindowLogic;

        public AddEmployeeWindow()
        {
            InitializeComponent(); // Đảm bảo giao diện được khởi tạo trước
            addEmployeeWindowLogic = new AddEmployeeWindowLogic(new MyQuanLyTrangSucContext()); // Truyền context hợp lệ
            DataContext = addEmployeeWindowLogic;
        }

        private void ChooseImageButton_Click(object sender, RoutedEventArgs e)
        {
            addEmployeeWindowLogic.ChooseImageFileDialog();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text; 
            string email = EmailTextBox.Text;
            string telephone = TelephoneTextBox.Text;
            string position = PositionComboBox.Text; 
            string imagePath = addEmployeeWindowLogic.ImagePath;

            bool success = addEmployeeWindowLogic.AddEmployeeToDatabase(name, email, telephone, position, imagePath);

            if (success)
            {
                MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close(); // Đóng cửa sổ sau khi thêm thành công
            }
            else
            {
                MessageBox.Show("Thêm nhân viên thất bại! Vui lòng kiểm tra lại thông tin.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}