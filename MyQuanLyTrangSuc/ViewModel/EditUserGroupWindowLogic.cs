using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class EditUserGroupWindowLogic
    {
        private readonly AuthenticationService authenticationService;
        private NotificationWindowLogic notificationWindowLogic;

        private UserGroup _userGroup;

        public UserGroup UserGroup
        {
            get => _userGroup;
            set
            {
                _userGroup = value;
                OnPropertyChanged(nameof(UserGroup));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditUserGroupWindowLogic(UserGroup userGroup)
        {
            authenticationService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            UserGroup = userGroup;
        }

        public bool EditUserGroup()
        {
            if (_userGroup == null)
            {
                notificationWindowLogic.LoadNotification("Error", "User group is not found!", "BottomRight");
                return false;
            }
            if (string.IsNullOrWhiteSpace(UserGroup.GroupName))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid user group name!", "BottomRight");
                return false;
            }
            bool res = authenticationService.UpdateUserGroup(_userGroup);
            if (res)
                notificationWindowLogic.LoadNotification("Success", "Update user group successfully", "BottomRight");
            else {
                notificationWindowLogic.LoadNotification("Error", "User group already exists!", "BottomRight");
                return false;
            }
            return true;
        }
    }
}
