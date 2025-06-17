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
        private void PrepaidPercentage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValidInput(e.Text, ((TextBox)sender).Text);
        }

        private void PrepaidPercentage_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));
                TextBox tb = (TextBox)sender;
                string newText = tb.Text.Insert(tb.SelectionStart, pasteText);
                if (!IsValidInput(newText, ""))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsValidInput(string newInput, string existingText)
        {
            string combined = existingText + newInput;
            if (!int.TryParse(combined, out int value))
                return false;
            return value >= 0 && value <= 100;
        }

    }
}
