using System.Linq;
using System.Text.RegularExpressions;

namespace OsEngine.Views
{
    public class AcceptedText
    {
        public string Text { get; set; } = string.Empty;

        private static AcceptedText acceptedText = new AcceptedText();

        char[] acceptedCharsForUnsignedDecimalTexBox = new char[] { '.' };
        char[] acceptedCharsForDecimalTextBox = new char[] { '.', '-' };        

        public static AcceptedText Create()
        {
            return acceptedText;
        }

        private AcceptedText() { }

        public bool CheckForUnsignedDecimalTextBox()
        {
            if (!IsDotCorrect())
                return false;

            return CheckForAcceptedSymbols(acceptedCharsForUnsignedDecimalTexBox);
        }

        private bool IsDotCorrect()
        {
            if (Text.Length == 0)
                return true;

            if (Text[0] == '.')
                return false;

            if (Text.Count(p => p == '.') > 1)
                return false;

            return true;
        }

        private bool CheckForAcceptedSymbols(char[] symbols)
        {
            var regex = new Regex(@"\D");               //поиск любых символов, исключая цифры
            var matches = regex.Matches(Text);
            foreach (Match match in matches)
                if (!symbols.Contains(match.Value[0]))
                    return false;

            return true;
        }

        public bool CheckForDecimalTextBox()
        {
            if (!IsSignCorrect())
                return false;

            if (!IsDotCorrect())
                return false;

            return CheckForAcceptedSymbols(acceptedCharsForDecimalTextBox);
        }

        private bool IsSignCorrect()
        {
            if (Text.IndexOf("-") > 0)
                return false;

            if (Text.Count(p => p == '-') > 1)
                return false;

            if (Text.Contains("-."))
                return false;

            return true;
        }
    }
}