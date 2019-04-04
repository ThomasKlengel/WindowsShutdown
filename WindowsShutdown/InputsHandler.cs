using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace WindowsShutdown
{
    partial class InputsHandler
    {
        private void TB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // "\d" = any digit
            if (!Regex.Match(e.Text, @"\d").Success || ((TextBox)sender).Text.Length >= 4)
            {
                e.Handled = true;
            }
        }
    }
}
