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
    public class AddItemCategoryWindowLogic
    {
        private readonly ItemCategoryService itemCategoryService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddItemCategoryWindowLogic()
        {
            itemCategoryService = ItemCategoryService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewItemCategoryID();
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

        public void GenerateNewItemCategoryID()
        {
            NewID = itemCategoryService.GenerateNewItemCategoryID();
            var unitRepo = new UnitRepository();
            ListOfUnits = new ObservableCollection<Unit>(unitRepo.GetListOfUnits());
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
            if (!itemCategoryService.IsValidItemCategoryData(name, _selectedUnit.UnitId, profitPercentage))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid item category data!", "BottomRight");
                return false;
            }

            string resultMessage = itemCategoryService.AddOrUpdateItemCategory(name, _selectedUnit.UnitId, profitPercentage);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewItemCategoryID();
            return true;
        }
    }
}
