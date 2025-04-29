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
    /// Interaction logic for AddUserGroupWindow.xaml
    /// </summary>
    public partial class AddUserGroupWindow : Window
    {
        private readonly AddUserGroupWindowLogic logicService;
        public AddUserGroupWindow()
        {
            InitializeComponent();
            logicService = new AddUserGroupWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.AddUserGroup(NameTextBox.Text);
            if (isSuccess)
                this.Close();
        }
    }
}
