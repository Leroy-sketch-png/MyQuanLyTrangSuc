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
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for AddCustomerWindow.xaml
    /// </summary>
    public partial class AddCustomerWindow : Window
    {
        private readonly AddCustomerWindowLogic logicService;
        public AddCustomerWindow()
        {
            InitializeComponent();
            logicService = new AddCustomerWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton temp = new RadioButton();
            if (maleRadioButton.IsChecked == true)
                temp = maleRadioButton;
            else
                temp = femaleRadioButton;

            if (!logicService.IsValidData(NameTextBox.Text, EmailTextBox.Text, TelephoneTextBox.Text))
            {
                MessageBox.Show("Thông tin không hợp lệ! Vui lòng nhập đúng định dạng", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            logicService.AddCustomerToDatabase(NameTextBox, EmailTextBox, TelephoneTextBox, birthdayDatePicker, temp, AddressTextBox);
            this.Close();
        }
    }
}
