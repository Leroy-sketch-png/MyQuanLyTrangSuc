using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddUserGroupWindowLogic
    {
        private readonly AuthenticationService userGroupService;
        private readonly NotificationWindowLogic notificationWindowLogic;

        public AddUserGroupWindowLogic()
        {
            this.userGroupService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
        }

        public bool AddUserGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                notificationWindowLogic.LoadNotification("Error", "Group name cannot be empty!", "BottomRight");
                return false;
            }
            UserGroup userGroup = new UserGroup
            {
                GroupName = name,
                IsDeleted = false
            };
            string message = userGroupService.AddUserGroup(userGroup);
            notificationWindowLogic.LoadNotification("Success", message, "BottomRight");
            return true;
        }
    }
}
