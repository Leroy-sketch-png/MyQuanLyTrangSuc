using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditItemCategoryWindowLogic: INotifyPropertyChanged
    {
        private readonly ItemCategoryService itemCategoryService;
        private readonly UnitService unitService;
        private NotificationWindowLogic notificationWindowLogic;

        private ProductCategory _originalItemCategory;

        private ProductCategory _editedItemCategory;
        public ProductCategory EditedItemCategory
        {
            get { return _editedItemCategory; }
            set
            {
                _editedItemCategory = value;
                OnPropertyChanged(nameof(EditedItemCategory));
            }
        }

        private ObservableCollection<Unit> _listOfUnits;
        public ObservableCollection<Unit> ListOfUnits
        {
            get { return _listOfUnits; }
            set { _listOfUnits = value; OnPropertyChanged(nameof(ListOfUnits)); }
        }

        private Unit _selectedUnit;
        public Unit SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;
                OnPropertyChanged(nameof(SelectedUnit));
            }
        }

        private double _profitPercentage;
        public double ProfitPercentage
        {
            get => _profitPercentage;
            set
            {
                if (value >= 0 && value <= 100)
                {
                    _profitPercentage = value;
                    OnPropertyChanged(nameof(ProfitPercentage));
                }
            }
        }

        // Implement IDataErrorInfo
        public string Error => null;
        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(ProfitPercentage))
                {
                    if (ProfitPercentage < 0 || ProfitPercentage > 100)
                        notificationWindowLogic.LoadNotification("Error", "Giá trị không phù hợp!", "BottomRight");
                    return "Giá trị không được âm hoặc lớn hơn 100!";
                }
                return null;
            }
        }

        public void LoadInitialData()
        {
            ListOfUnits = new ObservableCollection<Unit>(unitService.GetListOfUnits());
            SelectedUnit = ListOfUnits.FirstOrDefault(u => u.UnitId == _editedItemCategory.UnitId && !u.IsNotMarketable);
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
            unitService = UnitService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            _originalItemCategory = itemCategory;
            _editedItemCategory = new ProductCategory
            {
                CategoryId = itemCategory.CategoryId,
                CategoryName = itemCategory.CategoryName,
                UnitId = itemCategory.UnitId,
                ProfitPercentage = itemCategory.ProfitPercentage
            };
            LoadInitialData();
        }

        //edit function
        public bool EditItemCategory()
        {
            if (_editedItemCategory == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Item category is not found!", "BottomRight");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_editedItemCategory.CategoryName))
            {
                notificationWindowLogic.LoadNotification("Error", "Item category name can not be empty", "BottomRight");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_editedItemCategory.UnitId))
            {
                notificationWindowLogic.LoadNotification("Error", "Unit ID can not be empty", "BottomRight");
                return false;
            }

            if (!itemCategoryService.IsValidProfitPercentage(_editedItemCategory.ProfitPercentage.ToString()))
            {
                notificationWindowLogic.LoadNotification("Error", "Profit percentage must be a valid number", "BottomRight");
                return false;
            }

            try
            {
                _editedItemCategory.UnitId = SelectedUnit.UnitId;
                itemCategoryService.EditItemCategory(_editedItemCategory);
                notificationWindowLogic.LoadNotification("Success", "Item category updated successfully!", "BottomRight");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                notificationWindowLogic.LoadNotification("Error", ex.Message, "BottomRight");
                return false;
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", "An unexpected error occurred: " + ex.Message, "BottomRight");
                return false;
            }
        }

    }
}
