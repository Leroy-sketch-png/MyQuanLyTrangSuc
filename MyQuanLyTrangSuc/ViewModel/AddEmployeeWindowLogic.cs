using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    class AddEmployeeWindowLogic
    {
        private const string PREFIX = "EMP"; // Prefix cho Employee ID
        private readonly MyQuanLyTrangSucContext _context;

        public string NewID => GenerateNewID(PREFIX);
        public string Name { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Position { get; set; } // Thêm thuộc tính Position
        public string ImagePath { get; set; }

        public AddEmployeeWindowLogic(MyQuanLyTrangSucContext context)
        {
            _context = context;
        }

        private string GetLastID()
        {
            var lastEmployee = _context.Employees
                .OrderByDescending(e => e.EmployeeId)
                .FirstOrDefault();
            return lastEmployee?.EmployeeId ?? "EMP000";
        }

        private string GenerateNewID(string prefix)
        {
            string lastID = GetLastID();
            if (int.TryParse(lastID.Substring(prefix.Length), out int number))
            {
                return $"{prefix}{(number + 1):D3}";
            }
            return "EMP001";
        }

        bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, @"^[\p{L} ]+$");


        bool IsValidEmail(string email) => !string.IsNullOrWhiteSpace(email) && Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$");


        bool IsValidTelephoneNumber(string phoneNumber) => !string.IsNullOrWhiteSpace(phoneNumber) && Regex.IsMatch(phoneNumber, @"^\+?\d{10,15}$");



        public bool AddEmployeeToDatabase(string name, string email, string telephone, string position, string imagePath)
        {
            if (!IsValidName(name) || !IsValidEmail(email) || !IsValidTelephoneNumber(telephone))
            {
                return false;
            }
            Employee emp = new Employee
            {
                EmployeeId = NewID,
                Name = name,
                Position = position,
                Email = email,
                ContactNumber = telephone,
                ImagePath = imagePath
            };

            _context.Employees.Add(emp);
            _context.SaveChanges(); 
            return true;
        }

        public void ChooseImageFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePath = openFileDialog.FileName;
            }
        }
    }
}
