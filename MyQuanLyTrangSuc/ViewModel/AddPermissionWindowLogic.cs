using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddPermissionWindowLogic
    {
        private readonly AuthenticationService permissionService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public ObservableCollection<UserGroup> UserGroups { get; set; }

        private UserGroup selectedUserGroup;
        public UserGroup SelectedUserGroup
        {
            get => selectedUserGroup;
            set
            {
                selectedUserGroup = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Function> Functions { get; set; }

        private Function selectedFunction;
        public Function SelectedFunction
        {
            get => selectedFunction;
            set
            {
                selectedFunction = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool AddPermission(int selectedValue1, int selectedValue2)
        {
            try
            {
                bool created = permissionService.AddPermission(selectedValue1, selectedValue2);
                if (!created)
                {
                    notificationWindowLogic.LoadNotification("Error", "Permission already existed", "BottomRight");
                    return false;
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Success", "Add new permission successfully", "BottomRight");
                    return true;
                }
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", ex.Message, "BottomRight");
                return false;
            }
        }
        public AddPermissionWindowLogic()
        {
            this.permissionService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            var groups = permissionService.GetListOfUserGroups().ToList();
            UserGroups = new ObservableCollection<UserGroup>(groups);
            var functions = permissionService.GetListOfFunctions().ToList();
            Functions = new ObservableCollection<Function>(functions);
        }
    }
}
