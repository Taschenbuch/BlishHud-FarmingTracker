using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace FarmingTracker
{
    public class OpenUrlInBrowserButton : StandardButton
    {
        public OpenUrlInBrowserButton(string url, string buttonText, string buttonTooltip, Texture2D? buttonIcon, Container parent)
        {

            Text = buttonText;
            BasicTooltipText = buttonTooltip;
            Icon = buttonIcon;
            Width = 300;
            Parent = parent;

            Click += (s, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            };
        }
    }
}
