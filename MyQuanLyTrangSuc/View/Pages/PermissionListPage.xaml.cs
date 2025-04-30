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

namespace MyQuanLyTrangSuc.View.Pages
{
    /// <summary>
    /// Interaction logic for PermissionListPage.xaml
    /// </summary>
    public partial class PermissionListPage : Page
    {
        private readonly PermissionListPageLogic logicService;
        public PermissionListPage()
        {
            InitializeComponent();
            logicService = new PermissionListPageLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddPermissionWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditPermissionWindow((Model.Permission)permissionDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeletePermission((Model.Permission)permissionDataGrid.SelectedItem);

        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content.ToString();

                if (selectedValue == "Group Name")
                {
                    logicService.PermissionsSearchByGroupName(searchTextBox.Text);
                }
                else if (selectedValue == "Function Name")
                {
                    logicService.PermissionsSearchByFunctionName(searchTextBox.Text);
                }
            }
        }
    }
}
