using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System.Linq;

namespace FarmingTracker
{
    public class CustomStatProfitTabView : View
    {
        public CustomStatProfitTabView(Model model, Services services)
        {
            _model = model;
            _services = services;
        }

        protected override void Unload()
        {
            _rootFlowPanel?.Dispose();
            _rootFlowPanel = null;
            base.Unload();
        }

        protected override void Build(Container buildPanel)
        {
            _rootFlowPanel = new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel
            };

            var collapsibleHelp = new CollapsibleHelp(
                $"xxxx", // todo x
                buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET, // buildPanel because other Panels dont have correctly updated width yet.
                _rootFlowPanel);

            buildPanel.ContentResized += (s, e) =>
            {
                collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET);
            };

            var hintLabel = new HintLabel(_rootFlowPanel, Constants.ZERO_HEIGHT_EMPTY_LABEL);

            var customStatProfits = _model.CustomStatProfits.ToListSafe();
            var noCustomSatProfitsExist = customStatProfits.IsEmpty();
            if (noCustomSatProfitsExist)
            {
                ShowNoCustomStatProfitsExistHintIfNecessary(hintLabel, _model);
                return;
            }

            var statsSnapshot = _model.StatsSnapshot;
            var currencies = statsSnapshot.CurrencyById.Values.Where(c => !c.IsCoin).ToList();
            var items = statsSnapshot.ItemById.Values;

            var currenciesApiDataMissing = currencies.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            var itemApiDataMissing = items.Any(i => i.Details.State == ApiStatDetailsState.MissingBecauseApiNotCalledYet);
            if (currenciesApiDataMissing || itemApiDataMissing)
            {
                ShowLoadingHint(hintLabel);
                return;
            }

            var statsFlowPanel = CreateStatsFlowPanel(buildPanel);

            foreach (var customStatProfit in customStatProfits)
            {
                if(customStatProfit.StatType == StatType.Unknown)
                {
                    Module.Logger.Error($"unknown customStatProfit type for this ApiId: {customStatProfit.ApiId}");
                    continue;
                }

                var stats = customStatProfit.StatType == StatType.Item
                    ? items
                    : currencies;

                var stat = stats.SingleOrDefault(s => s.ApiId == customStatProfit.ApiId && s.StatType == customStatProfit.StatType);

                if (stat == null)
                {
                    Module.Logger.Error($"Missing stat in model for customStatprofit id: {customStatProfit.ApiId}");
                    continue;
                }

                CreateCustomStatProfitRow(customStatProfit, stat, hintLabel, _model, _services, statsFlowPanel);
            }
        }

        private FlowPanel CreateStatsFlowPanel(Container buildPanel)
        {
            return new FlowPanel
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(0, 5),
                OuterControlPadding = new Vector2(0, 5),
                Width = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };
        }

        private static void CreateCustomStatProfitRow( // todo x als eigenes control extracten.
            CustomStatProfit customStatProfit, 
            Stat stat, 
            HintLabel hintLabel, 
            Model model, 
            Services services, 
            Container parent)
        {
            var iconSize = 50;
            var iconMargin = 1;
            var backgroundSize = iconSize + 2 * iconMargin;
            var backgroundMargin = 5;
            var panelHeight = backgroundSize + 2 * backgroundMargin;

            var statRowPanel = new Panel
            {
                BackgroundColor = Color.Black * 0.4f,
                Width = 450, // to have some room for long german/french names.
                Height = panelHeight,
                Parent = parent
            };

            // inventory slot background
            new Image(services.TextureService.InventorySlotBackgroundTexture)
            {
                Location = new Point(backgroundMargin),
                BasicTooltipText = stat.Details.Name,
                Size = new Point(backgroundSize),
                Parent = statRowPanel,
            };

            var statImage = new Image(services.TextureService.GetTextureFromAssetCacheOrFallback(stat.Details.IconAssetId))
            {
                Location = new Point(backgroundMargin + iconMargin),
                BasicTooltipText = stat.Details.Name,
                Size = new Point(iconSize),
                Parent = statRowPanel
            };

            var statNameLabel = new Label()
            {
                Location = new Point(statImage.Right + 10, backgroundMargin + iconMargin),
                Text = stat.Details.Name,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = statRowPanel
            };

            var goldTextBox = new NumberTextBox()
            {
                Location = new Point(statImage.Right + 10, statNameLabel.Bottom + 5),
                Text = customStatProfit.CustomProfitInCopper.ToString(), // todo x
                Width = 60, 
                Parent = statRowPanel
            };

            var goldCoinImage = new Image(services.TextureService.SmallGoldCoinTexture)
            {
                Location = new Point(goldTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(goldTextBox.Height),
                Parent = statRowPanel,
            };

            var silverTextBox = new NumberTextBox(2)
            {
                Location = new Point(goldCoinImage.Right + 10, statNameLabel.Bottom + 5),
                Text = customStatProfit.CustomProfitInCopper.ToString(), // todo x
                Width = 35,
                Parent = statRowPanel
            };

            var silverCoinImage = new Image(services.TextureService.SmallSilverCoinTexture)
            {
                Location = new Point(silverTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(silverTextBox.Height),
                Parent = statRowPanel,
            };

            var copperTextBox = new NumberTextBox(2)
            {
                Location = new Point(silverCoinImage.Right + 10, statNameLabel.Bottom + 5),
                Text = customStatProfit.CustomProfitInCopper.ToString(), // todo x
                Width = 35,
                Parent = statRowPanel
            };

            // copper icon
            new Image(services.TextureService.SmallCopperCoinTexture) 
            {
                Location = new Point(copperTextBox.Right, statNameLabel.Bottom + 5),
                Size = new Point(copperTextBox.Height),
                Parent = statRowPanel,
            };

            var removeButton = new StandardButton
            {
                Text = "x",
                Width = 28,
                Parent = statRowPanel
            };
            
            removeButton.Location = new Point(statRowPanel.Width - removeButton.Width - 5, (panelHeight - removeButton.Height) / 2);
            removeButton.Click += (s, e) => 
            { 
                RemoveCustomStatProfit(customStatProfit, model, services);
                statRowPanel?.Dispose();
                ShowNoCustomStatProfitsExistHintIfNecessary(hintLabel, model);
            };
        }

        private static void RemoveCustomStatProfit(CustomStatProfit customStatProfit, Model model, Services services) 
        {
            model.CustomStatProfits.RemoveSafe(customStatProfit);
            services.UpdateLoop.TriggerUpdateUi();
            services.UpdateLoop.TriggerSaveModel();
        }

        private static void ShowNoCustomStatProfitsExistHintIfNecessary(HintLabel hintLabel, Model model)
        {
            if (model.CustomStatProfits.AnySafe())
                return;

            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}No custom profits are set.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}You can set custom profits by right clicking an item/currency in the '{FarmingTrackerWindow.SUMMARY_TAB_TITLE}' tab.";
        }

        private static void ShowLoadingHint(HintLabel hintLabel)
        {
            hintLabel.Text =
                $"{Constants.HINT_IN_PANEL_PADDING}This tab will not refresh automatically.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Go to '{FarmingTrackerWindow.SUMMARY_TAB_TITLE}' tab and " +
                $"wait until the '{Constants.UPDATING_HINT_TEXT}' hint disappears.\n" +
                $"{Constants.HINT_IN_PANEL_PADDING}Then come back here.";
        }

        private readonly Model _model;
        private readonly Services _services;
        private FlowPanel _rootFlowPanel;
    }
}
