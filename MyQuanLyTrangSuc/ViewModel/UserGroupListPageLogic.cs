using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View.Windows;
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
    public class UserGroupListPageLogic
    {
        private readonly AuthenticationService userGroupService;
        private ObservableCollection<UserGroup> userGroups;
        public ObservableCollection<UserGroup> UserGroups
        {
            get => userGroups;
            set
            {
                userGroups = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public UserGroupListPageLogic()
        {
            this.userGroupService = AuthenticationService.Instance;
            UserGroups = new ObservableCollection<UserGroup>();
            LoadUserGroupsFromDatabase();
            userGroupService.OnUserGroupAdded += UserGroupService_OnUserGroupAdded;
            userGroupService.OnUserGroupUpdated += UserGroupService_OnUserGroupUpdated;
        }

        private void UserGroupService_OnUserGroupUpdated(UserGroup group)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Trường hợp khôi phục từ IsDeleted => false, thì cần thêm lại vào danh sách
                if (!UserGroups.Any(g => g.GroupId == group.GroupId))
                {
                    UserGroups.Add(group);
                }
                else
                {
                    // Nếu cần cập nhật dữ liệu khác (ví dụ tên), thì cập nhật tại đây
                    var existing = UserGroups.First(g => g.GroupId == group.GroupId);
                    existing.GroupName = group.GroupName;
                    OnPropertyChanged(nameof(UserGroups));
                }
            });
        }

        //catch event for add new user group
        private void UserGroupService_OnUserGroupAdded(UserGroup obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UserGroups.Add(obj);
            });
        }
        // Load data from database
        private void LoadUserGroupsFromDatabase()
        {
            var userGroups = userGroupService.GetListOfUserGroups().Where(s => !s.IsDeleted).ToList();
            UserGroups = new ObservableCollection<UserGroup>(userGroups);
        }

        // add
        public void LoadAddUserGroupWindow()
        {
            var temp = new AddUserGroupWindow();
            temp.ShowDialog();
        }

        // edit
        public void LoadEditUserGroupWindow(UserGroup userGroup)
        {
            var temp = new EditUserGroupWindow(userGroup);
            temp.ShowDialog();
        }

        // delete
        public void DeleteUserGroup(UserGroup userGroup)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this user group?", "Delete User Group", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                userGroupService.DeleteUserGroup(userGroup);
                UserGroups.Remove(userGroup);
            }
        }

        // search
        public void SearchUserGroup(string searchText)
        {
            var filteredGroups = userGroupService.GetListOfUserGroups()
                .Where(g => g.GroupName.Contains(searchText, StringComparison.OrdinalIgnoreCase) && !g.IsDeleted)
                .ToList();
            UpdateUserGroupList(filteredGroups);
        }

        private void UpdateUserGroupList(List<UserGroup> filteredGroups)
        {
            if (!UserGroups.SequenceEqual(filteredGroups))
            {
                UserGroups.Clear();
                foreach (var group in filteredGroups)
                {
                    UserGroups.Add(group);
                }
            }
        }
    }
}
