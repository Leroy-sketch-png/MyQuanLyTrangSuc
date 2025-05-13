using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for UserPropertiesPageUI.xaml
    /// </summary>
    public partial class EmployeePropertiesPage : Page
    {

        private readonly EmployeePropertiesPageLogic logicService;

        public EmployeePropertiesPage()
        {
            InitializeComponent();
            logicService = new EmployeePropertiesPageLogic(this);
        }
        public EmployeePropertiesPage(Employee employee)
        {
            InitializeComponent();
            logicService = new EmployeePropertiesPageLogic(this);
            this.DataContext = employee;
        }

        private void OnClick_Edit_EmployeePropertiesPage(object sender, RoutedEventArgs e)
        {

            logicService.EditEmployee();
            //if (!logicService.IsValidData(inputEmployeeName, inputEmployeeEmail, inputEmployeePhone))
            //{
            //    MessageBox.Show("Thông tin không hợp lệ! Vui lòng nhập đúng định dạng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
        }

        private void OnClick_Back_EmployeePropertiesPage(object sender, RoutedEventArgs e)
        {
            logicService.LoadEmployeeListPage();
            MessageBox.Show("Có hoạt động");
        }

        private void OnClick_EditImage_EmployeePropertiesPage(object sender, RoutedEventArgs e)
        {

            logicService.EditEmployeeImage();
        }
    }
}
