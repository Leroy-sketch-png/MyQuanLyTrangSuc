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

        public List<Supplier> GetListOfSuppliers()
        {
            return context.Suppliers.Where(s => !s.IsDeleted).ToList();
        }

        public Supplier GetSupplierByID(string id)
        {
            return context.Suppliers.Find(id);
        }

        public void DeleteSupplier(Supplier temp)
        {
            if (temp != null)
            {
                temp.IsDeleted = true;
                context.SaveChangesAdded(temp);
            }
        }

        public List<Supplier> SearchSupplierByName(string name)
        {
            return context.Suppliers.Where(s => s.Name.Contains(name) && !s.IsDeleted).ToList();
        }

        public List<Supplier> SearchSupplierByID(string id)
        {
            return context.Suppliers.Where(s => s.SupplierId.Contains(id) && !s.IsDeleted).ToList();
        }

    }
}
