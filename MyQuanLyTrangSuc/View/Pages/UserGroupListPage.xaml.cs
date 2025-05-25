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
    /// Interaction logic for UserGroupListPage.xaml
    /// </summary>
    public partial class UserGroupListPage : Page
    {
        private readonly UserGroupListPageLogic logicService;
        public UserGroupListPage()
        {
            InitializeComponent();
            logicService = new UserGroupListPageLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadAddUserGroupWindow();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.LoadEditUserGroupWindow((Model.UserGroup)userGroupDataGrid.SelectedItem);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            logicService.DeleteUserGroup((Model.UserGroup)userGroupDataGrid.SelectedItem);
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            logicService.SearchUserGroup(searchTextBox.Text);
        }
    }
}
