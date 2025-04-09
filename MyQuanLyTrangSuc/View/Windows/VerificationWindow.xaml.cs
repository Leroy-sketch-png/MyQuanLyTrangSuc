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
    /// Interaction logic for VerificationWindow.xaml
    /// </summary>
    public partial class VerificationWindow : Window
    {
        ResetPasswordLogic resetPasswordLogic;
        public VerificationWindow()
        {
            InitializeComponent();
            resetPasswordLogic = new ResetPasswordLogic();
            DataContext = resetPasswordLogic;
            ResetPasswordLogic.flag = false;
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            resetPasswordLogic.SendCode(emailTextBox);
        }

        private void verifyButton_Click(object sender, RoutedEventArgs e)
        {
            resetPasswordLogic.VerifyCode(codeTextBox, emailTextBox);
            if (ResetPasswordLogic.flag == true)
                this.Hide();
        }
    }
}
