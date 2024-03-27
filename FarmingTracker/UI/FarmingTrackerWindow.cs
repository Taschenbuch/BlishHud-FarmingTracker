using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class FarmingTrackerWindow : StandardWindow
    {
        public FarmingTrackerWindow(
            AsyncTexture2D background,
            Microsoft.Xna.Framework.Rectangle windowRegion, 
            Microsoft.Xna.Framework.Rectangle contentRegion, 
            int flowPanelWidth, 
            Services services) 
            : base(background, windowRegion, contentRegion)
        {
            _services = services;
            _windowEmblemTexture = services.ContentsManager.GetTexture(@"windowEmblem.png");

            Title = "Farming Tracker (Beta)"; // todo remove "beta"
            Emblem = _windowEmblemTexture;
            SavesPosition = true;
            Id = "Ecksofa.FarmingTracker: error window";
            Location = new Point(300, 300);
            Parent = GameService.Graphics.SpriteScreen;
            CreateUi(flowPanelWidth);

            _timeSinceModuleStartStopwatch.Start();
        }

        public async Task InitAsync()
        {
            try
            {
                await _drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a");
                _farmingTimeStopwatch.Restart(); // muss starten wenn drf verbunden ist.
            }
            catch (Exception e)
            {
                Module.Logger.Warn(e, "failed to connect to drf");
            }
        }

        protected override void DisposeControl()
        {
            _drfWebSocketClient?.Close(); // fire and forget
            _windowEmblemTexture?.Dispose();
            base.DisposeControl();
        }
 
        public bool TrackItemsIsRunning { get; set; } // todo wieder private field machen

        public void Update2(GameTime gameTime) // Update2() because Update() does not always update
        {
            var farmingTime = _farmingTimeStopwatch.Elapsed;
            if (farmingTime >= _oldFarmingTime + ONE_SECOND)
            {
                // todo next update time vs elappsed farming time has 1 second difference
                _oldFarmingTime = farmingTime;
                UpdateFarmingTimeLabelText(farmingTime);
            }

            _updateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_updateLoop.UpdateIntervalEnded()) // todo sinnvolles intervall wählen. 2min? 5min? keine ahnung
            {
                _updateLoop.ResetRunningTime();
                _updateLoop.UseFarmingUpdateInterval();

                var apiToken = new ApiToken(_services.Gw2ApiManager);
                if (!apiToken.CanAccessApi)
                {
                    var apiTokenErrorTooltip = apiToken.CreateApiTokenErrorTooltipText();
                    var isGivingBlishSomeTimeToGiveToken = (apiToken.ApiTokenState == ApiTokenState.ApiTokenMissing) && (_timeSinceModuleStartStopwatch.Elapsed.TotalSeconds < 20);
                    _nextUpdateCountdownLabel.Text = isGivingBlishSomeTimeToGiveToken
                        ? LOADING_HINT_TEXT
                        : $"{apiToken.CreateApiTokenErrorLabelText()} Retry every {UpdateLoop.WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS / 1000}s";
                    _nextUpdateCountdownLabel.BasicTooltipText = isGivingBlishSomeTimeToGiveToken ? "" : apiTokenErrorTooltip;
                    if (!isGivingBlishSomeTimeToGiveToken)
                        Module.Logger.Debug(apiTokenErrorTooltip);

                    _updateLoop.UseWaitForApiTokenUpdateInterval();
                    return;
                }
                _nextUpdateCountdownLabel.BasicTooltipText = "";
                if (_nextUpdateCountdownLabel.Text == LOADING_HINT_TEXT) // todo blöd, dass das jedes mal geprüft wird aber nur 1x beim start nötig ist
                    _nextUpdateCountdownLabel.Text = "";

                if (!TrackItemsIsRunning)
                {
                    if (!_drfWebSocketClient.HasNewDrfMessages()) // does NOT ignore invalid messages. those are filtered somewhere else
                        return;

                    _nextUpdateCountdownLabel.Text = "updating...";
                    TrackItemsIsRunning = true;
                    Task.Run(() => TrackItems());
                }
            }
        }

        public async void TrackItems()
        {
            try
            {
                await UseDrfAndApiToDetermineFarmedItems();
                UiUpdater.UpdateUi(_currencyById, _itemById, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                _nextUpdateCountdownLabel.Text = "";
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message); // todo keine exception loggen? zu spammy?
                _updateLoop.UseRetryAfterApiFailureUpdateInterval();
                _nextUpdateCountdownLabel.Text = $"API error. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s (TODO: display countdown)"; // todo countdown
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, "track items failed.");
                _nextUpdateCountdownLabel.Text = $"Module crash. :-("; // todo was tun?
            }
            finally
            {
                _updateLoop.ResetRunningTime();
                TrackItemsIsRunning = false;
            }
        }

        private async Task UseDrfAndApiToDetermineFarmedItems()
        {
            var drfMessages = _drfWebSocketClient.GetDrfMessages();
            drfMessages = Drf.RemoveInvalidMessages(drfMessages);

            if (drfMessages.Count == 0)
                return;

            DrfSearcher.GetItemById(drfMessages, _itemById);
            DrfSearcher.GetCurrencyById(drfMessages, _currencyById);

            var currenciesWithoutDetails = _currencyById.Values.Where(c => c.IconAssetId == 0).Where(c => c.ApiId != OBSOLETE_GLORY_CURRENCY_ID).ToList();
            if (currenciesWithoutDetails.Any())
            {
                Module.Logger.Info("currencies no AssetID " + string.Join(" ,", currenciesWithoutDetails.Select(c => c.ApiId))); // todo weg
                IReadOnlyList<Currency> apiCurrencies;

                try
                {
                    apiCurrencies = await _services.Gw2ApiManager.Gw2ApiClient.V2.Currencies.ManyAsync(currenciesWithoutDetails.Select(c => c.ApiId));
                    Module.Logger.Info("apiCurrencies         " + string.Join(" ,", apiCurrencies.Select(c => c.Id))); // todo weg
                }
                catch (Exception e)
                {
                    throw new Gw2ApiException("API error: update currencies", e);
                }

                foreach (var apiCurrency in apiCurrencies)
                {
                    var currency = _currencyById[apiCurrency.Id];
                    currency.Name = apiCurrency.Name;
                    currency.Description = apiCurrency.Description;
                    currency.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiCurrency.Icon.Url.AbsoluteUri));
                }
            }

            var itemsWithoutDetails = _itemById.Values.Where(i => i.IconAssetId == 0).Where(c => c.ApiId != MISSING_YELLOW_ENTIAN_FLOWER_ITEM_ID).ToList();
            if (itemsWithoutDetails.Any())
            {
                Module.Logger.Info("items no AssetID      " + string.Join(" ,", itemsWithoutDetails.Select(c => c.ApiId))); // todo weg
                IReadOnlyList<Item> apiItems;

                try
                {
                    apiItems = await _services.Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(itemsWithoutDetails.Select(c => c.ApiId));
                    Module.Logger.Info("apiItems              " + string.Join(" ,", apiItems.Select(c => c.Id))); // todo weg
                }
                catch (Exception e)
                {
                    throw new Gw2ApiException("API error: update items", e);
                }

                foreach (var apiItem in apiItems)
                {
                    var currency = _itemById[apiItem.Id];
                    currency.Name = apiItem.Name;
                    currency.Description = apiItem.Description;
                    currency.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiItem.Icon.Url.AbsoluteUri));
                }
            }

            //CurrencySearcher.ReplaceCoinItemWithGoldSilverCopperItems(_currencyById); // todo fixen, dann wieder nutzen
            var c = _currencyById.Values.Where(c => c.IconAssetId == 0).Select(i => i.ApiId).ToList(); // todo weg
            var i = _itemById.Values.Where(c => c.IconAssetId == 0).Select(i => i.ApiId).ToList(); // todo weg
            if (c.Any())
                Module.Logger.Info("NOT FOUND WITH API currencies: " + string.Join(" ,", c)); // todo weg

            if (i.Any())
                Module.Logger.Info("NOT FOUND WITH API items:      " + string.Join(" ,", i)); // todo weg
        }

        private void UpdateFarmingTimeLabelText(TimeSpan farmingTime)
        {
            _elapsedFarmingTimeLabel.Text = $"farming for {farmingTime:h':'mm':'ss}";
        }

        private void CreateUi(int flowPanelWidth)
        {
            _rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = this,
            };

            _resetButton = new StandardButton()
            {
                Text = "Reset",
                BasicTooltipText = "Start new farming session by resetting farmed items and currencies.",
                Width = 90,
                Left = 460,
                Parent = _rootFlowPanel,
            };

            _resetButton.Click += (s, e) =>
            {
                _resetButton.Enabled = false;
                _updateLoop.TiggerUpdateInstantly();
                // todo items clear und UpdateUi() woanders hinschieben, ist hier falsch. poentielle racing conditions.
                // Es muss aber weiterhin instant die flowpanels clearen.
                _farmingTimeStopwatch.Restart();
                _oldFarmingTime = TimeSpan.Zero;
                UpdateFarmingTimeLabelText(TimeSpan.Zero);
                _nextUpdateCountdownLabel.Text = "";
                _itemById.Clear();
                _currencyById.Clear();
                UiUpdater.UpdateUi(_currencyById, _itemById, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                _resetButton.Enabled = true;
            };

            _controlsFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(20, 0),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            _elapsedFarmingTimeLabel = new Label
            {
                Text = "farming for -:--:--", // todo getElapsedTimeDisplayText() oder so, weil an vielen stellen vorhanden 
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size18, FontStyle.Regular),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = _controlsFlowPanel
            };

            _nextUpdateCountdownLabel = new Label
            {
                Text = "",
                Font = GameService.Content.GetFont(FontFace.Menomonia, FontSize.Size18, FontStyle.Regular),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = _controlsFlowPanel
            };

            _farmedCurrenciesFlowPanel = new FlowPanel()
            {
                Title = "Currencies",
                FlowDirection = ControlFlowDirection.LeftToRight,
                OuterControlPadding = new Vector2(15, 10),
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            _farmedItemsFlowPanel = new FlowPanel()
            {
                Title = "Items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                OuterControlPadding = new Vector2(15, 10),
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };
        }

        private Label _elapsedFarmingTimeLabel;
        private Label _nextUpdateCountdownLabel;
        private readonly Stopwatch _farmingTimeStopwatch = new Stopwatch(); // extract farming time and next update
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private readonly UpdateLoop _updateLoop = new UpdateLoop();
        private TimeSpan _oldFarmingTime;
        private readonly TimeSpan ONE_SECOND = TimeSpan.FromSeconds(1);
        private const string LOADING_HINT_TEXT = "Loading...";
        private readonly Dictionary<int, ItemX> _itemById = new Dictionary<int, ItemX>();
        private readonly Dictionary<int, ItemX> _currencyById = new Dictionary<int, ItemX>();
        private FlowPanel _farmedCurrenciesFlowPanel;
        private FlowPanel _farmedItemsFlowPanel;
        private const int OBSOLETE_GLORY_CURRENCY_ID = 17; // workaround for drf test data.
        private const int MISSING_YELLOW_ENTIAN_FLOWER_ITEM_ID = 17; // workaround for drf test data.
        private FlowPanel _rootFlowPanel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private readonly Texture2D _windowEmblemTexture;
        private readonly DrfWebSocketClient _drfWebSocketClient = new DrfWebSocketClient();
        private readonly Services _services;
    }
}
