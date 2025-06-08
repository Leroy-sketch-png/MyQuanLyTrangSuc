using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AssignAccountWindowLogic
    {
        private AuthenticationService authenticationService;
        private NotificationWindowLogic notificationWindowLogic;
        private EmployeeService employeeService;
        private Employee employee;


        public ObservableCollection<Account> Accounts { get; set; }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (_selectedAccount != value)
                {
                    _selectedAccount = value;
                    OnPropertyChanged(nameof(SelectedAccount));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AssignAccountWindowLogic(Employee employee)
        {
            notificationWindowLogic = new NotificationWindowLogic();
            authenticationService = AuthenticationService.Instance;
            employeeService = EmployeeService.Instance;
            this.employee = employee;
            Accounts = new ObservableCollection<Account>();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            List<Account> allAccounts = authenticationService.GetListOfAccounts();
            foreach (Account account in allAccounts)
            {
                // the service cant find the employee using the account, indicating the account is unassigned
                if (employeeService.GetEmployeeByAccountId(account.AccountId) == null)
                {
                    Accounts.Add(account);
                }
            }
            // let the employee be assigned with the account he/she is already assigned with
            if (employee.Account != null)
            {
                Accounts.Add(employee.Account);
            }
            SelectedAccount = employee.Account;
        }

        public bool AssignAccount()
        {
            if (employee == null)
            {
                notificationWindowLogic.LoadNotification("Error", "Employee is not found!", "BottomRight");
                return false;
            }
            if (SelectedAccount == null && employee.Account != null)
            {
                MessageBoxResult result = MessageBox.Show("No account selected. Are you sure you want to unassign this employee's account?", "Confirm?", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.Cancel) return false;
            }
            employee.AccountId = (SelectedAccount != null) ? SelectedAccount.AccountId : null;
            employee.Account = SelectedAccount;
            MyQuanLyTrangSucContext.Instance.SaveChanges();
            notificationWindowLogic.LoadNotification("Success", "Assign account successfully", "BottomRight");
            return true;
        }
    }
}
