using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AddCustomerWindowLogic
    {
        //private MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        //private NotificationWindowLogic notificationWindowLogic = new NotificationWindowLogic();


        //private String prefix = "KH";

        ////Data Context Zone

        //public string NewID
        //{
        //    get { return GenerateNewID(prefix); }
        //}

        //public string GetLastID()
        //{

        //    var lastID = context.Customers.OrderByDescending(c => c.CustomerId).Select(c => c.CustomerId).FirstOrDefault();
        //    return lastID;

        //}
        //private string GenerateNewID(string prefix)
        //{
        //    string lastID = GetLastID();
        //    int newNumber = 1;

        //    if (!string.IsNullOrEmpty(lastID) && lastID.StartsWith(prefix))
        //    {
        //        string numericPart = lastID.Substring(prefix.Length);
        //        if (int.TryParse(numericPart, out int parsedNumber))
        //        {
        //            newNumber = parsedNumber + 1;
        //        }
        //    }

        //    return $"{prefix}{newNumber:D3}";
        //}
        ////Check Name_Customer
        //private bool IsValidName(string name)
        //{
        //    return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        //}

        ////Check Email_Customer
        //private bool IsValidEmail(string email)
        //{
        //    var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
        //    return Regex.IsMatch(email, gmailPattern);
        //}

        ////Check Tel_Customer
        //private bool IsValidTelephoneNumber(string phoneNumber)
        //{
        //    return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
        //}
        //public bool IsValidData(string name, string email, string telephone)
        //{
        //    if (!IsValidName(name)) return false;
        //    if (!IsValidEmail(email)) return false;
        //    if (!IsValidTelephoneNumber(telephone)) return false;
        //    return true;
        //}
        //public void AddCustomerToDatabase(TextBox NameTextbox, TextBox EmailTextbox, TextBox TelephoneTextbox, DatePicker birthday, RadioButton gender, TextBox address)
        //{
        //    var temp = context.Customers.FirstOrDefault(c => c.CustomerName == NameTextbox.Text && c.Email == EmailTextbox.Text && c.ContactNumber == TelephoneTextbox.Text);
        //    if (temp != null)
        //    {
        //        if (temp.IsDeleted == true)
        //        {
        //            temp.IsDeleted = false;
        //            temp.CustomerName = NameTextbox.Text;
        //            temp.Email = EmailTextbox.Text;
        //            temp.ContactNumber = TelephoneTextbox.Text;
        //            temp.DateOfBirth = birthday.SelectedDate.HasValue ? DateOnly.FromDateTime(birthday.SelectedDate.Value) : default;
        //            temp.Gender = gender.Content.ToString();
        //            temp.Address = address.Text;

        //            context.SaveChangesAdded(temp);
        //            notificationWindowLogic.LoadNotification("Success", "Restored customer successfully!", "BottomRight");
        //        }
        //        else
        //        {
        //            notificationWindowLogic.LoadNotification("Warning", "Customer already exists!", "BottomRight");
        //        }
        //        return;
        //    }
        //    Customer customer = new Customer()
        //    {
        //        CustomerId = NewID,
        //        CustomerName = NameTextbox.Text,
        //        ContactNumber = TelephoneTextbox.Text,
        //        Email = EmailTextbox.Text,
        //        DateOfBirth = birthday.SelectedDate.HasValue ? DateOnly.FromDateTime(birthday.SelectedDate.Value) : default,
        //        Gender = gender.Content.ToString(),
        //        Address = address.Text,
        //        IsDeleted = false
        //    };
        //    context.Customers.Add(customer);
        //    context.SaveChangesAdded(customer);
        //    notificationWindowLogic.LoadNotification("Success", "Added customer successfully!", "BottomRight");

        //}
        private readonly CustomerService customerService;
        private readonly NotificationWindowLogic notificationWindowLogic;
        public AddCustomerWindowLogic()
        {
            customerService = new CustomerService();
            notificationWindowLogic = new NotificationWindowLogic();
            GenerateNewCustomerID();
        }
        private string _newID;
        public string NewID
        {
            get => _newID;
            private set
            {
                _newID = value;
                OnPropertyChanged(nameof(NewID));
            }
        }
        public void GenerateNewCustomerID()
        {
            NewID = customerService.GenerateNewCustomerID();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public bool AddCustomer(string name, string email, string phone, string address, DateTime? birthday, string gender)
        {
            if (!customerService.IsValidCustomerData(name, email, phone))
            {
                notificationWindowLogic.LoadNotification("Error", "Invalid supplier data!", "BottomRight");
                return false;
            }

            string resultMessage = customerService.AddOrUpdateCustomer(name, email, phone, address, birthday, gender);
            notificationWindowLogic.LoadNotification("Success", resultMessage, "BottomRight");

            GenerateNewCustomerID();
            return true;
        }
    }
}
