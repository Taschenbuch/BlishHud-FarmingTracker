using Blish_HUD.Controls;

namespace FarmingTracker
{
    public class ContextMenuService
    {
        public static ContextMenuStrip CreateContextMenu(
            Stat stat, 
            PanelType panelType, 
            SafeList<int> ignoredItemApiIds, 
            SafeList<int> favoriteItemApiIds, 
            Services services)
        {
            var contextMenuStrip = new ContextMenuStrip();

            var wikiMenuItem = contextMenuStrip.AddMenuItem("Open wiki");
            wikiMenuItem.Click += (s, e) => OpenWiki(stat);
            wikiMenuItem.BasicTooltipText = "Open its wiki page in your default browser.";

            if (panelType == PanelType.SummaryRegularItems)
            {
                var ignoreMenuItem = contextMenuStrip.AddMenuItem("Ignore item");
                ignoreMenuItem.Click += (s, e) => IgnoreItem(stat, ignoredItemApiIds, services);
                ignoreMenuItem.BasicTooltipText =
                    $"Ignored items are hidden and dont contribute to profit calculations. " +
                    $"They can be managed in the '{FarmingTrackerWindowService.IGNORED_ITEMS_TAB_TITLE}'-Tab.";

                var addFavoriteMenuItem = contextMenuStrip.AddMenuItem("Add to favorites");
                addFavoriteMenuItem.Click += (s, e) => AddToFavoriteItems(stat, favoriteItemApiIds, services);
                addFavoriteMenuItem.BasicTooltipText = 
                    $"Move item from '{SummaryTabView.ITEMS_PANEL_TITLE}' to '{SummaryTabView.FAVORITE_ITEMS_PANEL_TITLE} panel." +
                    $"Favorite items are not affected by filter or sort.";
            }

            if (panelType == PanelType.SummaryFavoriteItems)
            {
                var removeFavoriteMenuItem = contextMenuStrip.AddMenuItem("Remove from favorites");
                removeFavoriteMenuItem.Click += (s, e) => RemoveFromFavoriteItems(stat, favoriteItemApiIds, services);
                removeFavoriteMenuItem.BasicTooltipText =
                    $"Move item from '{SummaryTabView.FAVORITE_ITEMS_PANEL_TITLE}' to '{SummaryTabView.ITEMS_PANEL_TITLE} panel.";
            }

            return contextMenuStrip;
        }

        private static void RemoveFromFavoriteItems(Stat stat, SafeList<int> favoriteItemApiIds, Services services)
        {
            if (!favoriteItemApiIds.AnySafe(id => id == stat.ApiId))
            {
                Module.Logger.Error("Item is not a favorite item. It shouldnt have been displayed in the first place.");
                return;
            }

            favoriteItemApiIds.RemoveSafe(stat.ApiId);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void AddToFavoriteItems(Stat stat, SafeList<int> favoriteItemApiIds, Services services)
        {
            if (favoriteItemApiIds.AnySafe(id => id == stat.ApiId))
            {
                Module.Logger.Error("Item is already a favorite item. It shouldnt have been displayed in the first place.");
                return;
            }

            favoriteItemApiIds.AddSafe(stat.ApiId);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void IgnoreItem(Stat stat, SafeList<int> ignoredItemApiIds, Services services)
        {
            if (ignoredItemApiIds.AnySafe(id => id == stat.ApiId))
            {
                Module.Logger.Error("Item is already ignored. It shouldnt have been displayed in the first place.");
                return;
            }

            ignoredItemApiIds.AddSafe(stat.ApiId);
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
