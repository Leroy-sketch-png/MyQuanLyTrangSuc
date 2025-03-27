using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class ItemCategoryRepository
    {
        private readonly MyQuanLyTrangSucContext context;

        public ItemCategoryRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }

        public string GetLastItemCategoryID()
        {
            var lastID = context.ProductCategories.OrderByDescending(s => s.CategoryId).Select(s => s.CategoryId).FirstOrDefault();
            return lastID;
        }

        public ProductCategory GetItemCategoryByDetails(string name)
        {
            return context.ProductCategories.FirstOrDefault(s => s.Categoryname == name);
        }

        public void AddItemCategory(ProductCategory itemCategory)
        {
            context.ProductCategories.Add(itemCategory);
            context.SaveChanges();
        }

        public void UpdateItemCategory(ProductCategory productCategory)
        {
            context.SaveChanges();
        }

        public List<ProductCategory> GetListOfItemCategories()
        {
            return context.ProductCategories.Where(i => !i.IsNotMarketable).ToList();
        }
    }
}
