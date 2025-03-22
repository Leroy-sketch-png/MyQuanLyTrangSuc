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
            bool isSuccess = logicService.AddCustomer(NameTextBox.Text, EmailTextBox.Text, TelephoneTextBox.Text, AddressTextBox.Text, birthdayDatePicker.SelectedDate, temp.Content.ToString());
            if (isSuccess)
                this.Close();
        }
    }
}
