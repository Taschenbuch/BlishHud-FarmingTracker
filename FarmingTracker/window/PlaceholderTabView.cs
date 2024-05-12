using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace FarmingTracker
{
    public class PlaceholderTabView : View
    {
        public PlaceholderTabView(string featureText)
        {
            _featureText = featureText;
        }

        protected override void Build(Container buildPanel)
        {
            ControlFactory.CreateHintLabel(buildPanel, $"{_featureText} not yet implemented. May come with a future release!");
        }

        private readonly string _featureText;
    }
}
