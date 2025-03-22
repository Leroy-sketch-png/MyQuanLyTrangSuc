using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class SupplierRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public SupplierRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }
        public string GetLastSupplierID()
        {
            var lastID = context.Suppliers.OrderByDescending(s => s.SupplierId).Select(s => s.SupplierId).FirstOrDefault();
            return lastID;
        }
        public Supplier GetSupplierByDetails(string name, string email, string phone)
        {
            return context.Suppliers.FirstOrDefault(s => s.Name == name && s.Email == email && s.ContactNumber == phone);
        }
        public void AddSupplier(Supplier supplier)
        {
            context.Suppliers.Add(supplier);
            context.SaveChangesAdded(supplier);
        }
        public void UpdateSupplier(Supplier supplier)
        {
            context.SaveChangesAdded(supplier);
        }
    }
}
