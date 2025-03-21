using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddSupplierWindowLogic
    {
        private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;

        private NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();

        private String prefix = "SUP";

        //Data Context Zone

        public string NewID
        {
            get { return GenerateNewID(prefix); }
        }

        public string GetLastID()
        {

            var lastID = context.Suppliers.OrderByDescending(c => c.SupplierId).Select(c => c.SupplierId).FirstOrDefault();
            return lastID;

        }
        private string GenerateNewID(string prefix)
        {
            string lastID = GetLastID();
            int newNumber = 1;

            if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix))
            {
                string numericPart = lastID.Substring(prefix.Length);
                if (int.TryParse(numericPart, out int parsedNumber))
                {
                    newNumber = parsedNumber + 1;
                }
            }

            return $"{prefix}{newNumber:D3}";
        }

        //Check Name_Customer
        private bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        //Check Email_Customer
        private bool IsValidEmail(string email)
        {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }

        //Check Tel_Customer
        private bool IsValidTelephoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
        }
        public bool IsValidData(string name, string email, string telephone)
        {
            if (!IsValidName(name)) return false;
            if (!IsValidEmail(email)) return false;
            if (!IsValidTelephoneNumber(telephone)) return false;
            return true;
        }

        public void AddSupplierToDatabase(TextBox nameTextBox, TextBox emailTextBox, TextBox telTextBox, TextBox addressTextBox)
        {
            var temp = context.Suppliers.FirstOrDefault(s => s.Name == nameTextBox.Text && s.Email == emailTextBox.Text && s.ContactNumber == telTextBox.Text);
            if (temp != null)
            {
                if (temp.IsDeleted == true)
                {
                    temp.IsDeleted = false;
                    temp.Name = nameTextBox.Text;
                    temp.Email = emailTextBox.Text;
                    temp.ContactNumber = telTextBox.Text;
                    temp.Address = addressTextBox.Text;

                    context.SaveChangesAdded(temp);
                    notificationWindowLogic.LoadNotification("Success", "Restored supplier successfully!", "BottomRight");
                }
                else
                {
                    notificationWindowLogic.LoadNotification("Warning", "Supplier already exists!", "BottomRight");
                }
                return;
            }
            Supplier supplier = new Supplier()
            {
                SupplierId = NewID,
                Name = nameTextBox.Text,
                Email = emailTextBox.Text,
                ContactNumber = telTextBox.Text,
                Address = addressTextBox.Text,
                IsDeleted = false
            };
            context.Suppliers.Add(supplier);
            context.SaveChangesAdded(supplier);
            notificationWindowLogic.LoadNotification("Success", "Added supplier successfully!", "BottomRight");
        }
    }
}
