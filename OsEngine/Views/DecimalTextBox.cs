using System.Windows.Controls;
using System.Windows.Input;

namespace OsEngine.Views
{
    public class DecimalTextBox : UnsignedDecimalTextBox
    {
        protected override void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {   
            base.DecimalTextBox_PreviewTextInput(sender, e);            
            if (IsNewTextContainsMinusAndTextBoxIsEmpty())
                e.Handled = false;
        }

        protected override void DecimalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            acceptedText.Text = Text;
            if (acceptedText.CheckForDecimalTextBox())
                textBeforeChange = Text;
            else
                Text = textBeforeChange;

            Select(Text.Length, 0);
            textBeforeChange = Text;
        }
    }    
}