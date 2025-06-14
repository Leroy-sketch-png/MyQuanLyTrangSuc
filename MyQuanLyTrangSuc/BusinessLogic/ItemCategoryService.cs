using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class ItemCategoryService
    {
        private ItemCategoryRepository itemCategoryRepository;
        private readonly string prefix = "LSP";
        public event Action<ProductCategory> OnItemCategoryAdded; //add or update
        public event Action<ProductCategory> OnItemCategoryUpdated; //edit

        //singleton

        private static ItemCategoryService _instance;
        public static ItemCategoryService Instance => _instance ??= new ItemCategoryService();

        public ItemCategoryService()
        {
            itemCategoryRepository = new ItemCategoryRepository();
        }

        public string GenerateNewItemCategoryID()
        {
            string lastID = itemCategoryRepository.GetLastItemCategoryID();
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
        public bool IsValidName(string name)
        {
            // Kiểm tra null/empty và không chứa số
            if (string.IsNullOrWhiteSpace(name) || Regex.IsMatch(name, @"\d"))
                return false;

            try
            {
                var repository = new ItemCategoryRepository();
                var existingCategories = repository.GetListOfItemCategories(); // hoặc method tương tự

                // Trả về false nếu tên đã tồn tại (không phân biệt hoa thường)
                return !existingCategories.Any(c => c.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false; // Lỗi thì coi như không hợp lệ
            }
        }
        public bool IsValidProfitPercentage(string profitPercentage)
        {
            if (string.IsNullOrWhiteSpace(profitPercentage)) return false;

            if (!int.TryParse(profitPercentage, out int parsedPercentage))
            {
                return false;
            }
            return parsedPercentage >= 0 && parsedPercentage <= 100;
        }
        public bool IsValidUnitID(string unitID)
        {
            if (string.IsNullOrWhiteSpace(unitID))
            {
                return false;
            }
            try
            {
                var temp = new UnitRepository();
                return temp.GetListOfUnits().Any(u => u.UnitId == unitID);
            }
            catch
            {
                return false; // Tránh crash nếu database lỗi
            }
        }

        public bool IsValidItemCategoryData(string name, string unitID, string profitPercentage)
        {
            return IsValidProfitPercentage(profitPercentage) && IsValidName(name) && IsValidUnitID(unitID);
        }

        //Get list of item categories
        public List<ProductCategory> GetListOfItemCategories()
        {
            return itemCategoryRepository.GetListOfItemCategories();
        }

        //Add new item category
        public string AddOrUpdateItemCategory(string name, string unitID, string profitPercentage)
        {
            var itemCategory = itemCategoryRepository.GetItemCategoryByDetails(name);
            if (itemCategory != null)
            {
                if (itemCategory.IsNotMarketable)
                {
                    itemCategory.IsNotMarketable = false;
                    itemCategory.CategoryName = name;
                    itemCategory.UnitId = unitID;
                    itemCategory.ProfitPercentage = int.Parse(profitPercentage);
                    itemCategoryRepository.UpdateItemCategory(itemCategory);
                    return "Restored item category successfully!";
                }
                return "Item category already exists!";
            }

            ProductCategory newItemCategory = new ProductCategory()
            {
                CategoryId = GenerateNewItemCategoryID(),
                CategoryName = name,
                UnitId = unitID,
                ProfitPercentage = int.Parse(profitPercentage),
                IsNotMarketable = false
            };

            itemCategoryRepository.AddItemCategory(newItemCategory);
            OnItemCategoryAdded?.Invoke(newItemCategory);
            return "Added item category successfully!";
        }

        //edit item category
        public void EditItemCategory(ProductCategory itemCategory)
        {
            List<ProductCategory> productCategories = itemCategoryRepository.GetListOfItemCategories();
            bool isDuplicate = productCategories.Any(i => i.CategoryName == itemCategory.CategoryName && i.CategoryId != itemCategory.CategoryId);
            if (isDuplicate)
            {
                throw new InvalidOperationException("Item category already exists.");
            }
            var temp = itemCategoryRepository.GetItemCategoryByID(itemCategory.CategoryId);
            if (temp != null)
            {
                itemCategoryRepository.UpdateItemCategory(itemCategory);
                OnItemCategoryUpdated?.Invoke(itemCategory);
            }
            else
            {
                throw new InvalidOperationException("Unit not found.");
            }
        }

        //delete item category
        public void DeleteItemCategory(ProductCategory selectedItem)
        {
            var temp = itemCategoryRepository.GetItemCategoryByID(selectedItem.CategoryId);
            if (temp != null)
            {
                itemCategoryRepository.DeleteItemCategory(temp);
            }
        }

        //search options
        public List<ProductCategory> ItemCategoriesSearchByName(string name)
        {
            return itemCategoryRepository.SearchItemCategoryByName(name);
        }

        public List<ProductCategory> ItemCategoriesSearchByID(string id)
        {
            return itemCategoryRepository.SearchItemCategoryByID(id);
        }
    }
}
