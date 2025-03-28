using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditItemCategoryWindowLogic
    {
        private readonly ItemCategoryService itemCategoryService;
        private NotificationWindowLogic notificationWindowLogic;

        private ProductCategory _itemCategory;
        public ProductCategory ItemCategory
        {
            get { return _itemCategory; }
            set
            {
                _itemCategory = value;
                OnPropertyChanged(nameof(ItemCategory));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //constructor
        public EditItemCategoryWindowLogic(ProductCategory itemCategory)
        {
            itemCategoryService = ItemCategoryService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            ItemCategory = itemCategory;
        }

        //edit function
        public bool EditItemCategory()
        {
            if (_itemCategory == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Item category is not found!", "BottomRight");
                return false;
            }
            if (!itemCategoryService.IsValidItemCategoryData(ItemCategory.CategoryName,ItemCategory.UnitId, ItemCategory.ProfitPercentage.ToString()))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid item category data!", "BottomRight");
                return false;
            }
            itemCategoryService.EditItemCategory(_itemCategory);
            notificationWindowLogic.LoadNotification("Success", "Update item category successfully", "BottomRight");
            return true;
        }

    }
}
