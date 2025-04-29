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
    /// Interaction logic for AddAccountWindow.xaml
    /// </summary>
    public partial class AddAccountWindow : Window
    {
        private readonly AddAccountWindowLogic logicService;
        public AddAccountWindow()
        {
            InitializeComponent();
            logicService = new AddAccountWindowLogic();
            DataContext = logicService;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.AddAccount(UsernameTextBox.Text, PasswordBox.Password);
            PasswordBox.Clear();
            if (isSuccess)
                this.Close();
        }
    }
}
