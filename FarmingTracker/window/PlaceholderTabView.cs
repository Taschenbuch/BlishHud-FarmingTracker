using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace FarmingTracker
{
    public class PlaceholderTabView : View
    {
        protected override void Build(Container buildPanel)
        {
            ControlFactory.CreateHintLabel(buildPanel, "not yet implemented");
        }
    }
}
