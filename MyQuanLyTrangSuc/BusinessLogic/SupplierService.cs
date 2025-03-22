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
    public class SupplierService
    {
        private readonly SupplierRepository supplierRepository;
        private readonly string prefix = "SUP";

        public SupplierService()
        {
            supplierRepository = new SupplierRepository();
        }

        public string GenerateNewSupplierID()
        {
            string lastID = supplierRepository.GetLastSupplierID();
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

        public bool IsValidSupplierData(string name, string email, string phone)
        {
            return IsValidName(name) && IsValidEmail(email) && IsValidTelephoneNumber(phone);
        }
        public string AddOrUpdateSupplier(string name, string email, string phone, string address)
        {
            var supplier = supplierRepository.GetSupplierByDetails(name, email, phone);
            if (supplier != null)
            {
                if (supplier.IsDeleted)
                {
                    supplier.IsDeleted = false;
                    supplier.Address = address;
                    supplierRepository.UpdateSupplier(supplier);
                    return "Restored supplier successfully!";
                }
                return "Supplier already exists!";
            }

            Supplier newSupplier = new Supplier()
            {
                SupplierId = GenerateNewSupplierID(),
                Name = name,
                Email = email,
                ContactNumber = phone,
                Address = address,
                IsDeleted = false
            };

            supplierRepository.AddSupplier(newSupplier);
            return "Added supplier successfully!";
        }
    }
}
