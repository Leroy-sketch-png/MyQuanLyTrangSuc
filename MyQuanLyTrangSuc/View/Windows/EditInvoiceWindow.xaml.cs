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
    /// Interaction logic for EditInvoiceWindow.xaml
    /// </summary>
    public partial class EditInvoiceWindow : Window
    {
        private readonly EditInvoiceWindowLogic logicService;
        public EditInvoiceWindow(Invoice invoice)
        {
            InitializeComponent();
            logicService = new EditInvoiceWindowLogic(invoice);
            DataContext = logicService;
        }

        private void invoiceDetailsDatagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is InvoiceDetail selectedDetail)
            {
                if (MessageBox.Show($"Do you want to remove this invoice detail",
                                    "Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    logicService.RemoveInvoiceDetail(selectedDetail);
                }
            }
        }

        private void applyInvoiceBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.SaveInvoice();
        }

        private void addInvoiceDetailBtn_Click(object sender, RoutedEventArgs e)
        {
            logicService.AddInvoiceDetail();
        }

        private void addNewClientBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
