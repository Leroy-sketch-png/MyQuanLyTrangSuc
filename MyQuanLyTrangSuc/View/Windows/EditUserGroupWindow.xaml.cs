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
    /// Interaction logic for EditUserGroupWindow.xaml
    /// </summary>
    public partial class EditUserGroupWindow : Window
    {
        private readonly EditUserGroupWindowLogic logicService;
        public EditUserGroupWindow(UserGroup userGroup)
        {
            InitializeComponent();
            logicService = new EditUserGroupWindowLogic(userGroup);
            DataContext = logicService;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditUserGroup();
            if (isSuccess) this.Close();
        }
    }
}
