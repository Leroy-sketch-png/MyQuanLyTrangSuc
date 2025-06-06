﻿using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyQuanLyTrangSuc.DataAccess {
    public class ServiceRecordRepository {
        private readonly MyQuanLyTrangSucContext context;

        public ServiceRecordRepository() {
            context = MyQuanLyTrangSucContext.Instance;
        }

        public string GetLastServiceRecordID() {
            var lastID = context.ServiceRecords
                .OrderByDescending(sr => sr.ServiceRecordId)
                .Select(sr => sr.ServiceRecordId)
                .FirstOrDefault();
            return lastID;
        }

        public void AddServiceRecord(ServiceRecord serviceRecord) {
            context.ServiceRecords.Add(serviceRecord);
            context.SaveChanges();
        }

        public void DeleteServiceRecord(ServiceRecord serviceRecord)
        {
            context.ServiceRecords.Remove(serviceRecord);
            context.SaveChanges();
        }

        public void AddServiceDetail(ServiceDetail serviceDetail) {
            context.ServiceDetails.Add(serviceDetail);
            context.SaveChanges();
        }

        public void DeleteServiceDetail(ServiceDetail serviceDetail) {
            if (serviceDetail != null) {
                context.ServiceDetails.Remove(serviceDetail);
                context.SaveChanges();
            }
        }



        public List<ServiceRecord> GetListOfServiceRecords() {
            return context.ServiceRecords.ToList();
        }

        public List<Service> GetListOfServices() {
            return context.Services.Where(s => !s.IsDeleted).ToList();
        }
        public List<Customer> GetListOfCustomers() {
            return context.Customers.Where(c => !c.IsDeleted).ToList();
        }

        public List<Employee> GetListOfEmployees() {
            return context.Employees.ToList();
        }

        public int GetLastServiceDetailID() {
            var lastStt = context.ServiceDetails
                .OrderByDescending(sd => sd.Stt)
                .Select(sd => sd.Stt)
                .FirstOrDefault();
            return (lastStt != null && lastStt > 0) ? lastStt + 1 : 1;
        }
        public decimal GetPrepaidPercentage()
        {
            decimal PrepaidPercentage;
                var pam = context.Parameters
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ConstName == "PrepaidPercentage");
            if (pam != null)
            {
                PrepaidPercentage = pam.ConstValue ?? 50.0m;
            }
            else
            {
                PrepaidPercentage = 50.0m;
                // Cant find, create immediately
                context.Database.ExecuteSqlRaw("INSERT INTO Parameter (constName, constValue) VALUES ('PrepaidPercentage', {0})", PrepaidPercentage);
            }
            return PrepaidPercentage;
        }
    }
}
