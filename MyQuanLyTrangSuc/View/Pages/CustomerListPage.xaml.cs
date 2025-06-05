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
    /// Interaction logic for CustomerListPage.xaml
    /// </summary>
    public partial class CustomerListPage : Page
    {   
        private readonly CustomerListPageLogic logicService;
        public CustomerListPage()
        {
            InitializeComponent();
            logicService = new CustomerListPageLogic();
            DataContext = logicService;
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Name")
                {
                    logicService.CustomersSearchByName(searchTextBox.Text);
                }
                else if (selectedValue == "ID")
                {
                    logicService.CustomersSearchByID(searchTextBox.Text);
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_Checked(sender, e);

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            logicService.CheckBox_UnChecked(sender, e);
        }

    }
}
