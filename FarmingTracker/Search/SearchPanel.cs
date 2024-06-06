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

            var searchTextBox = new TextBox
            {
                Text = services.SearchTerm,
                PlaceholderText = "Search...",
                Left = 4,
                Width = 300,
                BasicTooltipText = "Search for items and currencies that include the search term in their name (case insensitive).",
                Parent = this,
            };

            var clearSearchButton = new StandardButton()
            {
                Text = "x",
                Top = searchTextBox.Top,
                Left = searchTextBox.Right - 30,
                Width = 30,
                BasicTooltipText = "Clear search input",
                Visible = !string.IsNullOrWhiteSpace(searchTextBox.Text),
                Parent = this,
            };

            clearSearchButton.Click += (s, o) => searchTextBox.Text = "";

            searchTextBox.TextChanged += (s, o) =>
            {
                var hasSearchTerm = !string.IsNullOrWhiteSpace(searchTextBox.Text);
                clearSearchButton.Visible = hasSearchTerm;
                
                services.SearchTerm = searchTextBox.Text;
                services.UpdateLoop.TriggerUpdateUi();
            };
        }
    }
}
