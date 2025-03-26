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
        private Unit _unit;
        public Unit Unit
        {
            get => _unit;
            set
            {
                _unit = value;
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
            Unit = unit;
        }

        //edit unit logic
        public bool EditUnit()
        {
            if (_unit == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Unit is not found!", "BottomRight");
                return false;
            }

            unitService.EditUnit(_unit);
            notificationWindowLogic.LoadNotification("Success", "Update unit successfully", "BottomRight");
            return true;
        }
    }
}
