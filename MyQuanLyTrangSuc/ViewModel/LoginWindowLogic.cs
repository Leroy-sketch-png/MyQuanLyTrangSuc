using MyQuanLyTrangSuc.BusinessLogic; // <-- Đảm bảo đã import AuthenticationService
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View; // <-- Đảm bảo đã import các View cần dùng
using MyQuanLyTrangSuc.View.Windows; // <-- Đảm bảo đã import VerificationWindow, MainNavigationWindow
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls; // Cần cho PasswordBox
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    class LoginWindowLogic
    {
        private readonly NotificationWindowLogic notificationLogic = new NotificationWindowLogic();
        // *** KHÔNG TRUY CẬP TRỰC TIẾP DB CONTEXT TRONG VIEWMODEL ***
        // private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance; // <-- XÓA DÒNG NÀY

        // Sử dụng AuthenticationService thay vì DB Context
        private readonly AuthenticationService authenticationService = AuthenticationService.Instance;


        //Data context binding zone
        public string userName { get; set; }
        private const string USER = "user"; // Dùng hằng số này thay cho chuỗi "user " có dấu cách
        private const string ADMIN = "admin";
        //

        private LoginWindow loginWindow;

        // Constructor mặc định (ít dùng trong MVVM với Window)
        public LoginWindowLogic()
        {
            // Có thể cần khởi tạo service ở đây nếu không dùng singleton Instance
        }

        // Constructor nhận Window (lưu ý: truyền View vào ViewModel không chuẩn MVVM tuyệt đối)
        public LoginWindowLogic(LoginWindow loginWindow)
        {
            this.loginWindow = loginWindow;
            // authenticationService = AuthenticationService.Instance; // Khởi tạo ở đây nếu không ở trên
        }

        public void ChangeToDarkTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = new SolidColorBrush(Color.FromArgb(255, 39, 46, 60)); // #FF272E3C
            leftBorder.Background = new SolidColorBrush(Colors.White); // White
        }

        public void ChangeToLightTheme(Border rightBorder, Border leftBorder)
        {
            rightBorder.Background = new SolidColorBrush(Colors.White); // White
            leftBorder.Background = new SolidColorBrush(Color.FromArgb(255, 39, 46, 60)); // #FF272E3C
        }

        public void LoadVerificationWindow()
        {
            VerificationWindow window = new VerificationWindow();
            window.Show();
        }

        public void Login(PasswordBox passwordBox) // Lưu ý: Truyền PasswordBox vào ViewModel không chuẩn MVVM
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(passwordBox.Password)) // Kiểm tra rỗng cả 2
                {
                    notificationLogic.LoadNotification("Error", "Please enter both username and password.", "BottomRight");
                    return; // Thoát khỏi hàm nếu rỗng
                }

                string password = passwordBox.Password; // Mật khẩu plaintext từ UI

                // *** THAY THẾ LOGIC KIỂM TRA MẬT KHẨU PLAINTEXT TRỰC TIẾP DB BẰNG GỌI SERVICE ***

                // Bước 1: Validate credentials bằng Service (dùng BCrypt bên trong)
                bool isValid = authenticationService.ValidateLogin(userName, password);

                if (isValid)
                {
                    // Bước 2: Nếu Validate thành công, lấy thông tin Account đầy đủ (kèm Group) từ Service
                    // Cần đảm bảo hàm GetAccountWithGroupByUsername này tồn tại trong Service
                    // và thực hiện query DB CÓ INCLUDE Navigation Property Group.
                    Account account = authenticationService.GetAccountWithGroupByUsername(userName);

                    if (account != null) // Kiểm tra lại account có tồn tại không (ValidateLogin đã check rồi nhưng cẩn thận)
                    {
                        WpfApplication.Current.Resources["CurrentUserID"] = account.Username;

                        // Lấy Group Name từ Navigation Property đã được load
                        string groupName = account.Group?.GroupName; // Dùng ?. để tránh lỗi nếu Group vẫn null

                        if (groupName.Equals(USER)) // So sánh với hằng số USER
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in as User!", "BottomRight"); // Thông báo cụ thể hơn
                        }
                        else if (groupName.Equals(ADMIN)) // So sánh với hằng số ADMIN
                        {
                            var mainWindow = new MainNavigationWindow();
                            mainWindow.Show();
                            loginWindow.Close();
                            notificationLogic.LoadNotification("Success", "You have logged in as Admin!", "BottomRight"); // Thông báo cụ thể hơn
                        }
                        else
                        {
                            // Xử lý các Group Name khác nếu có
                            notificationLogic.LoadNotification("Error", "Your account group does not have access permission.", "BottomRight");
                        }
                    }
                    else
                    {
                        // Trường hợp ValidateLogin true nhưng GetAccountWithGroupByUsername lại null (rất hiếm nếu logic đúng)
                        notificationLogic.LoadNotification("Error", "Login failed unexpectedly.", "BottomRight");
                    }
                }
                else
                {
                    // Bước 3: Nếu Validate thất bại
                    notificationLogic.LoadNotification("Error", "Invalid username or password.", "BottomRight");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                notificationLogic.LoadNotification("Error", $"An error occurred during login: {ex.Message}", "BottomRight"); // Thông báo lỗi chi tiết hơn
            }
        }
    }
}