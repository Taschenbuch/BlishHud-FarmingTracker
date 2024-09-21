using Blish_HUD.Controls;
using System;
using System.Text.RegularExpressions;

namespace FarmingTracker
{
    public class NumberTextBox : TextBox
    {
        public NumberTextBox(int maxNumberLength = 0)
        {
            TextChanged += (s, e) =>
            {
                var oldText = Text;
                var oldCursorIndex = CursorIndex;

                var newText = RemoveNonDigitsAndLeadingZeros(oldText, maxNumberLength);
                var newCursorIndex = DetermineNewCursorIndex(newText, oldCursorIndex);

                Text = newText;
                CursorIndex = newCursorIndex; // must be set AFTER .Text (i think...).
                NumberTextChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public event EventHandler? NumberTextChanged;

        private static int DetermineNewCursorIndex(string newText, int oldCursorIndex)
        {
            var newCursorIndex = oldCursorIndex;
            if (newCursorIndex >= newText.Length)
                newCursorIndex = newText.Length - 1;

            if (newCursorIndex < 0)
                newCursorIndex = 0;

            return newCursorIndex;
        }

        private static string RemoveNonDigitsAndLeadingZeros(string text, int maxNumberLength = 0)
        {
            var number = Regex
                .Replace(text, @"[^\d]", "")
                .TrimStart('0')
                .PadLeft(1, '0');

            if (maxNumberLength == 0)
                return number;

            return number.Length > maxNumberLength
                ? number.Substring(0, maxNumberLength)
                : number;
        }
    }
}
