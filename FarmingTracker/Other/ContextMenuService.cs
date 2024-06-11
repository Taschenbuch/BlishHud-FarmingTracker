using Blish_HUD.Controls;
using System.Linq;

namespace FarmingTracker
{
    public class ContextMenuService
    {
        public static ContextMenuStrip CreateContextMenu(Stat stat, Services services)
        {
            var contextMenuStrip = new ContextMenuStrip();

            var wikiMenuItem = contextMenuStrip.AddMenuItem("Open Wiki");
            wikiMenuItem.Click += (s, e) => OpenWiki(stat);
            wikiMenuItem.BasicTooltipText = "Open its wiki page in your default browser.";

            if (stat.StatType == StatType.Item)
            {
                var ignoreMenuItem = contextMenuStrip.AddMenuItem("Ignore item");
                ignoreMenuItem.Click += (s, e) => IgnoreItem(stat, services);
                ignoreMenuItem.BasicTooltipText =
                    $"Ignored items are hidden and dont contribute to profit calculations. " +
                    $"They can be managed in the '{FarmingTrackerWindowService.IGNORED_ITEMS_TAB_TITLE}'-Tab.";
            }

            return contextMenuStrip;
        }

        private static void IgnoreItem(Stat stat, Services services)
        {
            if (services.Model.IgnoredItemApiIds.Any(id => id == stat.ApiId))
            {
                Module.Logger.Error("Item is already ignored. It shouldnt have been displayed in the first place.");
                return;
            }

            services.Model.IgnoredItemApiIds.Add(stat.ApiId);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void OpenWiki(Stat stat)
        {
            if (stat.Details.State == ApiStatDetailsState.MissingBecauseUnknownByApi)
                WikiService.OpenWikiIdQueryInDefaultBrowser(stat.ApiId);

            if (stat.Details.HasWikiSearchTerm)
                WikiService.OpenWikiSearchInDefaultBrowser(stat.Details.WikiSearchTerm);
        }
    }
}
