using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.DataAccess;
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
    public class AddItemCategoryWindowLogic: INotifyPropertyChanged
    {
        private readonly UnitService unitService;
        private readonly ItemCategoryService itemCategoryService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddItemCategoryWindowLogic()
        {
            itemCategoryService = ItemCategoryService.Instance;
            unitService = UnitService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            LoadInitialData();
        }

        private string _newID;
        public string NewID
        {
            get => _newID;
            private set
            {
                _newID = value;
                OnPropertyChanged(nameof(NewID));
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

        

        public void LoadInitialData()
        {
            NewID = itemCategoryService.GenerateNewItemCategoryID();
            ListOfUnits = new ObservableCollection<Unit>(unitService.GetListOfUnits());
        }
        public void RefreshListOfUnits()
        {
            var unitRepo = new UnitRepository();
            ListOfUnits.Clear();
            foreach (var unit in unitRepo.GetListOfUnits())
            {
                ListOfUnits.Add(unit);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool AddItemCategory(string name, string profitPercentage)
        {
            if (_selectedUnit == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Please select a unit!", "BottomRight");
                return false;
            }
            if (!itemCategoryService.IsValidItemCategoryData(name, _selectedUnit.UnitId, profitPercentage))
            {
                notificationWindowLogic.LoadNotification("Error", "Unit Id không phù hợp!", "BottomRight");
                return false;
            }

            string resultMessage = itemCategoryService.AddOrUpdateItemCategory(name, _selectedUnit.UnitId, profitPercentage);
            if (resultMessage == "Item category already exists!")
            {
                notificationWindowLogic.LoadNotification("Error", resultMessage, "BottomRight");
                return false;
            }
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");
            LoadInitialData();
            return true;
        }
    }
}
