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

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for ResetPasswordWindow.xaml
    /// </summary>
    public partial class ResetPasswordWindow : Window
    {
        ResetPasswordLogic logicService;
        private string email;

        public ResetPasswordWindow(string email)
        {
            InitializeComponent();
            this.email = email;
            ResetPasswordLogic.flag = false;
            logicService = new ResetPasswordLogic();
            DataContext = logicService;
        }
        private void resetGrid_Click(object sender, RoutedEventArgs e)
        {
            logicService.ResetPassword(newPasswordBox, confirmPasswordBox, email);
            if (ResetPasswordLogic.flag) this.Close();
        }
    }
}
