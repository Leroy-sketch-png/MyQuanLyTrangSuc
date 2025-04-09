using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class InvoiceRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public InvoiceRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }

        public string GetLastInvoiceID()
        {
            var lastID = context.Invoices.OrderByDescending(i => i.InvoiceId).Select(i => i.InvoiceId).FirstOrDefault();
            return lastID;
        }
        public void AddInvoice(Invoice invoice)
        {
            context.Invoices.Add(invoice);
            context.SaveChanges();
        }
        public void AddInvoiceDetail(InvoiceDetail invoiceDetail)
        {
            context.InvoiceDetails.Add(invoiceDetail);
            context.SaveChanges();
        }
        public void DeleteInvoiceDetail(InvoiceDetail invoiceDetail)
        {
            if (invoiceDetail != null)
            {
                context.InvoiceDetails.Remove(invoiceDetail);
                context.SaveChanges();
            }
        }
        public List<Invoice> GetListOfInvoices()
        {
            return context.Invoices.ToList();
        }
        public List<Product> GetListOfProducts()
        {
            return context.Products.Where(p => !p.IsDeleted).ToList();
        }
        public List<Customer> GetListOfCustomers()
        {
            return context.Customers.Where(c => !c.IsDeleted).ToList();
        }

        public int GetLastInvoideDetailID()
        {
            var lastID = context.InvoiceDetails.OrderByDescending(i => i.Stt).Select(i => i.Stt).FirstOrDefault();
            return (lastID != null && lastID > 0) ? lastID + 1 : 1;
        }
    }
}
