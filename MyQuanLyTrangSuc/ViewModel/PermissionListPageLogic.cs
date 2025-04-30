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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static OfficeOpenXml.ExcelErrorValue;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class PermissionListPageLogic
    {
        private readonly AuthenticationService permissionService;

        private ObservableCollection<Permission> permissions;

        public ObservableCollection<Permission> Permissions
        {
            get => permissions;
            set
            {
                permissions = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public PermissionListPageLogic()
        {
            this.permissionService = AuthenticationService.Instance;
            Permissions = new ObservableCollection<Permission>();
            LoadPermissionsFromDatabase();
            permissionService.OnPermissionAdded += PermissionService_OnPermissionAdded;
            permissionService.OnPermissionUpdated += PermissionService_OnPermissionUpdated;
        }

        private void PermissionService_OnPermissionUpdated(Permission updated)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Permissions.Any(a => a.PermissionId == updated.PermissionId))
                {
                    Permissions.Add(updated);
                }
                else
                {
                    var existing = Permissions.First(a => a.PermissionId == updated.PermissionId);
                    existing.GroupId = updated.GroupId;
                    existing.FunctionId = updated.FunctionId;
                    existing.IsDeleted = updated.IsDeleted;
                    existing.Group = updated.Group;
                    existing.Function = updated.Function;
                    OnPropertyChanged(nameof(Permissions));
                }
            });
        }

        private void PermissionService_OnPermissionAdded(Permission permission)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Permissions.Add(permission);
            });
        }

        // Load data from database
        private void LoadPermissionsFromDatabase()
        {
            var permissions = permissionService.GetListOfPermissions().ToList();
            Permissions = new ObservableCollection<Permission>(permissions);
        }

        // add
        public void LoadAddPermissionWindow()
        {
            var temp = new AddPermissionWindow();
            temp.ShowDialog();
        }

        // edit
        public void LoadEditPermissionWindow(Permission permission)
        {
            var temp = new EditPermissionWindow(permission);
            temp.ShowDialog();
        }

        // delete
        public void DeletePermission(Permission selectedItem)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this permission?", "Delete Permission", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                permissionService.DeletePermission(selectedItem);
                Permissions.Remove(selectedItem);
            }
        }

        // search
        public void PermissionsSearchByGroupName(string text)
        {
            var filteredPermissions = permissionService.GetListOfPermissions()
                            .Where(p => p.Group.GroupName.Contains(text, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted)
                            .ToList();
            UpdatePermissionList(filteredPermissions);
        }

        public void PermissionsSearchByFunctionName(string text)
        {
            var filteredPermissions = permissionService.GetListOfPermissions()
                .Where(p => p.Function.FunctionName.Contains(text, StringComparison.OrdinalIgnoreCase) && !p.IsDeleted)
                .ToList();
            UpdatePermissionList(filteredPermissions);
        }

        private void UpdatePermissionList(List<Permission> filteredPermissions)
        {
            if (!Permissions.SequenceEqual(filteredPermissions))
            {
                Permissions.Clear();
                foreach (var perm in filteredPermissions)
                {
                    Permissions.Add(perm);
                }
            }
        }
    }
}
