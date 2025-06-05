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
    public class EditUnitWindowLogic
    {
        private readonly UnitService unitService;
        private NotificationWindowLogic notificationWindowLogic;

        //bind to view
        private Unit _originalUnit;
        private Unit _editedUnit;
        public Unit EditedUnit
        {
            get => _editedUnit;
            set
            {
                _editedUnit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public EditUnitWindowLogic(Unit unit)
        {
            unitService = UnitService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            _originalUnit = unit;
            _editedUnit = new Unit
            {
                UnitId = unit.UnitId,
                UnitName = unit.UnitName,
                IsNotMarketable = unit.IsNotMarketable
            };
        }

        //edit unit logic
        public bool EditUnit()
        {
            if (_editedUnit == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Unit is not found!", "BottomRight");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_editedUnit.UnitName))
            {
                notificationWindowLogic.LoadNotification("Error", "Unit name can not be empty", "BottomRight");
                return false;
            }

            try
            {
                unitService.EditUnit(_editedUnit);
                notificationWindowLogic.LoadNotification("Success", "Update unit successfully!", "BottomRight");
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
