using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for EmployeeListPageUI.xaml
    /// </summary>
    public partial class EmployeeListPage : Page
    {
        private readonly EmployeeListPageLogic logicService;

        public EmployeeListPage()
        {
            InitializeComponent();
            logicService = new EmployeeListPageLogic(this);
            DataContext = logicService;
        }

        // Add function
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddEmployeeWindow();
        }

        // Delete function
        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (logicService.SelectedEmployee != null)
            {
                MessageBoxResult result = MessageBox.Show("Do you want to delete this employee?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    logicService.DeleteEmployee();
                }
            }
            else
            {
                MessageBox.Show("No employee selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Edit function
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            // Assuming you have a method to load employee properties page
            logicService.LoadEmployeePropertiesPage();
            //MessageBox.Show("Edit functionality is not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Search functionality
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchTextBlock.Text = "";
            if (searchComboBox.SelectedItem != null)
            {
                string searchCriteria = ((ComboBoxItem)searchComboBox.SelectedItem).Content.ToString();
                if (searchCriteria == "Name")
                {
                    logicService.EmployeesSearchByName(searchTextBox.Text);
                }
                else if (searchCriteria == "ID")
                {
                    logicService.EmployeesSearchByID(searchTextBox.Text);
                }
            }
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBlock.Text = "Search by name";
            }
        }

        // Export to Excel
        private void exportExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.ExportExcelFile(employeesDataGrid);
        }

        // Import from Excel
        private void importExcelFileButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.ImportExcelFile();
        }
    }
}
