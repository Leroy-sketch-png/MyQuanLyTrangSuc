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
    /// Interaction logic for ExportDetailsWindow.xaml
    /// </summary>
    public partial class InvoiceDetailsWindow : Window
    {
        MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        public InvoiceDetailsWindow()
        {
            InitializeComponent();
            Invoice selectedInvoiceRecord = context.Invoices.FirstOrDefault();
            InvoiceDetailWindowLogic invoiceDetailWindowLogic = new InvoiceDetailWindowLogic(this, selectedInvoiceRecord);
            this.DataContext = invoiceDetailWindowLogic;

        }
        public InvoiceDetailsWindow(Invoice selectedInvoiceRecord) {
            InitializeComponent();
            InvoiceDetailWindowLogic invoiceDetailWindowLogic = new InvoiceDetailWindowLogic(this, selectedInvoiceRecord);
            this.DataContext = invoiceDetailWindowLogic;
        }

    }
}
