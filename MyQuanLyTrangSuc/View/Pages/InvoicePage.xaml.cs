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

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditInvoiceWindow((Model.Invoice)InvoicesDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteInvoice((Model.Invoice)InvoicesDataGrid.SelectedItem);
        }
    }
}
