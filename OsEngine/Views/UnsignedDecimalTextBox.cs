using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace OsEngine.Views
{
    public class UnsignedDecimalTextBox : TextBox
    {
        protected string textBeforeChange;
        protected string newText;
        protected AcceptedText acceptedText;

        public UnsignedDecimalTextBox()
        {
            acceptedText = AcceptedText.Create();
            PreviewTextInput += DecimalTextBox_PreviewTextInput;
            TextChanged += DecimalTextBox_TextChanged;
        }

        protected virtual void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            newText = e.Text;
            e.Handled = true;

            if (IsNewTextContainsADigit() || IsNewTextContainsDotAndTextBoxDoesntAlreadyContainsDotAndTextBoxContainsADigit())
                e.Handled = false;
        }

        protected virtual void DecimalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            acceptedText.Text = Text;
            if (acceptedText.CheckForUnsignedDecimalTextBox())
                textBeforeChange = Text;
            else
                Text = textBeforeChange;
                      
            Select(Text.Length, 0);
            textBeforeChange = Text;
        }

        protected bool IsNewTextContainsADigit()
        {
            return char.IsDigit(newText, 0);
        }

        protected bool IsNewTextContainsMinusAndTextBoxIsEmpty()
        {
            return newText.Contains("-") && Text.Length == 0;
        }

        protected bool IsNewTextContainsDotAndTextBoxDoesntAlreadyContainsDotAndTextBoxContainsADigit()
        {
            var isTextBoxAlreadyContainsDot = false;
            if (Text.Contains("."))
                isTextBoxAlreadyContainsDot = true;

            Regex regex = new Regex(@"\d");                 //поиск любых цифр
            MatchCollection matches = regex.Matches(Text);
            var doesTextBoxContainsADigit = matches.Count > 0;

            return newText.Contains(".") && !isTextBoxAlreadyContainsDot && doesTextBoxContainsADigit;
        }
    }
}