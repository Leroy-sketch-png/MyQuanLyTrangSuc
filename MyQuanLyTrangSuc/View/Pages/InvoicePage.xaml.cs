using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for InvoicePage.xaml
    /// </summary>
    public partial class InvoicePage : Page
    {
        private readonly InvoicePageLogic logicService;

        public InvoicePage()
        {
            InitializeComponent();
            logicService = new InvoicePageLogic(this);
            DataContext = logicService;
        }

        private void OnClick_AddInvoiceWindow(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddInvoiceWindow();
        }

        private void OnDoubleClick_InspectRecord_InvoicePageDataGrid(object sender, MouseButtonEventArgs e)
        {
            logicService.LoadInvoiceDetailsWindow();
        }

        private void viewButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadInvoiceDetailsWindow();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectedItem != null)
            {
                string selectedCriteria = (searchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedCriteria)
                {
                    case "Customer":
                        logicService.SearchInvoicesByNameOfCustomer(searchTextBox.Text);
                        break;
                    case "ID":
                        logicService.SearchInvoicesByID(searchTextBox.Text);
                        break;
                    case "Date":
                        logicService.SearchInvoicesByDate(searchTextBox.Text);
                        break;
                }
            }
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBlock.Text = "Search";
            }
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadInvoiceWindow();
        }
    }
}
