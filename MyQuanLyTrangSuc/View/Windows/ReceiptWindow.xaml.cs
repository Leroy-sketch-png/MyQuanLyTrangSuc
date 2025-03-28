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
    /// Interaction logic for ReceiptWindow.xaml
    /// </summary>
    public partial class ReceiptWindow : Window
    {
        public ReceiptWindow()
        {
            InitializeComponent();
        }
        //private InvoiceDetailWindowLogic logicService;
        public ReceiptWindow(Invoice selectedInvoiceRecord) {
            InitializeComponent();
            //logicService = new InvoiceDetailWindowLogic(this, selectedInvoiceRecord);
            //DataContext = logicService;
        }

        internal void Measure(System.Drawing.Size pageSize) {
            throw new NotImplementedException();
        }

        private void printButton_Click(object sender, RoutedEventArgs e) {
            //logicService.PrintReceipt(this);
            this.Activate();
        }
    }
}
