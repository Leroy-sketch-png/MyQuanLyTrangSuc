using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class InvoiceService
    {
        private InvoiceRepository invoiceRepository;
        private ItemRepository itemRepository;
        private readonly string prefix = "INV";
        public event Action<Invoice> OnInvoiceAdded;
        public event Action<Invoice> OnInvoiceUpdated;

        //singleton
        private static InvoiceService _instance;
        public static InvoiceService Instance => _instance ??= new InvoiceService();

        public InvoiceService()
        {
            invoiceRepository = new InvoiceRepository();
            itemRepository = new ItemRepository();
        }

        public string GenerateNewInvoiceID()
        {
            string lastID = invoiceRepository.GetLastInvoiceID();
            int newNumber = 1;
            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix))
            {
                string numericPart = lastID.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }
            return $"{prefix}{newNumber:D3}";
        }
        public List<Product> GetListOfProducts()
        {
            return invoiceRepository.GetListOfProducts();
        }
        public List<Customer> GetListOfCustomers()
        {
            return invoiceRepository.GetListOfCustomers();
        }
        public void AddInvoiceDetail(InvoiceDetail invoiceDetail)
        {
            invoiceRepository.AddInvoiceDetail(invoiceDetail);
        }
        public void AddInvoice(Invoice invoice)
        {
            invoiceRepository.AddInvoice(invoice);
            OnInvoiceAdded?.Invoke(invoice);
        }
        public int GenerateNewInvoiceDetailID()
        {
            return invoiceRepository.GetLastInvoiceDetailID();
        }

        public void UpdateProductQuantity(string productId, int quantity, bool isAddition = false)
        {
            itemRepository.UpdateProductQuantity(productId, quantity, isAddition);
        }

        public IEnumerable<InvoiceDetail> GetInvoiceDetailsByInvoiceId(string invoiceId)
        {
            return invoiceRepository.GetInvoiceDetailsByInvoiceId(invoiceId);
        }

        public void RemoveInvoiceDetail(string invoiceId, string productId)
        {
            invoiceRepository.RemoveInvoiceDetail(invoiceId, productId);
        }

        public void UpdateInvoiceDetail(InvoiceDetail originalDetail)
        {
            invoiceRepository.UpdateInvoiceDetail(originalDetail);
        }

        public void UpdateInvoice(Invoice invoice)
        {
            invoiceRepository.UpdateInvoice(invoice);
            OnInvoiceUpdated?.Invoke(invoice);
        }

        public void DeleteInvoice(Invoice selectedItem)
        {
            invoiceRepository.DeleteInvoice(selectedItem);
        }
    }
}
