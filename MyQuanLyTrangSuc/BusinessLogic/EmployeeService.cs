using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class EmployeeService
    {
        private readonly EmployeeRepository employeeRepository;
        private readonly string prefix = "EMP";

        //singleton

        private static EmployeeService _instance;
        public static EmployeeService Instance => _instance ??= new EmployeeService();

        public EmployeeService()
        {
            employeeRepository = new EmployeeRepository();
        }

        // get
        public Employee GetEmployeeByAccountId(int accountId)
        {
            return employeeRepository.GetEmployeeByAccountId(accountId);
        }

        public string GenerateNewEmployeeID()
        {
            string lastID = employeeRepository.GetLastEmployeeID();
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

        public bool IsValidEmployeeData(string name, string email, string phone)
        {
            return IsValidName(name) && IsValidEmail(email) && IsValidTelephoneNumber(phone);
        }

        public string AddOrUpdateEmployee(string name, string email, string phone, string imagePath, DateTime? birthday, string gender, bool isDeleted)
        {
            var employee = employeeRepository.GetEmployeeByDetails(name, email, phone);

            if (employee != null)
            {
                if (employee.IsDeleted)
                {
                    employee.IsDeleted = false;
                    employee.Name = name;
                    employee.Email = email;
                    employee.ContactNumber = phone;
                    employee.DateOfBirth = birthday;
                    employee.Gender = gender;
                    employee.ImagePath = imagePath;

                    employeeRepository.UpdateEmployee(employee);
                    return "Restored employee successfully!";
                }
                return "Employee already exists!";
            }

            Employee newEmployee = new Employee()
            {
                EmployeeId = GenerateNewEmployeeID(),
                Name = name,
                ContactNumber = phone,
                Email = email,
                DateOfBirth = birthday,
                Gender = gender,
                ImagePath = imagePath,
                IsDeleted = false
            };
            employeeRepository.AddEmployee(newEmployee);
            return "Added employee successfully!";
        }
    }
}
