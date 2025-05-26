using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<ImportDetail> GetImportDetailsByImportId(string importId)
        {
            return context.ImportDetails.Where(id => id.ImportId == importId);
        }

        public void RemoveImportDetail(string importId, string productId)
        {
            var detailToRemove = context.ImportDetails
                                        .FirstOrDefault(id => id.ImportId == importId && id.ProductId == productId);
            if (detailToRemove != null)
            {
                context.ImportDetails.Remove(detailToRemove);
                context.SaveChanges();
            }
        }

        public void UpdateImport(Import import)
        {
            var existingImport = context.Imports.Find(import.ImportId);
            if (existingImport != null)
            {
                existingImport.SupplierId = import.SupplierId;
                existingImport.Date = import.Date; 
                existingImport.TotalAmount = import.TotalAmount;
                context.Imports.Update(existingImport); 
                context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException($"Không tìm thấy phiếu nhập với ID: {import.ImportId} để cập nhật.");
            }
        }

        public void UpdateImportDetail(ImportDetail currentDetail)
        {
            var existingDetail = context.ImportDetails
                                                    .FirstOrDefault(id => id.ImportId == currentDetail.ImportId && id.ProductId == currentDetail.ProductId);

            if (existingDetail != null)
            {
                existingDetail.Quantity = currentDetail.Quantity;
                existingDetail.Price = currentDetail.Price; 
                existingDetail.TotalPrice = currentDetail.TotalPrice;

                context.ImportDetails.Update(existingDetail);
                context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException($"Không tìm thấy chi tiết nhập hàng cho phiếu nhập '{currentDetail.ImportId}' và sản phẩm '{currentDetail.ProductId}' để cập nhật.");
            }
        }

        
    }
}
