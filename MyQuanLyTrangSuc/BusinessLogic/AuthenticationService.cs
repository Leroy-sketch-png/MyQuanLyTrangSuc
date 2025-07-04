﻿using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security;
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
        public event Action<Account> OnAccountAdded;
        public event Action<Account> OnAccountUpdated;
        public event Action<Permission> OnPermissionAdded;
        public event Action<Permission> OnPermissionUpdated;

        // singleton
        private static AuthenticationService _instance;
        public static AuthenticationService Instance => _instance ??= new AuthenticationService();

        private AuthenticationService()
        {
            authenticationRepository = new AuthenticationRepository();
        }

        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // User Group

        //read
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

        // Account

        // read
        public List<Account> GetListOfAccounts()
        {
            return authenticationRepository.GetListOfAccounts();
        }

        // add
        public string AddAccount(Account account)
        {

            bool exists = authenticationRepository.ExistsUsername(account.Username);

            if (exists)
            {
                return "Username already exists!";
            }
            if (string.IsNullOrEmpty(account.Password))
            {
                return "Password cannot be empty!";
            }
            String hash = BCrypt.Net.BCrypt.HashPassword(account.Password);
            account.Password = hash;
            authenticationRepository.AddAccount(account);
            OnAccountAdded?.Invoke(account);
            return "Account created successfully!";

        }

        public bool ValidateLogin(string username, string plainPassword)
        {
            List<Account> accounts = authenticationRepository.GetListOfAccounts();
            var acc = authenticationRepository.GetAccountByUsername(username);
            if (acc == null)
            {
                return false;
            }
            //bool res = plainPassword.Equals(acc.Password);
            bool res = BCrypt.Net.BCrypt.Verify(plainPassword, acc.Password);
            return res;
        }

        public bool UpdateAccount(Account account)
        {
            var existing = authenticationRepository.GetAccountById(account.AccountId);
            if (existing == null)
                return false;

            bool usernameTaken = authenticationRepository.GetListOfAccounts().Any(a => a.Username.Equals(account.Username, StringComparison.OrdinalIgnoreCase) && a.AccountId != account.AccountId && !a.IsDeleted);

            if (usernameTaken)
                return false;

            existing.Username = account.Username;

            if (!string.IsNullOrWhiteSpace(account.Password))
                existing.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);

            existing.GroupId = account.GroupId;
            existing.Group = account.Group;


            authenticationRepository.UpdateAccount(existing);


            OnAccountUpdated?.Invoke(existing);

            return true;
        }

        // delete 
        public void DeleteAccount(Account account)
        {
            authenticationRepository.DeletedAccount(account);
        }

        public Account GetAccountWithGroupByUsername(string username)
        {
            return authenticationRepository.GetAccountByUsernameIncludeGroup(username);
        }

        // Permission
        public List<Permission> GetListOfPermissions()
        {
            return authenticationRepository.GetListOfPermissions();
        }

        public List<Function> GetListOfFunctions()
        {
            return authenticationRepository.GetListOfFunctions();

        }

        public bool AddPermission(int selectedValue1, int selectedValue2)
        {
            var permission = authenticationRepository.GetListOfPermissions().FirstOrDefault(p => p.GroupId == selectedValue1 && p.FunctionId == selectedValue2);
            if (permission != null)
            {
                if (permission.IsDeleted)
                {
                    CurrentUserPrincipal.AddPermission(permission.Function.ScreenToLoad);
                    permission.IsDeleted = false;
                    return true;
                }
                return false;
            }
            else
            {
                var newPermission = new Permission
                {
                    GroupId = selectedValue1,
                    FunctionId = selectedValue2,
                    IsDeleted = false
                };
                authenticationRepository.AddPermission(newPermission);
                CurrentUserPrincipal.AddPermission(newPermission.Function.ScreenToLoad);
                OnPermissionAdded?.Invoke(newPermission);
                return true;
            }
        }

        public bool UpdatePermission(Permission permission)
        {
            var existing = authenticationRepository.GetPermissionById(permission.PermissionId);
            if (existing == null)
                return false;
            existing.GroupId = permission.GroupId;
            existing.FunctionId = permission.FunctionId;
            authenticationRepository.UpdatePermission(existing);
            OnPermissionUpdated?.Invoke(existing);
            return true;

        }

        public void DeletePermission(Permission selectedItem)
        {
            authenticationRepository.DeletePermission(selectedItem);
            CurrentUserPrincipal.RemovePermission(selectedItem.Function.ScreenToLoad);
        }
    }
}
