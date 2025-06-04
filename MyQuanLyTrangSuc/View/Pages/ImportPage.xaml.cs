using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        private readonly ImportPageLogic logicService;

        public ImportPage()
        {
            InitializeComponent();
            logicService = new ImportPageLogic(this);
            DataContext = logicService;
        }

        private void OnClick_AddRecord_ImportRecordPage(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddRecordWindow();
        }

        private void OnDoubleClick_InspectRecord_ExportRecordPageDataGrid(object sender, MouseButtonEventArgs e)
        {
            logicService.LoadImportDetailsWindow();
        }

        private void viewButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadImportDetailsWindow();
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectedItem != null)
            {
                string selectedCriteria = (searchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                switch (selectedCriteria)
                {
                    case "Supplier":
                        logicService.ImportsSearchByNameOfSupplier(searchTextBox.Text);
                        break;
                    case "ID":
                        logicService.ImportsSearchByID(searchTextBox.Text);
                        break;
                    case "Date":
                        logicService.ImportsSearchByDate(searchTextBox.Text);
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
            logicService.PrintImportRecord();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditImportWindow((Model.Import)importRecordsDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteImport((Model.Import)importRecordsDataGrid.SelectedItem);
        }
    }
}
