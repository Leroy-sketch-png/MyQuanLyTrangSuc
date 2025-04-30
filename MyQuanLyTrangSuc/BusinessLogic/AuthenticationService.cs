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
        public event Action<Account> OnAccountAdded;
        public event Action<Account> OnAccountUpdated;

        // singleton
        private static AuthenticationService _instance;
        public static AuthenticationService Instance => _instance ??= new AuthenticationService();

        public AuthenticationService()
        {
            authenticationRepository = new AuthenticationRepository();
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
            account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);

            bool exists = authenticationRepository.ExistsUsername(account.Username);

            if (exists)
            {
                return "Username already exists!";
            }
            if (!string.IsNullOrEmpty(account.Password))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(account.Password);
            }
            else
            {
                return "Password cannot be empty!";
            }
            authenticationRepository.AddAccount(account);
            OnAccountAdded?.Invoke(account);
            return "Account created successfully!";

        }

        public bool ValidateLogin(string username, string plainPassword)
        {
            var acc = authenticationRepository.GetListOfAccounts().FirstOrDefault(a => a.Username == username && !a.IsDeleted);
            if (acc == null)
            {
                return false;
            }
            return BCrypt.Net.BCrypt.Verify(plainPassword, acc.Password);
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
    }
}
