using Blish_HUD;
using System;
using System.Linq;

namespace FarmingTracker
{
    public class StatContextMenuStrip : CustomContextMenuStrip
    {
        public StatContextMenuStrip(
            Stat stat, 
            PanelType panelType, 
            SafeList<int> ignoredItemApiIds, 
            SafeList<int> favoriteItemApiIds,
            SafeList<CustomStatProfit> customStatProfits,
            Services services)
        {
            _generalHeaderMenuItem = new CustomContextMenuStripItem("General", this, true);

            if (panelType == PanelType.SummaryRegularItems)
            {
                _ignoreMenuItem = new CustomContextMenuStripItem("Ignore item", this);
                _ignoreMenuItem.Click += (s, e) => IgnoreItem(stat, ignoredItemApiIds, services);
                _ignoreMenuItem.BasicTooltipText =
                    $"Ignored items are hidden and dont contribute to profit calculations. " +
                    $"They can be managed in the '{Constants.TabTitles.IGNORED}'-Tab.";

                _addFavoriteMenuItem = new CustomContextMenuStripItem("Add to favorites", this);
                _addFavoriteMenuItem.Click += (s, e) => AddToFavoriteItems(stat, favoriteItemApiIds, services);
                _addFavoriteMenuItem.BasicTooltipText =
                    $"Move item from '{Constants.ITEMS_PANEL_TITLE}' to '{Constants.FAVORITE_ITEMS_PANEL_TITLE} panel. " +
                    $"Favorite items are not affected by filter or sort.";
            }

            if (panelType == PanelType.SummaryFavoriteItems)
            {
                _removeFavoriteMenuItem = new CustomContextMenuStripItem("Remove from favorites", this);
                _removeFavoriteMenuItem.Click += (s, e) => RemoveFromFavoriteItems(stat, favoriteItemApiIds, services);
                _removeFavoriteMenuItem.BasicTooltipText =
                    $"Move item from '{Constants.FAVORITE_ITEMS_PANEL_TITLE}' to '{Constants.ITEMS_PANEL_TITLE} panel.";
            }

            _setCustomProfitMenuItem = new CustomContextMenuStripItem($"Set to a custom profit of 0 copper. Navigate to '{Constants.TabTitles.CUSTOM_STAT_PROFIT}' tab to edit or remove the custom profit.", this);
            _setCustomProfitMenuItem.Click += (s, e) => SetToZeroProfitAndNavigateToProfitTab(stat, customStatProfits, services);
            _setCustomProfitMenuItem.Enabled = !stat.IsCoinOrCustomCoin;
            _setCustomProfitMenuItem.BasicTooltipText = stat.IsCoinOrCustomCoin
                ? "Not available for coins."
                : $"Read the help text in the '{Constants.TabTitles.CUSTOM_STAT_PROFIT}' tab for more details.";

            _copyHeaderMenuItem = new CustomContextMenuStripItem("Copy", this, true);

            _copyNameMenuItem = new CustomContextMenuStripItem("Name", this);
            _copyNameMenuItem.Click += async (s, e) => await ClipboardUtil.WindowsClipboardService.SetTextAsync(stat.Details.Name);
            _copyNameMenuItem.BasicTooltipText = "Copy item/currency name to clipboard (like CTRL + C). You can paste it somewhere else with CTRL + V";

            _copyNameMenuItem = new CustomContextMenuStripItem("Chat link", this);
            _copyNameMenuItem.Click += async (s, e) => await ClipboardUtil.WindowsClipboardService.SetTextAsync(stat.Details.WikiSearchTerm);
            _copyNameMenuItem.Enabled = stat.StatType == StatType.Item;
            _copyNameMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Copy item/currency chat link to clipboard (like CTRL + C). You can paste it somewhere else with CTRL + V. Chat links for items you dont own anymore, may not work."
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _websitesHeaderMenuItem = new CustomContextMenuStripItem("Open website", this, true);

            _wikiMenuItem = new CustomContextMenuStripItem("Wiki", this);
            _wikiMenuItem.Click += (s, e) => OpenWiki(stat);
            _wikiMenuItem.BasicTooltipText = "Open its wiki page in your default browser.";

            var languageString = BrowserService.GetGw2EfficiencyLanguageString();

            _gw2EfficiencyTradingPostMenuItem = new CustomContextMenuStripItem("GW2 Efficiency (trading post search)", this);
            _gw2EfficiencyTradingPostMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://gw2efficiency.com/tradingpost?filter.search.term={Uri.EscapeDataString(stat.Details.Name)}&lang={languageString}");
            _gw2EfficiencyTradingPostMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2EfficiencyTradingPostMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 efficiency' trading post search page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _gw2EfficiencyAccountMenuItem = new CustomContextMenuStripItem("GW2 Efficiency (account search)", this);
            _gw2EfficiencyAccountMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://gw2efficiency.com/account/overview?filter.name={Uri.EscapeDataString(stat.Details.Name)}&lang={languageString}");
            _gw2EfficiencyAccountMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2EfficiencyAccountMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 Efficiency' account search page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _gw2BltcMenuItem = new CustomContextMenuStripItem("GW2 BLTC", this);
            _gw2BltcMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://www.gw2bltc.com/en/item/{stat.ApiId}");
            _gw2BltcMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2BltcMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 BLTC' page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _gw2TreasuresMenuItem = new CustomContextMenuStripItem("GW2 Treasures", this);
            _gw2TreasuresMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://en.gw2treasures.com/item/{stat.ApiId}");
            _gw2TreasuresMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2TreasuresMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 Treasures' page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _gw2TpMenuItem = new CustomContextMenuStripItem("GW2 TP", this);
            _gw2TpMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://www.gw2tp.com/item/{stat.ApiId}");
            _gw2TpMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2TpMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 TP' page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;

            _gw2ProfitsMenuItem = new CustomContextMenuStripItem("GW2 Profits", this);
            _gw2ProfitsMenuItem.Click += (s, e) => BrowserService.OpenUrlInDefaultBrowser($"https://gw2profits.com/items.php?iid={stat.ApiId}");
            _gw2ProfitsMenuItem.Enabled = stat.StatType == StatType.Item;
            _gw2ProfitsMenuItem.BasicTooltipText = stat.StatType == StatType.Item
                ? "Open its 'GW2 Profits' page in your default browser"
                : NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP;
        }

        protected override void DisposeControl()
        {
            _generalHeaderMenuItem?.Dispose();
            _removeFavoriteMenuItem?.Dispose();
            _addFavoriteMenuItem?.Dispose();
            _ignoreMenuItem?.Dispose();
            _setCustomProfitMenuItem?.Dispose();
            _copyHeaderMenuItem?.Dispose();
            _copyNameMenuItem?.Dispose();
            _websitesHeaderMenuItem?.Dispose();
            _wikiMenuItem?.Dispose();
            _gw2EfficiencyTradingPostMenuItem?.Dispose();
            _gw2EfficiencyAccountMenuItem?.Dispose();
            _gw2BltcMenuItem?.Dispose();
            _gw2TreasuresMenuItem?.Dispose();
            _gw2TpMenuItem?.Dispose();
            _gw2ProfitsMenuItem?.Dispose();
            base.DisposeControl();
        }

        private static void SetToZeroProfitAndNavigateToProfitTab(Stat stat, SafeList<CustomStatProfit> customStatProfits, Services services)
        {
            var matchingCustomStatProfit = customStatProfits.ToListSafe().SingleOrDefault(c => c.BelongsToStat(stat));

            if (matchingCustomStatProfit != null) // custom stat already exists -> override its custom profit.
                matchingCustomStatProfit.Unsigned_CustomProfitInCopper = 0;
            else
            {
                var customStatProfit = new CustomStatProfit(stat.ApiId, stat.StatType);
                customStatProfits.AddSafe(customStatProfit);
            }

            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
            services.WindowTabSelector.SelectWindowTab(WindowTab.CustomProfit, WindowVisibility.Show);
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

        private readonly CustomContextMenuStripItem _generalHeaderMenuItem;
        private readonly CustomContextMenuStripItem _copyHeaderMenuItem;
        private readonly CustomContextMenuStripItem _websitesHeaderMenuItem;
        private readonly CustomContextMenuStripItem _wikiMenuItem;
        private readonly CustomContextMenuStripItem? _ignoreMenuItem;
        private readonly CustomContextMenuStripItem? _addFavoriteMenuItem;
        private readonly CustomContextMenuStripItem? _removeFavoriteMenuItem;
        private readonly CustomContextMenuStripItem? _setCustomProfitMenuItem;
        private readonly CustomContextMenuStripItem _gw2BltcMenuItem;
        private readonly CustomContextMenuStripItem _gw2TreasuresMenuItem;
        private readonly CustomContextMenuStripItem _gw2TpMenuItem;
        private readonly CustomContextMenuStripItem _gw2ProfitsMenuItem;
        private readonly CustomContextMenuStripItem _copyNameMenuItem;
        private readonly CustomContextMenuStripItem _gw2EfficiencyTradingPostMenuItem;
        private readonly CustomContextMenuStripItem _gw2EfficiencyAccountMenuItem;
        private const string NOT_AVAILABLE_FOR_CURRENCY_TOOLTIP = "Not available for currencies";
    }
}
