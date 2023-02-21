using System.Windows.Controls;
using System.Windows.Input;

namespace OsEngine.Views
{
    public class IntTextBox:TextBox
    {
        public IntTextBox() 
        {
            PreviewTextInput += IntTextBox_PreviewTextInput;
            TextChanged += IntTextBox_TextChanged;
        }

        private void IntTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text,0))            
                e.Handled = true;
        }

        private void IntTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.Select(tb.Text.Length, 0);
        }
    }
}