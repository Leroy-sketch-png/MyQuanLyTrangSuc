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
    /// Interaction logic for AddSupplierWindow.xaml
    /// </summary>
    public partial class AddSupplierWindow : Window
    {
        private AddSupplierWindowLogic logicService;
        public AddSupplierWindow()
        {
            InitializeComponent();
            logicService = new AddSupplierWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (!logicService.IsValidData(NameTextBox.Text, EmailTextBox.Text, TelephoneTextBox.Text))
            {
                MessageBox.Show("Invalid data! Please enter the valid format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            logicService.AddSupplierToDatabase(NameTextBox, EmailTextBox, TelephoneTextBox, AddressTextBox);
            this.Close();
        }
    }
}
