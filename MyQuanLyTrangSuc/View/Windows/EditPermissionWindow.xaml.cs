using MyQuanLyTrangSuc.Model;
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
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View.Windows
{
    /// <summary>
    /// Interaction logic for EditPermissionWindow.xaml
    /// </summary>
    public partial class EditPermissionWindow : Window
    {
        private readonly EditPermissionWindowLogic logicService;
        public EditPermissionWindow(Permission permission)
        {
            InitializeComponent();
            logicService = new EditPermissionWindowLogic(permission);
            DataContext = logicService;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditPermission((int)UserGroupComboBox.SelectedValue, (int)FunctionComboBox.SelectedValue);
            if (isSuccess)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to edit permission. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
