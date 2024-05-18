using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class CollapsibleHelp : Panel
    {
        public CollapsibleHelp(string helpText, int expandedWidth, Container parent)
        {
            var button = new StandardButton()
            {
                Text = GetHelpButtonText(),
                BackgroundColor = Color.Yellow,
                Width = 100,
                Top = HELP_LABEL_BORDER_SIZE,
                Left = HELP_LABEL_BORDER_SIZE,
            };

            var collapsedHeight = button.Height + 35;
            var collapsedWidth = button.Width + 35;

            ShowBorder = true;
            Height = collapsedHeight;
            Width = collapsedWidth;
            Parent = parent;

            var label = new Label
            {
                Text = helpText,
                VerticalAlignment = VerticalAlignment.Top,
                WrapText = true,
                Width = expandedWidth - 20 - 2 * HELP_LABEL_BORDER_SIZE,
                AutoSizeHeight = true,
                Top = collapsedHeight - 20,
                Left = HELP_LABEL_BORDER_SIZE,
            };

            var expandedHeight = label.Height + collapsedHeight + HELP_LABEL_BORDER_SIZE;

            var blackContainer = new LocationContainer()
            {
                BackgroundColor = Color.Black * 0.5f,
                Width = expandedWidth - 20,
                Height = expandedHeight,
                Top = 4,
                Left = 4,
                Parent = this
            };

            button.Parent = blackContainer;
            label.Parent = blackContainer;

            button.Click += (s, e) =>
            {
                _isHelpExpanded = !_isHelpExpanded;
                button.Text = GetHelpButtonText();
                Height = _isHelpExpanded ? expandedHeight : collapsedHeight;
                Width = _isHelpExpanded ? expandedWidth : collapsedWidth;
            };
        }

        private string GetHelpButtonText()
        {
            return _isHelpExpanded ? HIDE_HELP_BUTTON_TEXT : SHOW_HELP_BUTTON_TEXT;
        }

        private const int HELP_LABEL_BORDER_SIZE = 10;
        private const string SHOW_HELP_BUTTON_TEXT = "Show Help";
        private const string HIDE_HELP_BUTTON_TEXT = "Hide Help";
        private bool _isHelpExpanded;
    }
}
