using Blish_HUD.Controls;
using System.Linq;

namespace FarmingTracker
{
    public class StatContextMenuStrip : ContextMenuStrip
    {
        public StatContextMenuStrip(
            Stat stat, 
            PanelType panelType, 
            SafeList<int> ignoredItemApiIds, 
            SafeList<int> favoriteItemApiIds,
            SafeList<CustomStatProfit> customStatProfits,
            Services services)
        {
            _wikiMenuItem = AddMenuItem("Open wiki");
            _wikiMenuItem.Click += (s, e) => OpenWiki(stat);
            _wikiMenuItem.BasicTooltipText = "Open its wiki page in your default browser.";

            if (panelType == PanelType.SummaryRegularItems)
            {
                _ignoreMenuItem = AddMenuItem("Ignore item");
                _ignoreMenuItem.Click += (s, e) => IgnoreItem(stat, ignoredItemApiIds, services);
                _ignoreMenuItem.BasicTooltipText =
                    $"Ignored items are hidden and dont contribute to profit calculations. " +
                    $"They can be managed in the '{FarmingTrackerWindow.IGNORED_ITEMS_TAB_TITLE}'-Tab.";

                _addFavoriteMenuItem = AddMenuItem("Add to favorites");
                _addFavoriteMenuItem.Click += (s, e) => AddToFavoriteItems(stat, favoriteItemApiIds, services);
                _addFavoriteMenuItem.BasicTooltipText = 
                    $"Move item from '{Constants.ITEMS_PANEL_TITLE}' to '{Constants.FAVORITE_ITEMS_PANEL_TITLE} panel. " +
                    $"Favorite items are not affected by filter or sort.";
            }

            if (panelType == PanelType.SummaryFavoriteItems)
            {
                _removeFavoriteMenuItem = AddMenuItem("Remove from favorites");
                _removeFavoriteMenuItem.Click += (s, e) => RemoveFromFavoriteItems(stat, favoriteItemApiIds, services);
                _removeFavoriteMenuItem.BasicTooltipText =
                    $"Move item from '{Constants.FAVORITE_ITEMS_PANEL_TITLE}' to '{Constants.ITEMS_PANEL_TITLE} panel.";
            }

            if(!stat.IsCoinOrCustomCoin)
            {
                _setCustomProfitMenuItem = AddMenuItem($"Set to a custom profit of 0 copper. Navigate to '{FarmingTrackerWindow.CUSTOM_STAT_PROFIT_TAB_TITLE}' tab to edit or remove the custom profit.");
                _setCustomProfitMenuItem.Click += (s, e) => SetToZeroProfitAndNavigateToProfitTab(stat, customStatProfits, services);
                _setCustomProfitMenuItem.BasicTooltipText = $"Read the help text in the '{FarmingTrackerWindow.CUSTOM_STAT_PROFIT_TAB_TITLE}' tab for more details.";
            }
        }

        protected override void DisposeControl()
        {
            _removeFavoriteMenuItem?.Dispose();
            _addFavoriteMenuItem?.Dispose();
            _ignoreMenuItem?.Dispose();
            _wikiMenuItem?.Dispose();
            _setCustomProfitMenuItem?.Dispose();
            base.DisposeControl();
        }

        private static void SetToZeroProfitAndNavigateToProfitTab(Stat stat, SafeList<CustomStatProfit> customStatProfits, Services services)
        {
            var matchingCustomStatProfit = customStatProfits.ToListSafe().SingleOrDefault(c => c.BelongsToStat(stat));

            if (matchingCustomStatProfit != null) // custom stat already exists -> override its custom profit.
                matchingCustomStatProfit.CustomProfitInCopper = 0;
            else
            {
                var customStatProfit = new CustomStatProfit(stat.ApiId, stat.StatType);
                customStatProfits.AddSafe(customStatProfit);
            }

            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
            services.FarmingTrackerWindow?.ShowWindowAndSelectCustomProfitTab();
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

        private readonly ContextMenuStripItem _wikiMenuItem;
        private readonly ContextMenuStripItem? _ignoreMenuItem;
        private readonly ContextMenuStripItem? _addFavoriteMenuItem;
        private readonly ContextMenuStripItem? _removeFavoriteMenuItem;
        private readonly ContextMenuStripItem? _setCustomProfitMenuItem;
    }
}
