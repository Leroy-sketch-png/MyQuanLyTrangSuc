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
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddAccountWindowLogic
    {
        
        private readonly AuthenticationService accountService;
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

        public AddAccountWindowLogic()
        {
            this.accountService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            var groups = accountService.GetListOfUserGroups().ToList();
            UserGroups = new ObservableCollection<UserGroup>(groups);
        }

        public bool AddAccount(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || SelectedUserGroup == null)
            {
                notificationWindowLogic.LoadNotification("Error", "All fields are required!", "BottomRight");
                return false;
            }
            Account account = new Account
            {
                Username = username,
                Password = password,
                GroupId = selectedUserGroup.GroupId,
                IsDeleted = false
            };
            string message = accountService.AddAccount(account);
            notificationWindowLogic.LoadNotification("Success", message, "BottomRight");
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
