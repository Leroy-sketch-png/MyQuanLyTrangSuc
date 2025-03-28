using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess {
    public class InvoiceDetailRepository {
        MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        public List<InvoiceDetail> LoadInvoiceDetailsFromDatabase(Invoice SelectedInvoiceRecord) {
            List<InvoiceDetail> InvoiceDetailsFromDb = context.InvoiceDetails.Where(ii => ii.InvoiceId == SelectedInvoiceRecord.InvoiceId).ToList();
            //foreach (ImportDetail ii in ImportDetailsFromDb) {
            //ImportDetails.Add(ii);
            //}
            return InvoiceDetailsFromDb;
        }

    }
}
