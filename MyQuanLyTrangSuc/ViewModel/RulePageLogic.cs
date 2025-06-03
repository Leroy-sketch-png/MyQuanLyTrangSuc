using Microsoft.EntityFrameworkCore;
using MyQuanLyTrangSuc.BusinessLogic;
using MyQuanLyTrangSuc.Model;
using MyQuanLyTrangSuc.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyQuanLyTrangSuc.ViewModel
{
    class RulePageLogic : INotifyPropertyChanged
    {
        private readonly ServiceRecordService serviceRecordService = ServiceRecordService.Instance;
        private readonly MyQuanLyTrangSucContext context = MyQuanLyTrangSucContext.Instance;
        private readonly RulePage rulePageUI;

        private decimal _prepaidPercentage;
        public decimal PrepaidPercentage
        {
            get => _prepaidPercentage;
            set { _prepaidPercentage = value; OnPropertyChanged(); }
        }

        public RulePageLogic(RulePage rulePageUI)
        {
            this.rulePageUI = rulePageUI ?? throw new ArgumentNullException(nameof(rulePageUI));
            rulePageUI.DataContext = this;
            RulesLoad();
        }


        public void RulesLoad()
        {
            PrepaidPercentage = serviceRecordService.GetPrepaidPercentage();
        }
        public void RulesUpdate()
        {
            var pam = context.Parameters.FirstOrDefault(a => a.ConstName == "PrepaidPercentage");
            if (pam != null)
            {
                int affected = context.Database.ExecuteSqlRaw("UPDATE Parameter SET constValue = {0} WHERE constName = 'PrepaidPercentage'", PrepaidPercentage);
                if (affected > 0)
                {
                    MessageBox.Show("The rules have been updated!");
                }
                else
                {
                    MessageBox.Show("No record was updated. Check if 'PrepaidPercentage' exists.");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    }
}
