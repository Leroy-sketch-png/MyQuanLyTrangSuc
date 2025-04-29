using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.DataAccess
{
    public class AuthenticationRepository
    {
        private readonly MyQuanLyTrangSucContext context;
        public AuthenticationRepository()
        {
            context = MyQuanLyTrangSucContext.Instance;
        }

        // User Group
        public void AddUserGroup(UserGroup userGroup)
        {
            context.UserGroups.Add(userGroup);
            context.SaveChanges();
        }

        public void UpdateUserGroup(UserGroup userGroup)
        {
            context.SaveChanges();
        }

        public void DeletedUserGroup(UserGroup userGroup)
        {
            if (userGroup != null)
            {
                userGroup.IsDeleted = true;
                context.SaveChanges();
            }
        }

        public UserGroup ExistsUserGroup(string name)
        {
            return context.UserGroups.FirstOrDefault(ug => ug.GroupName == name);
        }

        public List<UserGroup> GetListOfUserGroups()
        {
            return context.UserGroups.Where(ug => !ug.IsDeleted).ToList();
        }

        public List<UserGroup> SearchUserGroupByName(string name)
        {
            return context.UserGroups.Where(ug => ug.GroupName.Contains(name) && !ug.IsDeleted).ToList();
        }
        
        public List<UserGroup> SearchUserGroupByID(string id)
        {
            return context.UserGroups.Where(ug => ug.GroupId.ToString().Contains(id) && !ug.IsDeleted).ToList();
        }
    }
}
