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
    /// Interaction logic for ServicesListPage.xaml
    /// </summary>
    public partial class ServiceRecordListPage : Page
    {
        private readonly ServiceRecordListPageLogic logicService;
        public ServiceRecordListPage()
        {
            InitializeComponent();
            logicService = new ServiceRecordListPageLogic(this);
            DataContext = logicService;
        }
        
        private void OnClick_AddServiceRecordWindow(object sender, RoutedEventArgs e) {
            logicService.LoadAddServiceRecordWindow();
        }
        
        private void OnDoubleClick_InspectRecord_ServiceRecordPageDataGrid(object sender, MouseButtonEventArgs e) {
            //logicService.LoadServiceRecordDetailsWindow();
        }
        
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectedItem != null) {
                string selectedCriteria = (searchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedCriteria) {
                    case "Customer":
                        logicService.SearchServiceRecordsByNameOfCustomer(searchTextBox.Text);
                        break;
                    case "ID":
                        logicService.SearchServiceRecordsByID(searchTextBox.Text);
                        break;
                    case "Date":
                        logicService.SearchServiceRecordsByDate(searchTextBox.Text);
                        break;
                }
            }
            if (string.IsNullOrEmpty(searchTextBox.Text)) {
                searchTextBlock.Text = "Search";
            }
        }

        private void printButton_Click(object sender, RoutedEventArgs e) {
            logicService.LoadServiceRecordDetailsWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.HandleServiceRecordDeleted();
        }

    }
}
