using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class ImportRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public ImportRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }
        public string GetLastImportID()
        {
            var lastID = context.Imports.OrderByDescending(i => i.ImportId).Select(i => i.ImportId).FirstOrDefault();
            return lastID;
        }
        public void AddImport(Import import)
        {
            context.Imports.Add(import);
            context.SaveChanges();
        }
        public void AddImportDetail(ImportDetail importDetail)
        {
            context.ImportDetails.Add(importDetail);
            context.SaveChanges();
        }
        public void DeleteImportDetail(ImportDetail importDetail)
        {
            if (importDetail != null)
            {
                context.ImportDetails.Remove(importDetail);
                context.SaveChanges();
            }
        }
        
        public List<Import> GetListOfImports()
        {
            return context.Imports.ToList();
        }   
        public List<Product> GetListOfProducts()
        {
            return context.Products.Where(p => !p.IsDeleted).ToList();
        }
        public List<Supplier> GetListOfSuppliers()
        {
            return context.Suppliers.Where(s => !s.IsDeleted).ToList();
        }

        public int GetLastImportDetailID()
        {
            var lastID = context.ImportDetails.OrderByDescending(i => i.Stt).Select(i => i.Stt).FirstOrDefault();
            return (lastID != null && lastID > 0) ? lastID + 1 : 1;
        }
    }
}
