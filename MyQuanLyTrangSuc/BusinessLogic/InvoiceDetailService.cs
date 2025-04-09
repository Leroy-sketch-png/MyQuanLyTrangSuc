using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic {
    public class InvoiceDetailService {
        private readonly InvoiceDetailRepository invoiceDetailRepository;
        private static InvoiceDetailService instance;
        public static InvoiceDetailService Instance {
            get {
                if (instance == null) {
                    instance = new InvoiceDetailService();
                }
                return instance;
            }
        }
        private InvoiceDetailService() {
            invoiceDetailRepository = new InvoiceDetailRepository();
        }

        public void LoadImportDetailsFromDatabase(Invoice SelectedImportRecord, ObservableCollection<InvoiceDetail> InvoiceDetails) {
            List<InvoiceDetail> InvoiceDetailsFromDb = invoiceDetailRepository.LoadInvoiceDetailsFromDatabase(SelectedImportRecord);
            foreach (InvoiceDetail ii in InvoiceDetailsFromDb) {
                InvoiceDetails.Add(ii);
            }
        }

    }
}
