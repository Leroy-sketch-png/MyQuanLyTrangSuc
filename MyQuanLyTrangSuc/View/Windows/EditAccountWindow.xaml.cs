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
    /// Interaction logic for EditAccountWindow.xaml
    /// </summary>
    public partial class EditAccountWindow : Window
    {
        private readonly EditAccountWindowLogic logicService;
        public EditAccountWindow(Account account)
        {
            InitializeComponent();
            logicService = new EditAccountWindowLogic(account);
            DataContext = logicService;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditAccount();
            if (isSuccess) this.Close();
        }
    }
}
