using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;

namespace MyQuanLyTrangSuc.ViewModel
{
    class ResetPasswordLogic
    {
        // Verify
        public string randomCode { get; set; }
        public static string to;
        public static bool flag;

        // Reset
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();

        private bool IsValidEmail(string email)
        {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }

        public void SendCode(TextBox emailTextBox)
        {
            if (string.IsNullOrEmpty(emailTextBox.Text) || !IsValidEmail(emailTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid email", "BottomRight");
                return;
            }

            Random rand = new Random();
            randomCode = rand.Next(999999).ToString("D6"); // always 6 digits

            var message = new MailMessage
            {
                From = new MailAddress("ngominhtri9107@gmail.com"),
                Subject = "Password Reset Code",
                Body = $"Your reset code is: {randomCode}"
            };
            to = emailTextBox.Text;
            message.To.Add(to);

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("ngominhtri9107@gmail.com", "zguvjxsbfykomlsg"),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            try
            {
                smtp.Send(message);
                notificationWindowLogic.LoadNotification("Success", "Code sent successfully!", "BottomRight");
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"Failed to send code: {ex.Message}", "BottomRight");
            }
        }

        public void VerifyCode(TextBox codeTextBox, TextBox emailTextBox)
        {
            if (string.IsNullOrWhiteSpace(codeTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", "Please enter the code!", "BottomRight");
            }
            else if (codeTextBox.Text == randomCode)
            {
                to = emailTextBox.Text;
                var window = new ResetPasswordWindow(emailTextBox.Text);
                window.Show();
                flag = true;
            }
            else
            {
                notificationWindowLogic.LoadNotification("Error", "Wrong code!", "BottomRight");
            }
        }

        public void ResetPassword(PasswordBox newPasswordBox, PasswordBox confirmPasswordBox, string email)
        {
            string newPassword = newPasswordBox.Password?.Trim();
            string confirmPassword = confirmPasswordBox.Password?.Trim();

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                notificationWindowLogic.LoadNotification("Error", "Please fill in all fields!", "BottomRight");
                return;
            }
            if (newPassword != confirmPassword)
            {
                notificationWindowLogic.LoadNotification("Error", "Passwords do not match!", "BottomRight");
                return;
            }

            // 1) Find the employee by email
            var employee = context.Employees.FirstOrDefault(emp => emp.Email == email);
            if (employee == null)
            {
                var account = context.Accounts.FirstOrDefault(acc => acc.Username == employee.Account.Username);
                if (account != null)
                {
                    if (account.Password == newPassword)
                    {
                        notificationWindowLogic.LoadNotification("Error", "The new password cannot be the same as the old one!", "BottomRight");
                        return;
                    }

                    account.Password = newPassword;

            // 4) Apply change and save
            account.Password = newPassword;
            try
            {
                context.SaveChanges();
                notificationWindowLogic.LoadNotification("Success", "Password has been reset successfully!", "BottomRight");
                flag = true;
            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Error", $"Error saving changes: {ex.Message}", "BottomRight");
            }
        }
    }
}
