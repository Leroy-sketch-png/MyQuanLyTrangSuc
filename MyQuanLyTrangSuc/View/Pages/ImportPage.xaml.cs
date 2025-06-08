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




        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
