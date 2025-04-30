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
    public class EditPermissionWindowLogic
    {
        private readonly AuthenticationService authenticationService;
        private NotificationWindowLogic notificationWindowLogic;

        public ObservableCollection<UserGroup> UserGroups { get; set; }
        public ObservableCollection<Function> Functions { get; set; }

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


        private Permission _permission;

        public Permission Permission
        {
            get => _permission;
            set
            {
                _permission = value;
                OnPropertyChanged(nameof(Permission));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

        public EditPermissionWindowLogic(Permission permission)
        {
            authenticationService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            Permission = permission;
            var groups = authenticationService.GetListOfUserGroups().ToList();
            UserGroups = new ObservableCollection<UserGroup>(groups);
            SelectedUserGroup = UserGroups.FirstOrDefault(g => g.GroupId == permission.GroupId);
            var functions = authenticationService.GetListOfFunctions().ToList();
            Functions = new ObservableCollection<Function>(functions);
            SelectedFunction = Functions.FirstOrDefault(f => f.FunctionId == permission.FunctionId);
        }

        public bool EditPermission(int selectedValue1, int selectedValue2)
        {
            try
            {
                bool duplicate = authenticationService.GetListOfPermissions().Any(p => p.GroupId == selectedValue1 && p.FunctionId == selectedValue2 && p.PermissionId != Permission.PermissionId);
                if (duplicate)
                {
                    notificationWindowLogic.LoadNotification("Error", "Permission already exists!", "BottomRight");
                    return false;
                }
                Permission.GroupId = selectedValue1;
                Permission.FunctionId = selectedValue2;

                bool updated = authenticationService.UpdatePermission(Permission);
                if (!updated)
                {
                    notificationWindowLogic.LoadNotification("Error", "Fail to update permission!", "BottomRight");
                    return false;
                }

                notificationWindowLogic.LoadNotification("Success", "Update permission successfully", "BottomRight");
                return true;
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", "An error occurred while updating permission: " + ex.Message, "BottomRight");
                return false;
            }
        }


    }
}
