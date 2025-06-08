using Microsoft.EntityFrameworkCore;
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
            return context.Invoices.Where(i => !i.IsDeleted).ToList();
        }
        public List<Product> GetListOfProducts()
        {
            return context.Products.Where(p => !p.IsDeleted).ToList();
        }
        public List<Customer> GetListOfCustomers()
        {
            return context.Customers.Where(c => !c.IsDeleted).ToList();
        }

        public int GetLastInvoiceDetailID()
        {
            var lastID = context.InvoiceDetails.OrderByDescending(i => i.Stt).Select(i => i.Stt).FirstOrDefault();
            return (lastID != null && lastID > 0) ? lastID + 1 : 1;
        }

        public void RemoveInvoiceDetail(string invoiceId, string productId)
        {
            var detailToRemove = context.InvoiceDetails
                                        .FirstOrDefault(id => id.InvoiceId == invoiceId && id.ProductId == productId);
            if (detailToRemove != null)
            {
                context.InvoiceDetails.Remove(detailToRemove);
                context.SaveChanges();
            }
        }

        public void UpdateInvoiceDetail(InvoiceDetail currentDetail)
        {
            var existingDetail = context.InvoiceDetails.FirstOrDefault(id => id.InvoiceId == currentDetail.InvoiceId && id.ProductId == currentDetail.ProductId);

            if (existingDetail != null)
            {
                existingDetail.Quantity = currentDetail.Quantity;
                existingDetail.Price = currentDetail.Price;
                existingDetail.TotalPrice = currentDetail.TotalPrice;

                context.InvoiceDetails.Update(existingDetail);
                context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException($"Can not find the invoice detail for record with detail ID: '{currentDetail.InvoiceId}' and product ID: '{currentDetail.ProductId}' to update.");
            }
        }

        public void UpdateInvoice(Invoice invoice)
        {
            var existingInvoice = context.Invoices.Find(invoice.InvoiceId);
            if (existingInvoice != null)
            {
                existingInvoice.CustomerId = invoice.CustomerId;
                existingInvoice.Date = invoice.Date;
                existingInvoice.TotalAmount = invoice.TotalAmount;
                context.Invoices.Update(existingInvoice);
                context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException($"Can not find the invoice record with ID: {invoice.InvoiceId} to update.");
            }
        }

        public IEnumerable<InvoiceDetail> GetInvoiceDetailsByInvoiceId(string invoiceId)
        {
            return context.InvoiceDetails
                          .Where(id => id.InvoiceId == invoiceId)
                          .Include(id => id.Product)
                          .AsNoTracking();
        }

        public void DeleteInvoice(Invoice selectedItem)
        {
            if (selectedItem != null)
            {
                foreach (InvoiceDetail invoiceDetail in GetInvoiceDetailsByInvoiceId(selectedItem.InvoiceId))
                {
                    invoiceDetail.Product.Quantity += invoiceDetail.Quantity;
                }
                selectedItem.IsDeleted = true;
                context.SaveChanges();
            }
        }
    }
}
