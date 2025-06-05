using Microsoft.Win32; // Retained for consistency in using statements, though not explicitly used for export now.
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security; // Needed for CustomPrincipal
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.View.Windows; // Needed for EditServiceWindow
using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Linq;
using System.Threading; // Needed for Thread.CurrentPrincipal
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceUserControlLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceUserControl serviceUserControlUI;

        // Constructor fixed to initialize serviceUserControlUI safely
        public ServiceUserControlLogic(ServiceUserControl serviceUserControl)
        {
            serviceUserControlUI = serviceUserControl ?? throw new ArgumentNullException(nameof(serviceUserControl));
        }

        // Property to get the current authenticated user's principal
        public CustomPrincipal CurrentUserPrincipal
        {
            get => Thread.CurrentPrincipal as CustomPrincipal;
        }

        // --- Permission Check Methods ---

        private bool CanRemoveService()
        {
            // Requires permission to delete a service
            // Assumes "DeleteService" is the permission string for removing services
            return CurrentUserPrincipal?.HasPermission("DeleteService") == true;
        }

        private bool CanLoadEditServiceWindow()
        {
            // Requires permission to edit a service
            // Assumes "EditService" is the permission string for opening the edit window
            return CurrentUserPrincipal?.HasPermission("EditService") == true;
        }

        // --- Service Operations ---

        public void RemoveServiceFromDatabase()
        {
            // Check for permission before proceeding
            if (!CanRemoveService())
            {
                MessageBox.Show("You do not have permission to remove services.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var service = serviceUserControlUI.DataContext as Service;
            if (service == null) return;

            var result = MessageBox.Show(
                "Do you want to remove this service?",
                "Remove Service",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                service.IsDeleted = true;
                context.Services.Entry(service).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChangesRemoved(service);
                MessageBox.Show("Service removed successfully.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void LoadEditServiceWindow()
        {
            // Check for permission before proceeding
            if (!CanLoadEditServiceWindow())
            {
                MessageBox.Show("You do not have permission to edit services.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get current Service from DataContext of UserControl
            if (serviceUserControlUI.DataContext is Service service)
            {
                var editWindow = new EditServiceWindow(service);
                editWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Unable to get service information for editing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}