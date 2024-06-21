using Blish_HUD.Controls;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace FarmingTracker
{
    public class ErrorWindow : StandardWindow
    {
        public ErrorWindow(string windowTitle, string windowText)
            : base(ContentService.Textures.TransparentPixel, new Rectangle(0, 0, WINDOW_WIDTH, WINDOW_HEIGHT), new Rectangle(30, 30, WINDOW_WIDTH - 30, WINDOW_HEIGHT - 30))
        {
            Title = windowTitle;
            BackgroundColor = new Color(Color.Black, 0.8f);
            Location = new Point(50, 50);
            Parent = GameService.Graphics.SpriteScreen;

            var formattedLabel = new FormattedLabelBuilder()
                .SetWidth(WINDOW_WIDTH - 100)
                .AutoSizeHeight()
                .Wrap() // does not really work. Text might be cut off.
                .CreatePart(windowText, builder => builder.SetFontSize(ContentService.FontSize.Size20))
                .Build();

            formattedLabel.Parent = this;
            Height = formattedLabel.Height + 100;
        }

        private const int WINDOW_WIDTH = 910;
        private const int WINDOW_HEIGHT = 300;
    }
}