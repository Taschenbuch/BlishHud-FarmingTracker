using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class SearchPanel : Panel
    {
        public SearchPanel(Services services, Container parent)
        {
            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            Parent = parent;

            _searchTextBox = new TextBox
            {
                Text = services.SearchTerm,
                PlaceholderText = "Search...",
                Left = 4,
                Width = 300,
                BasicTooltipText = "Search for items and currencies that include the search term in their name (case insensitive).",
                Parent = this,
            };

            _clearSearchButton = new StandardButton()
            {
                Text = "x",
                Top = _searchTextBox.Top,
                Left = _searchTextBox.Right - 30,
                Width = 30,
                BasicTooltipText = "Clear search input",
                Visible = !string.IsNullOrWhiteSpace(_searchTextBox.Text),
                Parent = this,
            };

            _clearSearchButton.Click += (s, o) => _searchTextBox.Text = "";

            _searchTextBox.TextChanged += (s, o) =>
            {
                var hasSearchTerm = !string.IsNullOrWhiteSpace(_searchTextBox.Text);
                _clearSearchButton.Visible = hasSearchTerm;
                
                services.SearchTerm = _searchTextBox.Text;
                services.UpdateLoop.TriggerUpdateUi();
            };

            SetSize(parent.Width);
        }

        public void UpdateSize(int width)
        {
            SetSize(width);
        }

        private void SetSize(int width)
        {
            _searchTextBox.Width = width > 300
                ? 300
                : width;

            _clearSearchButton.Left = _searchTextBox.Right - 30;
        }

        private readonly TextBox _searchTextBox;
        private readonly StandardButton _clearSearchButton;
    }
}
