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
    /// Interaction logic for AssignAccountWindow.xaml
    /// </summary>
    public partial class AssignAccountWindow : Window
    {
        private readonly AssignAccountWindowLogic logicService;
        public AssignAccountWindow(Employee employee)
        {
            InitializeComponent();
            logicService = new AssignAccountWindowLogic(employee);
            DataContext = logicService;
        }

        private void assignButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.AssignAccount();
            if (isSuccess) this.Close();
        }

        private void UserGroupComboBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.SelectedIndex = -1;
                logicService.SelectedAccount = null;
            }

        }
    }
}
