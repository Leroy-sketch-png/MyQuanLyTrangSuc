using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MyQuanLyTrangSuc.View;
using MyQuanLyTrangSuc.Model;
using Microsoft.Xaml.Behaviors.Media;
using System.Text.RegularExpressions;

namespace MyQuanLyTrangSuc.ViewModel
{
    class ResetPasswordLogic
    {
        //Verify
        public string randomCode { get; set; }
        public static string to;
        public static bool flag;

        //Reset
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();

        private bool IsValidEmail(string email)
        {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }

        public void SendCode(TextBox emailTextBox)
        {
            if (!IsValidEmail(emailTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid email", "BottomRight");
                return;
            }
            string from, pass, messageBody;
            Random rand = new Random();
            randomCode = rand.Next(999999).ToString();
            MailMessage message = new MailMessage();
            if (string.IsNullOrEmpty(emailTextBox.Text))
            {
                notificationWindowLogic.LoadNotification("Error", "Please fill in all fields!", "BottomRight");
                return;
            }
            to = emailTextBox.Text;
            from = "ngominhtri9107@gmail.com";
            pass = "zguvjxsbfykomlsg";
            messageBody = "Your reset code is " + randomCode;
            message.To.Add(to);
            message.From = new MailAddress(from);
            message.Body = messageBody;
            message.Subject = "Password Reseting Code";
            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(from, pass);
            try
            {
                smtp.Send(message);
                notificationWindowLogic.LoadNotification("Success", "Code send successfully!", "BottomRight");

            }
            catch (Exception ex)
            {
                notificationWindowLogic.LoadNotification("Success", ex.Message, "BottomRight");
            }
        }

        public void VerifyCode(TextBox codeTextBox, TextBox emailTextBox)
        {
            if (randomCode == codeTextBox.Text)
            {
                to = emailTextBox.Text;
                ResetPasswordWindow window = new ResetPasswordWindow(emailTextBox.Text);
                window.Show();
                flag = true;
            }
            else if (codeTextBox.Text == "")
            {
                notificationWindowLogic.LoadNotification("Error", "Please fill in all fields!", "BottomRight");

            }
            else
            {
                notificationWindowLogic.LoadNotification("Error", "Wrong code!", "BottomRight");
            }
        }
        public void ResetPassword(PasswordBox newPasswordBox, PasswordBox confirmPasswordBox, string email)
        {
            string newPassword = newPasswordBox.Password.Trim();
            string confirmPassword = confirmPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                notificationWindowLogic.LoadNotification("Error", "Please fill in all fields!", "BottomRight");
                return;
            }

            if (newPassword != confirmPassword)
            {
                notificationWindowLogic.LoadNotification("Error", "The new password and confirmation password do not match!", "BottomRight");
                return;
            }
            var employee = context.Employees.FirstOrDefault(emp => emp.Email == email);
            if (employee != null)
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

                    try
                    {
                        context.SaveChanges();
                        notificationWindowLogic.LoadNotification("Success", "Password has been reset successfully!", "BottomRight");
                        flag = true;
                    }
                    catch (Exception ex)
                    {
                        notificationWindowLogic.LoadNotification("Error", $"An error occured while saving changes: {ex.Message}!", "BottomRight");
                    }

                }
                else
                {
                    notificationWindowLogic.LoadNotification("Error", "Account not found in database!", "BottomRight");
                }

            }
            else
            {
                notificationWindowLogic.LoadNotification("Error", "The provided email not found!", "BottomRight");
            }

        }
    }
}
