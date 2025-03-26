using MyQuanLyTrangSuc.BusinessLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddUnitWindowLogic
    {
        private readonly UnitService unitService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddUnitWindowLogic()
        {
            this.unitService = UnitService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewUnitID();
        }

        //binding to textblock
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void GenerateNewUnitID()
        {
            NewID = unitService.GenerateNewUnitID();
        }

        //add new unit
        public void AddUnit(string name)
        {
            string resultMessage = unitService.AddUnit(name);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");
            GenerateNewUnitID();
        }
    }
}
