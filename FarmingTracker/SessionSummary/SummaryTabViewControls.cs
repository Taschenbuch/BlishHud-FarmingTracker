using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class SummaryTabViewControls
    {
        public SummaryTabViewControls(Model model, Services services)
        {
            RootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
            };

            DrfErrorLabel = new Label
            {
                Text = Constants.ZERO_HEIGHT_EMPTY_LABEL,
                Font = services.FontService.Fonts[FontSize.Size18],
                TextColor = Color.Yellow,
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = RootFlowPanel
            };

            OpenSettingsButton = new OpenSettingsButton("Open settings tab to setup DRF", services.FarmingTrackerWindow, RootFlowPanel);
            OpenSettingsButton.Hide();

            FarmingRootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = RootFlowPanel
            };

            var buttonFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = FarmingRootFlowPanel
            };

            CollapsibleHelp = new CollapsibleHelp(
                $"SETUP AND TROUBLESHOOTING:\n" +
                $"DRF setup instructions and DRF and module troubleshooting can be found in the '{Constants.TabTitles.SETTINGS}' tab.\n" +
                $"\n" +
                $"CONTEXT MENU:\n" +
                $"- right click an item/currency to open a context menu with additional features.\n" +
                $"- the context menu entries differs for items, favorites, currencies, coins.\n" +
                $"\n" +
                $"PROFIT:\n" +
                "- 15% trading post fee is already deducted.\n" +
                "- Profit also includes changes in 'raw gold'. In other words coins spent or gained. " +
                $"'raw gold' changes are also visible in the '{Constants.CURRENCIES_PANEL_TITLE}' panel.\n" +
                "- Lost items reduce the profit accordingly.\n" +
                "- Currencies are not included in the profit calculation (except 'raw gold').\n" +
                "- rough profit = raw gold + item count * tp sell price * 0.85 + ...for all items.\n" +
                "- When tp sell price does not exist, tp buy price will be used. " +
                "Vendor price will be used when it is higher than tp sell/buy price * 0.85.\n" +
                "- Module and DRF live tracking website profit calculation may differ because different profit formulas are used.\n" +
                $"- Profit per hour is updated every {Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS} seconds.\n" +
                $"- The profit is only a rough estimate because the trading post buy/sell prices can change over time and " +
                $"only the highest tp buy price and tp sell price for an item are considered. The tp buy/sell prices are a snapshot from " +
                $"when the item was tracked for the first time during a blish sesssion.\n" +
                $"\n" +
                $"RESIZE:\n" +
                $"- You can resize the window by dragging the bottom right window corner. " +
                $"- Some UI elements might be cut off when the window becomes too small.",
                300 - Constants.SCROLLBAR_WIDTH_OFFSET, // set dummy width because no buildPanel exists yet.
                buttonFlowPanel);

            var subButtonFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(5, 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = buttonFlowPanel
            };

            ResetButton = new StandardButton()
            {
                Text = "Reset",
                BasicTooltipText = "Start new farming session by resetting tracked items and currencies.",
                Width = 90,
                Parent = subButtonFlowPanel,
            };

            new OpenUrlInBrowserButton(
                "https://drf.rs/dashboard/livetracker/summary",
                "DRF",
                "Open DRF live tracking website in your default web browser.\n" +
                "The module and the DRF live tracking web page are both DRF clients. But they are independent of each other. " +
                "They do not synchronize the data they display. So one client may show less or more data dependend on when the client session started.",
                services.TextureService.OpenLinkTexture,
                subButtonFlowPanel)
            {
                Width = 60,
            };

            var csvExportButton = new StandardButton()
            {
                Text = "Export CSV",
                BasicTooltipText = $"Export tracked items and currencies to '{services.CsvFileExporter.ModuleFolderPath}\\<date-time>.csv'.\n" +
                "This feature can be used to import the tracked items/currencies in Microsoft Excel for example.",
                Width = 90,
                Parent = subButtonFlowPanel,
            };

            csvExportButton.Click += async (s, e) => await services.CsvFileExporter.ExportSummaryAsCsvFile(model);

            var timeAndHintFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(20, 0),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = FarmingRootFlowPanel
            };

            ElapsedFarmingTimeLabel = new ElapsedFarmingTimeLabel(services, timeAndHintFlowPanel);

            HintLabel = new Label
            {
                Text = Constants.FULL_HEIGHT_EMPTY_LABEL,
                Font = services.FontService.Fonts[FontSize.Size14],
                Width = 250, // prevents that when window width is small the empty label moves behind the elapsed time label causing the whole UI to move up.
                AutoSizeHeight = true,
                Parent = timeAndHintFlowPanel
            };

            ProfitPanels = new ProfitPanels(services, false, FarmingRootFlowPanel);
            SearchPanel = new SearchPanel(services, FarmingRootFlowPanel);
            StatsPanels = CreateStatsPanels(services, FarmingRootFlowPanel);
        }

        public FlowPanel RootFlowPanel { get; }
        public Label DrfErrorLabel { get; }
        public OpenSettingsButton OpenSettingsButton { get; }
        public FlowPanel FarmingRootFlowPanel { get; }
        public ProfitPanels ProfitPanels { get; }
        public SearchPanel SearchPanel { get; }
        public StatsPanels StatsPanels { get; }
        public ElapsedFarmingTimeLabel ElapsedFarmingTimeLabel { get; }
        public Label HintLabel { get; }
        public CollapsibleHelp CollapsibleHelp { get; }
        public StandardButton ResetButton { get; }

        private static StatsPanels CreateStatsPanels(Services services, Container parent)
        {
            var currenciesFilterIconPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var currenciesFlowPanel = new FlowPanel()
            {
                Title = Constants.CURRENCIES_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = currenciesFilterIconPanel
            };

            var favoriteItemsFlowPanel = new FlowPanel()
            {
                Title = Constants.FAVORITE_ITEMS_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var itemsFilterIconPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = parent
            };

            var itemsFlowPanel = new FlowPanel()
            {
                Title = Constants.ITEMS_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = itemsFilterIconPanel
            };

            var currencyFilterIcon = new ClickThroughImage(services.TextureService.FilterTabIconTexture, new Point(380, 3), currenciesFilterIconPanel);
            var itemsFilterIcon = new ClickThroughImage(services.TextureService.FilterTabIconTexture, new Point(380, 3), itemsFilterIconPanel);
            var statsPanels = new StatsPanels(currenciesFlowPanel, favoriteItemsFlowPanel, itemsFlowPanel, currencyFilterIcon, itemsFilterIcon);

            new HintLabel(statsPanels.CurrenciesFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");
            new HintLabel(statsPanels.FavoriteItemsFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");
            new HintLabel(statsPanels.ItemsFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");

            return statsPanels;
        }
    }
}
