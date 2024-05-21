using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace FarmingTracker
{
    public class PlaceholderTabView : View
    {
        public PlaceholderTabView(string featureText, bool onlyShowFeatureText = false)
        {
            _featureText = featureText;
            _onlyShowFeatureText = onlyShowFeatureText;
        }

        protected override void Build(Container buildPanel)
        {
            var text = _onlyShowFeatureText
                ? _featureText
                : $"{_featureText} not yet implemented. May come with a future release!";
            new HintLabel(buildPanel, text);
        }

        private readonly string _featureText;
        private readonly bool _onlyShowFeatureText;
    }
}
