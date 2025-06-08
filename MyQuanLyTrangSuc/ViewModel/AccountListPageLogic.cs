using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApplication = System.Windows.Application;

namespace MyQuanLyTrangSuc.ViewModel
{
    public class AccountListPageLogic
    {
        private readonly AuthenticationService accountService;

        private ObservableCollection<Account> accounts;
        public ObservableCollection<Account> Accounts
        {
            get => accounts;
            set
            {
                accounts = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public AccountListPageLogic()
        {
            this.accountService = AuthenticationService.Instance;
            Accounts = new ObservableCollection<Account>();
            LoadAccountsFromDatabase();
            accountService.OnAccountAdded += AccountService_OnAccountAdded;
            accountService.OnAccountUpdated += AuthenticationService_OnAccountUpdated;
        }

        private void AuthenticationService_OnAccountUpdated(Account updatedAccount)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!Accounts.Any(a => a.AccountId == updatedAccount.AccountId))
                {
                    Accounts.Add(updatedAccount);
                }
                else
                {
                    var existing = Accounts.First(a => a.AccountId == updatedAccount.AccountId);
                    existing.Username = updatedAccount.Username;
                    existing.Password = updatedAccount.Password;
                    existing.GroupId = updatedAccount.GroupId;
                    existing.Group = updatedAccount.Group;
                    OnPropertyChanged(nameof(Accounts));
                }
            });
        }

        //catch event for add new account
        private void AccountService_OnAccountAdded(Account account)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Accounts.Add(account);
            });
        }

        // Load accounts from database
        private void LoadAccountsFromDatabase()
        {
            var accounts = accountService.GetListOfAccounts().ToList();
            Accounts = new ObservableCollection<Account>(accounts);
        }

        public void LoadAddAccountWindow()
        {
            var temp = new AddAccountWindow();
            temp.ShowDialog();
        }

        public void LoadEditAccountWindow(Account selectedItem)
        {
            var temp = new EditAccountWindow(selectedItem);
            temp.ShowDialog();
        }

        public void DeleteAccount(Account account)
        {
            if (AuthenticationService.Instance.GetAccountWithGroupByUsername((string)WpfApplication.Current.Resources["CurrentUsername"]).AccountId == account.AccountId)
            {
                MessageBox.Show("You're trying to delete an account you're currently using. This action cannot be done!", "Delete account", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this account?", "Delete account", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                accountService.DeleteAccount(account);
                Accounts.Remove(account);
            }
        }

        public void SearchAccount(string text)
        {
            var filteredAccounts = accountService.GetListOfAccounts()
                .Where(a => a.Username.Contains(text, StringComparison.OrdinalIgnoreCase) && !a.IsDeleted)
                .ToList();
            UpdateAccountList(filteredAccounts);
        }

        private void UpdateAccountList(List<Account> filteredAccounts)
        {
            if (!Accounts.SequenceEqual(filteredAccounts))
            {
                Accounts.Clear();
                foreach (var group in filteredAccounts)
                {
                    Accounts.Add(group);
                }
            }
        }
    }
}
