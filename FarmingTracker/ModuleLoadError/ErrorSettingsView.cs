using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace FarmingTracker
{
    public class ErrorSettingsView : View
    {
        public ErrorSettingsView(string infoText)
        {
            _infoText = infoText;
        }

        protected override void Build(Container buildPanel)
        {
            _formattedLabel = new FormattedLabelBuilder() // usefull to embed clickable hyperlinks.
                .SetWidth(buildPanel.Width)
                .AutoSizeHeight()
                .Wrap()
                .CreatePart(_infoText, builder => builder.SetFontSize(ContentService.FontSize.Size16))
                .Build();

            _formattedLabel.Parent = buildPanel;
        }

        protected override void Unload()
        {
            _formattedLabel?.Dispose();
        }

        private FormattedLabel _formattedLabel;
        private readonly string _infoText;
    }
}