using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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



        // Account 
        public List<Account> GetListOfAccounts()
        {
            return context.Accounts.Where(a => !a.IsDeleted).ToList();
        }

        public bool ExistsUsername(string username)
        {
            return context.Accounts.Any(a => a.Username.ToLower() == username.ToLower() && !a.IsDeleted);
        }

        public void AddAccount(Account account)
        {
            context.Accounts.Add(account);
            context.SaveChanges();
        }

        public Account GetAccountById(int accountId)
        {
            return context.Accounts.FirstOrDefault(a => a.AccountId == accountId && !a.IsDeleted);
        }

        public void UpdateAccount(Account updated)
        {
            context.SaveChanges();
        }

        public void DeletedAccount(Account account)
        {
            if (account != null)
            {
                account.IsDeleted = true;
                context.SaveChanges();
            }
        }

        public Account GetAccountByUsernameIncludeGroup(string username)
        {
            return context.Accounts
                       .Include(a => a.Group)
                       .FirstOrDefault(a => a.Username == username && !a.IsDeleted); 
        }

        // Permission
        public List<Permission> GetListOfPermissions()
        {
            return context.Permissions.Include(p => p.Function).ToList();
        }

        public List<Function> GetListOfFunctions()
        {
            return context.Functions.ToList();
        }

        public void AddPermission(Permission permission)
        {
            context.Permissions.Add(permission);
            context.SaveChanges();
        }

        public bool UpdatePermission(Permission permission)
        {
            context.Permissions.Update(permission);
            context.SaveChanges();
            return true;
        }

        public Permission GetPermissionById(int permissionId)
        {
            return context.Permissions.FirstOrDefault(a => a.PermissionId == permissionId && !a.IsDeleted);
        }

        public void DeletePermission(Permission selectedItem)
        {
            if (selectedItem != null)
            {
                selectedItem.IsDeleted = true;
                context.SaveChanges();
            }
        }
    }
}
