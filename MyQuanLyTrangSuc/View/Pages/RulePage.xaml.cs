using MyQuanLyTrangSuc.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyQuanLyTrangSuc.View
{
    /// <summary>
    /// Interaction logic for RulePage.xaml
    /// </summary>
    public partial class RulePage : Page
    {
        private readonly RulePageLogic rulePageLogic;

        public RulePage()
        {
            InitializeComponent();
            rulePageLogic = new RulePageLogic(this);
        }

        private void OnClick_RuleUpdate(object sender, RoutedEventArgs e)
        {
            rulePageLogic.RulesUpdate();
        }
    }
}
