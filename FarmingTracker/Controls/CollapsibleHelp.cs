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

            _collapsedHeight = button.Height + 35;
            _collapsedWidth = button.Width + 35;

            ShowBorder = true;
            Parent = parent;

            _label = new Label
            {
                Text = helpText,
                VerticalAlignment = VerticalAlignment.Top,
                WrapText = true,
                AutoSizeHeight = true,
                Top = _collapsedHeight - 20,
                Left = HELP_LABEL_BORDER_SIZE,
            };

            _blackContainer = new Panel()
            {
                BackgroundColor = Color.Black * 0.5f,
                Top = 4,
                Left = 4,
                Parent = this
            };

            button.Parent = _blackContainer;
            _label.Parent = _blackContainer;


            button.Click += (s, e) =>
            {
                _isHelpExpanded = !_isHelpExpanded;
                button.Text = GetHelpButtonText();
                UpdateSize(_expandedWidth); // must use field here to always use the updated expandedWidth value instead of fixed paramter expandedWidth value.
            };

            UpdateSize(expandedWidth);
        }

        public void UpdateSize(int expandedWidth)
        {
            UpdateHeight();
            UpdateWidth(expandedWidth);
        }

        private void UpdateHeight()
        {
            _expandedHeight = _label.Height + _collapsedHeight + HELP_LABEL_BORDER_SIZE;
            Height = _isHelpExpanded ? _expandedHeight : _collapsedHeight;
            _blackContainer.Height = _expandedHeight;
        }

        private void UpdateWidth(int expandedWith)
        {
            _expandedWidth = expandedWith;
            _label.Width = expandedWith - 20 - 2 * HELP_LABEL_BORDER_SIZE;
            _blackContainer.Width = expandedWith - 20;
            Width = _isHelpExpanded ? _expandedWidth : _collapsedWidth;
        }

        private string GetHelpButtonText()
        {
            return _isHelpExpanded ? HIDE_HELP_BUTTON_TEXT : SHOW_HELP_BUTTON_TEXT;
        }

        private const int HELP_LABEL_BORDER_SIZE = 10;
        private const string SHOW_HELP_BUTTON_TEXT = "Show Help";
        private const string HIDE_HELP_BUTTON_TEXT = "Hide Help";
        private bool _isHelpExpanded;
        private readonly Label _label;
        private int _expandedHeight;
        private readonly Panel _blackContainer;
        private int _expandedWidth;
        private readonly int _collapsedHeight;
        private readonly int _collapsedWidth;
    }
}
