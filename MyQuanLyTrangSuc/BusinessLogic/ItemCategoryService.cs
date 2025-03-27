using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return !string.IsNullOrWhiteSpace(name);
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
            var temp = new UnitRepository();

            return temp.GetListOfUnits().Any(u => u.UnitId == unitID);
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
                    itemCategory.Categoryname = name;
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
                Categoryname = name,
                UnitId = unitID,
                ProfitPercentage = int.Parse(profitPercentage),
                IsNotMarketable = false
            };

            itemCategoryRepository.AddItemCategory(newItemCategory);
            OnItemCategoryAdded?.Invoke(newItemCategory);
            return "Added item category successfully!";
        }

    }
}
