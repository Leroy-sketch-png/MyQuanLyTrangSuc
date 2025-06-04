using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class ServiceRecordDetailLogic : INotifyPropertyChanged
    {
        private readonly MyQuanLyTrangSucContext _context;

        private ServiceRecord _selectedServiceRecord;
        public ServiceRecord SelectedServiceRecord
        {
            get => _selectedServiceRecord;
            set
            {
                if (_selectedServiceRecord != value)
                {
                    _selectedServiceRecord = value;
                    OnPropertyChanged();
                    LoadServiceDetails();
                    OnPropertyChanged(nameof(GrandTotal));
                }
            }
        }

        private ObservableCollection<ServiceDetail> _serviceDetails = new ObservableCollection<ServiceDetail>();
        public ObservableCollection<ServiceDetail> ServiceDetails
        {
            get => _serviceDetails;
            set
            {
                _serviceDetails = value;
                OnPropertyChanged();
            }
        }

        public decimal GrandTotal => SelectedServiceRecord?.GrandTotal ?? 0m;

        // Khai báo sự kiện mới để thông báo ServiceRecord đã hoàn tất
        public event Action<ServiceRecord> ServiceRecordCompleted;

        public ServiceRecordDetailLogic(ServiceRecord record)
        {
            _context = MyQuanLyTrangSucContext.Instance;

            if (record != null)
            {
                // Bao gồm các liên kết cần thiết để hiển thị đầy đủ thông tin
                SelectedServiceRecord = _context.ServiceRecords
                    .Include(sr => sr.Customer)
                    .Include(sr => sr.Employee)
                    .Include(sr => sr.ServiceDetails)
                        .ThenInclude(sd => sd.Service)
                    .FirstOrDefault(sr => sr.ServiceRecordId == record.ServiceRecordId);
            }
        }

        public void LoadServiceDetails()
        {
            ServiceDetails.Clear();
            if (SelectedServiceRecord?.ServiceDetails != null)
            {
                foreach (var detail in SelectedServiceRecord.ServiceDetails)
                {
                    ServiceDetails.Add(detail);
                }
            }
        }
        public void PrintServiceRecord()
        {
            if (SelectedServiceRecord != null)
            {
                var printPage = new ServiceRecordPrint(SelectedServiceRecord);
                var printDialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    printPage.ShowDialog(); //Optional: preview
                }
            }
            else
            {
                MessageBox.Show("Please select a record to print.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public async Task UpdateServiceDetailStatusAsync(ServiceDetail detailToUpdate)
        {
            if (detailToUpdate == null)
                return;

            var existing = ServiceDetails.FirstOrDefault(d => d.Stt == detailToUpdate.Stt);
            if (existing != null)
            {
                int idx = ServiceDetails.IndexOf(existing);
                ServiceDetails.RemoveAt(idx);

                existing.Status = "Delivered";

                decimal unpaid = existing.Unpaid ?? 0m;

                if (SelectedServiceRecord != null)
                {
                    SelectedServiceRecord.TotalPaid = (SelectedServiceRecord.TotalPaid ?? 0m) + unpaid;
                    SelectedServiceRecord.TotalUnpaid = (SelectedServiceRecord.TotalUnpaid ?? 0m) - unpaid;

                    // Kiểm tra và cập nhật trạng thái của ServiceRecord chính
                    if (SelectedServiceRecord.TotalPaid == SelectedServiceRecord.GrandTotal)
                    {
                        SelectedServiceRecord.Status = "Complete";
                        // Kích hoạt sự kiện khi ServiceRecord hoàn tất
                        ServiceRecordCompleted?.Invoke(SelectedServiceRecord);
                    }
                    else if (SelectedServiceRecord.TotalPaid > 0 && SelectedServiceRecord.TotalPaid < SelectedServiceRecord.GrandTotal)
                    {
                        SelectedServiceRecord.Status = "Partially Paid";
                    }
                    else
                    {
                        SelectedServiceRecord.Status = "Pending"; // Hoặc trạng thái mặc định ban đầu
                    }


                    _context.Entry(SelectedServiceRecord).State = EntityState.Modified;
                }

                existing.Unpaid = 0m;
                _context.Entry(existing).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                ServiceDetails.Insert(idx, existing); // Thêm lại vào ObservableCollection để UI cập nhật
                                                      // (Cũng có thể chỉ cần cập nhật thuộc tính nếu ServiceDetail implement INotifyPropertyChanged)

                OnPropertyChanged(nameof(ServiceDetails));
                OnPropertyChanged(nameof(SelectedServiceRecord)); // Cập nhật SelectedServiceRecord để UI bound vào nó được refresh
                OnPropertyChanged(nameof(GrandTotal)); // Cập nhật GrandTotal nếu nó là thuộc tính tính toán dựa trên ServiceRecord
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}