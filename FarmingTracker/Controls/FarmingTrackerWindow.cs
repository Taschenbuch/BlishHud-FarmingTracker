using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class FarmingTrackerWindow : StandardWindow
    {
        public FarmingTrackerWindow(
            AsyncTexture2D background,
            Rectangle windowRegion, 
            Rectangle contentRegion, 
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
            CreateUi(flowPanelWidth, services);

            _timeSinceModuleStartStopwatch.Start();
        }

        public async Task InitAsync()
        {
            try
            {
                await _drfWebSocketClient.Connect("a886872e-d942-4766-b499-e5802359d93a", "wss://drf.rs/ws");
                _farmingTimeStopwatch.Restart(); // muss starten wenn drf verbunden ist.
            }
            catch (Exception e)
            {
                Module.Logger.Warn(e, "failed to connect to drf");
            }
        }

        protected override void DisposeControl()
        {
            _drfWebSocketClient?.Close(); // fire and forget, gehört nicht ins farming window. eher irgendwie automatisch im module handeln?
            _windowEmblemTexture?.Dispose();
            base.DisposeControl();
        }
 
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

                if (!_trackItemsIsRunning)
                {
                    _nextUpdateCountdownLabel.Text = "";
                    if (!_drfWebSocketClient.HasNewDrfMessages()) // does NOT ignore invalid messages. those are filtered somewhere else
                        return;

                    _nextUpdateCountdownLabel.Text = "updating...";
                    _trackItemsIsRunning = true;
                    Task.Run(() => TrackItems());
                }
            }
        }

        private async void TrackItems()
        {
            try
            {
                await UseDrfAndApiToDetermineFarmedItems();
                UiUpdater.UpdateUi(_currencyById, _itemById, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel, _services);
                _nextUpdateCountdownLabel.Text = "";
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message); // todo keine exception loggen? zu spammy?
                _updateLoop.UseRetryAfterApiFailureUpdateInterval();
                _nextUpdateCountdownLabel.Text = $"API error. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, "track items failed.");
                _nextUpdateCountdownLabel.Text = $"Module crash. :-("; // todo was tun?
            }
            finally
            {
                _updateLoop.ResetRunningTime();
                _trackItemsIsRunning = false;
            }
        }

        private async Task UseDrfAndApiToDetermineFarmedItems()
        {
            var drfMessages = _drfWebSocketClient.GetDrfMessages();
            drfMessages = Drf.RemoveInvalidMessages(drfMessages);

            if (drfMessages.Count == 0)
                return;

            DrfResultAdder.UpdateCurrencyById(drfMessages, _currencyById);
            DrfResultAdder.UpdateItemById(drfMessages, _itemById);

            await StatDetailsSetter.SetCurrencyDetailsFromApi(_currencyById, _services);
            await StatDetailsSetter.SetItemDetailsFromApi(_itemById, _services);

            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_currencyById);
            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_itemById);

            CoinSplitter.ReplaceCoinWithGoldSilverCopper(_currencyById); // todo fixen, dann wieder nutzen

            Debug_LogItemsWithoutDetailsFromApi(); // todo weg
        }

        private void Debug_LogItemsWithoutDetailsFromApi()  // todo weg
        {
            var missingCurrencies = _currencyById.Values.Where(c => c.NotFoundByApi).Select(i => i.ApiId).ToList();
            var missingItems = _itemById.Values.Where(c => c.NotFoundByApi).Select(i => i.ApiId).ToList();
            if (missingCurrencies.Any())
                Module.Logger.Info("currencies api MISS   " + string.Join(" ", missingCurrencies)); // todo weg

            if (missingItems.Any())
                Module.Logger.Info("items      api MISS   " + string.Join(" ", missingItems));
        }

        private void UpdateFarmingTimeLabelText(TimeSpan farmingTime)
        {
            _elapsedFarmingTimeLabel.Text = $"farming for {farmingTime:h':'mm':'ss}";
        }

        private void CreateUi(int flowPanelWidth, Services services)
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
                _updateLoop.TiggerUpdateInstantly(); // todo überflüssig?
                // todo items clear und UpdateUi() woanders hinschieben, ist hier falsch. poentielle racing conditions.
                // Es muss aber weiterhin instant die flowpanels clearen.
                _farmingTimeStopwatch.Restart();
                _oldFarmingTime = TimeSpan.Zero;
                UpdateFarmingTimeLabelText(TimeSpan.Zero);
                _nextUpdateCountdownLabel.Text = "";
                _itemById.Clear();
                _currencyById.Clear();
                UiUpdater.UpdateUi(_currencyById, _itemById, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel, _services);
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
                Font = services.FontService.Fonts[FontSize.Size18],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = _controlsFlowPanel
            };

            _nextUpdateCountdownLabel = new Label
            {
                Text = "",
                Font = services.FontService.Fonts[FontSize.Size18],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = _controlsFlowPanel
            };

            _farmedCurrenciesFlowPanel = new FlowPanel()
            {
                Title = "Currencies",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            _farmedItemsFlowPanel = new FlowPanel()
            {
                Title = "Items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };
        }

        private bool _trackItemsIsRunning;
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
        private FlowPanel _rootFlowPanel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private readonly Texture2D _windowEmblemTexture;
        private readonly DrfWebSocketClient _drfWebSocketClient = new DrfWebSocketClient();
        private readonly Services _services;
    }
}
