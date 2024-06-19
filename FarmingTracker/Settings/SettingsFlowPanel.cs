using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class SettingsFlowPanel : FlowPanel
    {
        public SettingsFlowPanel(Container parent, string title)
        {
            Title = title;
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            BackgroundColor = Color.Black * 0.5f;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(0, 10);
            ShowBorder = true; // helps with padding at the bottom, even though black backround hides it.
            Width = Constants.PANEL_WIDTH;
            HeightSizingMode = SizingMode.AutoSize;
            Parent = parent;
        }
    }
}
