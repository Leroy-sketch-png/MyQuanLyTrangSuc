using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    class NotificationWindowLogic
    {
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private Queue<NotificationWindow> notificationQueue = new Queue<NotificationWindow>();
        private bool isNotificationVisible = false;
        public void 
            ForItem()
        {
            var lowItems = context.Products.Where(item => item.Quantity <= 10 && item != null ).ToList();
            //&& item.isdeleted_item == false
            foreach (var lowItem in lowItems)
            {
                NotificationWindow notification = new NotificationWindow(
                    "Notification",
                    lowItem.Name + " is low on stock",
                    "/Resources/bell_icon.png",
                    WpfApplication.Current.Resources["YellowGradient"] as LinearGradientBrush,
                    HextoSolidBrush("#E7BC06"),
                    "BottomRight"
                );
                notificationQueue.Enqueue(notification);
            }
            if (!isNotificationVisible)
                ShowNextNotification();
        }
        public void LoadNotification(string type, string message, string position)
        {
            string path = "";
            string linear = "";
            string brush = "";
            switch (type)
            {
                case "Notification":
                    path = "/Resources/bell_icon.png";
                    linear = "YellowGradient";
                    brush = "#E7BC06";
                    break;
                case "Error":
                    path = "/Resources/Error_Icon.png";
                    linear = "RedGradient";
                    brush = "#F24A50";
                    break;
                case "Success":
                    path = "/Resources/success_icon.png";
                    linear = "GreenGradient";
                    brush = "#36AE3B";
                    break;
            }
            NotificationWindow notification = new NotificationWindow(type, message, path, WpfApplication.Current.Resources[linear] as LinearGradientBrush, HextoSolidBrush(brush), position);
            notification.Show();
        }
        private void ShowNextNotification()
        {
            if (notificationQueue.Count > 0)
            {
                isNotificationVisible = true;

                var notification = notificationQueue.Dequeue();

                notification.Closed += (s, e) =>
                {
                    isNotificationVisible = false;

                    ShowNextNotification();
                };

                notification.Show();
            }
        }
        private SolidColorBrush HextoSolidBrush(string Hex)
        {
            return new SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString(Hex));
        }
    }
}

