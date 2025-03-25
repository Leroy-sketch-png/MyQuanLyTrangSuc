using MyQuanLyTrangSuc.BusinessLogic;
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

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for EditCustomerWindow.xaml
    /// </summary>
    public partial class EditCustomerWindow : Window
    {
        private readonly EditCustomerWindowLogic logicService;
        public EditCustomerWindow(Customer customer)
        {
            InitializeComponent();
            logicService = new EditCustomerWindowLogic(customer);
            DataContext = logicService;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = logicService.EditCustomer();
            if (isSuccess) this.Close();
        }
    }
}
