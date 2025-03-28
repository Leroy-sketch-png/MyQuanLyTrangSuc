using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel {
    public class InvoiceDetailWindowLogic {
        private readonly InvoiceDetailsWindow invoiceDetailsWindowUI;
        private readonly InvoiceDetailService invoiceDetailService = InvoiceDetailService.Instance;

        //DataContext Zone
        public Invoice SelectedInvoiceRecord { get; set; }
        public ObservableCollection<InvoiceDetail> InvoiceDetails { get; set; }

        //

        public InvoiceDetailWindowLogic() {
            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
        }
        public InvoiceDetailWindowLogic(InvoiceDetailsWindow invoiceDetailsWindowUI, Invoice selectedInvoiceRecord) {
            this.invoiceDetailsWindowUI = invoiceDetailsWindowUI;
            this.SelectedInvoiceRecord = selectedInvoiceRecord;

            InvoiceDetails = new ObservableCollection<InvoiceDetail>();
            invoiceDetailService.LoadImportDetailsFromDatabase(SelectedInvoiceRecord, InvoiceDetails);
        }


    }
}
