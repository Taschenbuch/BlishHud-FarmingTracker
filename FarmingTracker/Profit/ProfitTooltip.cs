using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class ProfitTooltip : DisposableTooltip
    {
        public ProfitTooltip(Services services)
        {
            var rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 10),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = this
            };

            var font = services.FontService.Fonts[FontSize.Size16];

            new Label
            {
                Text = $"Rough profit when selling everything to vendor and on trading post. Click help button in '{FarmingTrackerWindow.SUMMARY_TAB_TITLE}' tab of the main window for more info.",
                Font = font,
                WrapText = true,
                Width = 420, // 400 cuts off word
                AutoSizeHeight = true,
                Parent = rootFlowPanel,
            };

            ProfitPerHourPanel = new ProfitPanel(null, font, services.TextureService, rootFlowPanel);

            new Label
            {
                Text = " Profit per hour",
                Font = font,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = ProfitPerHourPanel,
            };
        }

        public ProfitPanel ProfitPerHourPanel { get; }
    }
}
