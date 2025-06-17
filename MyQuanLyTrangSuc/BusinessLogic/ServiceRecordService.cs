using MyQuanLyTrangSuc.DataAccess;
using MyQuanLyTrangSuc.Model;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Make sure you have this using statement
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace MyQuanLyTrangSuc.BusinessLogic {
    public class ServiceRecordService
    {
        private ServiceRecordRepository serviceRecordRepository;
        private readonly string prefix = "SRV";
        public event Action<ServiceRecord> OnServiceRecordAdded;
        public event Action<ServiceRecord> OnServiceRecordUpdated;
        public event Action<ServiceRecord> OnServiceRecordDeleted;

        // Singleton
        private static ServiceRecordService _instance;
        public static ServiceRecordService Instance => _instance ??= new ServiceRecordService();

        public ServiceRecordService()
        {
            serviceRecordRepository = new ServiceRecordRepository();
        }
        public ObservableCollection<MyQuanLyTrangSuc.Model.Customer> Customers { get; set; }

        public string GenerateNewServiceRecordID()
        {
            string lastID = serviceRecordRepository.GetLastServiceRecordID();
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
        public List<Service> GetListOfServices()
        {
            return serviceRecordRepository.GetListOfServices();
        }

        public List<Customer> GetListOfCustomers()
        {
            return serviceRecordRepository.GetListOfCustomers();
        }

        public List<Employee> GetListOfEmployees()
        {
            return serviceRecordRepository.GetListOfEmployees();
        }

        public void AddServiceDetail(ServiceDetail serviceDetail)
        {
            serviceRecordRepository.AddServiceDetail(serviceDetail);
        }

        public void AddServiceRecord(ServiceRecord serviceRecord)
        {
            serviceRecordRepository.AddServiceRecord(serviceRecord);
            OnServiceRecordAdded?.Invoke(serviceRecord);
        }

        public void DeleteServiceDetail(ServiceDetail serviceDetail)
        {
            serviceRecordRepository.DeleteServiceDetail(serviceDetail);

        }
        public void DeleteServiceRecord(ServiceRecord serviceRecord)
        {
            serviceRecordRepository.DeleteServiceRecord(serviceRecord);
            OnServiceRecordDeleted?.Invoke(serviceRecord);
        }

        public int GenerateNewServiceDetailID()
        {
            return serviceRecordRepository.GetLastServiceDetailID();
        }

        public decimal GetPrepaidPercentage()
        {
            return serviceRecordRepository.GetPrepaidPercentage();
        }
    }
}
