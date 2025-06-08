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

        // Search functionality
        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
        }
    }
}
