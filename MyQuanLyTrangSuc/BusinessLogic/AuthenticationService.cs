using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class AuthenticationService
    {
        private AuthenticationRepository authenticationRepository;

        public event Action<UserGroup> OnUserGroupAdded; 
        public event Action<UserGroup> OnUserGroupUpdated; 

        // singleton
        private static AuthenticationService _instance;
        public static AuthenticationService Instance => _instance ??= new AuthenticationService();

        public AuthenticationService()
        {
            authenticationRepository = new AuthenticationRepository();
        }

        // User Group
        public List<UserGroup> GetListOfUserGroups()
        {
            return authenticationRepository.GetListOfUserGroups();
        }


        // add
        public string AddUserGroup(UserGroup userGroup)
        {
            UserGroup exists = authenticationRepository.ExistsUserGroup(userGroup.GroupName);
            if (exists != null)
            {
                
                if (exists.IsDeleted == true)
                {
                    exists.IsDeleted = false;
                    authenticationRepository.UpdateUserGroup(exists);
                    OnUserGroupUpdated?.Invoke(exists);
                    return "User group reactivated successfully!";
                }
                return "User group already exists!";
            }
            authenticationRepository.AddUserGroup(userGroup);
            OnUserGroupAdded?.Invoke(userGroup);
            return "User group added successfully!";
        }

        // edit
        public bool UpdateUserGroup(UserGroup userGroup)
        {
            var temp = authenticationRepository.ExistsUserGroup(userGroup.GroupName);
            if (temp != null)
                return false;
            authenticationRepository.UpdateUserGroup(userGroup);
            return true;
        }

        // delete
        public void DeleteUserGroup(UserGroup userGroup)
        {
            authenticationRepository.DeletedUserGroup(userGroup);
        }

    }
}
