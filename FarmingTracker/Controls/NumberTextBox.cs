using Blish_HUD.Controls;
using System;
using System.Text.RegularExpressions;

namespace FarmingTracker
{
    public class NumberTextBox : TextBox
    {
        public NumberTextBox(int maxNumberLength = 8) // 8 = prevent of integer range exception when parsing textBox.Text.
        {
            PlaceholderText = "0";

            TextChanged += (s, e) =>
            {
                var oldText = Text;
                var oldCursorIndex = CursorIndex;

                var newText = RemoveNonDigitsAndLeadingZeros(oldText, maxNumberLength);
                var newCursorIndex = DetermineNewCursorIndex(newText, oldText, oldCursorIndex);

                Text = newText;
                CursorIndex = newCursorIndex; // must be set AFTER .Text (i think...).
                NumberTextChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public event EventHandler? NumberTextChanged;

        // this is a hack, it will never work properly because the oldCursorIndex is not from before changing the Text. TextBox already updated it.
        private static int DetermineNewCursorIndex(string newText, string oldText, int oldCursorIndex) 
        {
            var newCursorIndex = oldCursorIndex;

            if (newText.Length == oldText.Length) 
                return oldCursorIndex; // no illegal character was removed. text <= maxNumberLength.

            if (newCursorIndex >= newText.Length)
                newCursorIndex = newText.Length - 1; // prevents cursor from moving

            if (newCursorIndex < 0)
                newCursorIndex = 0;

            return newCursorIndex;
        }

        private static string RemoveNonDigitsAndLeadingZeros(string text, int maxNumberLength = 0)
        {
            try
            {
                var number = Regex
                    .Replace(text, @"[^\d]", "", RegexOptions.None, TimeSpan.FromMilliseconds(1000))
                    .TrimStart('0');

                return number.Length > maxNumberLength
                    ? number.Substring(0, maxNumberLength)
                    : number;
            }
            catch (RegexMatchTimeoutException)
            {
                Module.Logger.Error($"regex timedout for NumberTextBox.");
                return "0";
            }
        }
    }
}
