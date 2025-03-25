using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class CustomerService
    {
        private readonly CustomerRepository customerRepository;
        private readonly string prefix = "KH";
        public event Action<Customer> OnCustomerAdded; //add or update
        public event Action<Customer> OnCustomerUpdated; //edit

        //singleton

        private static CustomerService _instance;
        public static CustomerService Instance => _instance ??= new CustomerService();


        public CustomerService()
        {
            customerRepository = new CustomerRepository();
        }

        public string GenerateNewCustomerID()
        {
            string lastID = customerRepository.GetLastCustomerID();
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
        public bool IsValidName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
        }

        public bool IsValidEmail(string email)
        {
            var gmailPattern = @"^(?!.*\.\.)[a-zA-Z0-9._%+-]+(?<!\.)@gmail\.com$";
            return Regex.IsMatch(email, gmailPattern);
        }

        public bool IsValidTelephoneNumber(string phoneNumber)
        {
            return !string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length >= 10 && phoneNumber.Length <= 15;
        }

        public bool IsValidCustomerData(string name, string email, string phone)
        {
            return IsValidName(name) && IsValidEmail(email) && IsValidTelephoneNumber(phone);
        }

        //add new customer
        public string AddOrUpdateCustomer(string name, string email, string phone, string address, DateTime? birthday, string gender)
        {
            var customer = customerRepository.GetCustomerByDetails(name, email, phone);

            if (customer != null)
            {
                if (customer.IsDeleted)
                {
                    customer.IsDeleted = false;
                    customer.CustomerName = name;
                    customer.Email = email;
                    customer.ContactNumber = phone;
                    customer.DateOfBirth = birthday.HasValue ? DateOnly.FromDateTime(birthday.Value) : default;
                    customer.Gender = gender;
                    customer.Address = address;

                    customerRepository.UpdateCustomer(customer);
                    return "Restored customer successfully!";
                }
                return "Customer already exists!";
            }

            Customer newCustomer = new Customer()
            {
                CustomerId = GenerateNewCustomerID(),
                CustomerName = name,
                ContactNumber = phone,
                Email = email,
                DateOfBirth = birthday.HasValue ? DateOnly.FromDateTime(birthday.Value) : default,
                Gender = gender,
                Address = address,
                IsDeleted = false
            };
            customerRepository.AddCustomer(newCustomer);
            OnCustomerAdded?.Invoke(newCustomer);
            return "Added customer successfully!";
        }

        //Get list of customers
        public List<Customer> GetListOfCustomers()
        {
            return customerRepository.GetListOfCustomers();
        }
    }
}
