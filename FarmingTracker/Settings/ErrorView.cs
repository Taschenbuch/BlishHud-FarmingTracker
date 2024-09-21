using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ErrorView : View
    {
        protected override void Build(Container buildPanel)
        {
            new HintLabel(buildPanel, "Module failed loading :(. Report in BlishHUD Discord please!");
            new OpenUrlInBrowserButton("https://discord.com/invite/FYKN3qh", "Open BlishHUD Discord in your default browser", "", null, buildPanel)
            {
                Location = new Point(0, 30)
            };
        }
    }
}
