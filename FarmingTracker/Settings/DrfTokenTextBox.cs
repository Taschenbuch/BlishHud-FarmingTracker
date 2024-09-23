using Blish_HUD.Controls;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Text.RegularExpressions;

namespace FarmingTracker
{
    public class DrfTokenTextBox : TextBox
    {
        public DrfTokenTextBox(string text, string tooltip, BitmapFont font, Container parent)
        {
            Text = text;
            BasicTooltipText = tooltip;
            PlaceholderText = "Enter token...";
            Font = font;
            Width = 350; // widest token because no monospace: 00000000-0000-0000-0000-000000000000
            Height = 40;
            Parent = parent;

            TextChanged += (s, e) =>
            {
                var token = Text;
                // als SanitizedTextBox: Textbox auslagern? -> override OnTextChanged.
                var sanitizedToken = GetLowerCaseAndRemoveIllegalCharacters(token);
                SanitizeTextBoxText(token, sanitizedToken);
                SanitizedTextChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        public event EventHandler? SanitizedTextChanged;

        // allow only 0-9 a-f A-F and '-'.
        // upper case A-F is converted to lower case a-f. 
        // prevents that cursor position is reset to index 1 when typing illegal characters.
        // cursor position will still move when A-F is replaced by a-f.
        private void SanitizeTextBoxText(string token, string sanitizedToken)
        {
            if (token == sanitizedToken)
                return;

            var oldCursorIndex = CursorIndex;
            Text = sanitizedToken;
            CursorIndex = DetermineNewCursorIndex(token, sanitizedToken, oldCursorIndex);
        }

        private static int DetermineNewCursorIndex(string token, string sanitizedToken, int oldCursorIndex)
        {
            var onlyUpperCaseWasReplacedByLowerCaseCharacter = token.ToLower() == sanitizedToken;
            if (onlyUpperCaseWasReplacedByLowerCaseCharacter)
                return oldCursorIndex;

            var cursorWasAtTheEndWhenIllegalCharacterWasTypedIn = oldCursorIndex >= sanitizedToken.Length;
            var newCursorIndex = cursorWasAtTheEndWhenIllegalCharacterWasTypedIn
                ? sanitizedToken.Length - 1
                : oldCursorIndex - 1;

            if (newCursorIndex < 0)
                newCursorIndex = 0;

            return newCursorIndex;
        }

        private static string GetLowerCaseAndRemoveIllegalCharacters(string drfToken)
        {
            try
            {
                return _charactersNotAllowedInDrfTokenRegex.Replace(drfToken.ToLower(), "");
            }
            catch (RegexMatchTimeoutException)
            {
                Module.Logger.Error($"regex timedout for drf {nameof(_charactersNotAllowedInDrfTokenRegex)}.");
                return "0";
            }
        }

        private static readonly Regex _charactersNotAllowedInDrfTokenRegex = new Regex(@"[^a-f0-9-]", RegexOptions.None, TimeSpan.FromMilliseconds(1000));
    }
}
