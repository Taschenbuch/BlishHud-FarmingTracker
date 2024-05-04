using System;
using System.Collections.Generic;
using Blish_HUD;
using MonoGame.Extended.BitmapFonts;

namespace FarmingTracker
{
    public class FontService
    {
        public FontService()
        {
            CreateFontSizeDict();
        }

        public Dictionary<ContentService.FontSize, BitmapFont> Fonts = new Dictionary<ContentService.FontSize, BitmapFont>();

        private void CreateFontSizeDict()
        {
            var fontSizes = (ContentService.FontSize[])Enum.GetValues(typeof(ContentService.FontSize));

            foreach (var fontSize in fontSizes)
                Fonts[fontSize] = GameService.Content.GetFont(ContentService.FontFace.Menomonia, fontSize, ContentService.FontStyle.Regular);
        }
    }
}