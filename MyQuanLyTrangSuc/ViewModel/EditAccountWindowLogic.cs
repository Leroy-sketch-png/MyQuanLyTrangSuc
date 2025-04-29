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
    public class EditAccountWindowLogic: INotifyPropertyChanged
    {
        private readonly AuthenticationService authenticationService;
        private NotificationWindowLogic notificationWindowLogic;

        public ObservableCollection<UserGroup> UserGroups { get; set; }

        private UserGroup selectedUserGroup;
        public UserGroup SelectedUserGroup
        {
            get => selectedUserGroup;
            set
            {
                if (selectedUserGroup != value)
                {
                    selectedUserGroup = value;
                    OnPropertyChanged(); 
                }
            }
        }

        private Account _account;

        public Account Account
        {
            get => _account;
            set
            {
                _account = value;
                OnPropertyChanged(nameof(Account));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditAccountWindowLogic(Account account)
        {
            authenticationService = AuthenticationService.Instance;
            notificationWindowLogic = new NotificationWindowLogic();
            Account = account;
            var groups = authenticationService.GetListOfUserGroups().ToList();
            UserGroups = new ObservableCollection<UserGroup>(groups);
            SelectedUserGroup = UserGroups.FirstOrDefault(g => g.GroupId == Account.GroupId);
        }

        public bool EditAccount()
        {
            if (_account == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Account is not found!", "BottomRight");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Account.Username))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid account name!", "BottomRight");
                return false;
            }
            Account.GroupId = SelectedUserGroup?.GroupId ?? Account.GroupId;

            //MessageBox.Show(SelectedUserGroup.GroupName);
            Account.Group = SelectedUserGroup;
            bool res = authenticationService.UpdateAccount(Account);
            if (res)
                notificationWindowLogic.LoadNotification("Success", "Update account successfully", "BottomRight");
            else
            {
                notificationWindowLogic.LoadNotification("Error", "Account already exists!", "BottomRight");
                return false;
            }
            return true;
        }
    }
}
