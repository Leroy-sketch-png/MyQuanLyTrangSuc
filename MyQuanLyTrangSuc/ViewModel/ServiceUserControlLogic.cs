using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceUserControlLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly ServiceUserControl serviceUserControlUI;

        public ServiceUserControlLogic(ServiceUserControl serviceUserControl)
        {
            serviceUserControlUI = serviceUserControl;
        }

        public void RemoveServiceFromDatabase()
        {
            var service = serviceUserControlUI.DataContext as Service;
            if (service == null) return;

            var result = MessageBox.Show(
                "Do you want to remove this service?",
                "Remove?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                service.IsDeleted = true;
                context.Entry(service).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChanges();
                MessageBox.Show("Service removed successfully.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}