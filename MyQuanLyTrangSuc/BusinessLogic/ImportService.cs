using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyQuanLyTrangSuc.BusinessLogic
{
    public class ImportService
    {
        private ImportRepository importRepository;
        private readonly string prefix = "IM";
        public event Action<Import> OnImportAdded;
        public event Action<Import> OnImportUpdated;

        //singleton

        private static ImportService _instance;
        public static ImportService Instance => _instance ??= new ImportService();

        public ImportService()
        {
            importRepository = new ImportRepository();
        }

        public string GenerateNewImportID()
        {
            string lastID = importRepository.GetLastImportID();
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
        public List<Product> GetListOfProducts()
        {
            return importRepository.GetListOfProducts();
        }
        public List<Supplier> GetListOfSuppliers()
        {
            return importRepository.GetListOfSuppliers();
        }
   
        public void AddImportDetail(ImportDetail importDetail)
        {
            importRepository.AddImportDetail(importDetail);
        }
        public void AddImport(Import import)
        {
            importRepository.AddImport(import);
            OnImportAdded?.Invoke(import);
        }

        public int GenerateNewImportDetailID()
        {
            return importRepository.GetLastImportDetailID();
        }
       
    }
}
