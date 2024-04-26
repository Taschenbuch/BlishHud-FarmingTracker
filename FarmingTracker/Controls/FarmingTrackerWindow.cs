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

            Title = "Farming Tracker";
            Emblem = _windowEmblemTexture;
            SavesPosition = true;
            Id = "Ecksofa.FarmingTracker: FarmingTrackerWindow";
            Location = new Point(300, 300);
            Parent = GameService.Graphics.SpriteScreen;            
            CreateUi(flowPanelWidth, services);

            _timeSinceModuleStartStopwatch.Start();
        }

        protected override void DisposeControl()
        {
            _windowEmblemTexture?.Dispose();
            base.DisposeControl();
        }

        public void Update2(GameTime gameTime) // Update2() because Update() does not always update
        {
            _elapsedFarmingTimeLabel.UpdateTimeOncePerSecond();
            _updateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_updateLoop.UpdateIntervalEnded()) // todo sinnvolles intervall wählen. 2min? 5min? keine ahnung
            {
                _updateLoop.ResetRunningTime();
                _updateLoop.UseFarmingUpdateInterval();

                var apiToken = new ApiToken(_services.Gw2ApiManager);
                if (!apiToken.CanAccessApi)
                {
                    _errorHintVisible = true;
                    var apiTokenErrorMessage = apiToken.CreateApiTokenErrorTooltipText();
                    var isGivingBlishSomeTimeToGiveToken = _timeSinceModuleStartStopwatch.Elapsed.TotalSeconds < 20;
                    var loadingHintVisible = apiToken.ApiTokenMissing && isGivingBlishSomeTimeToGiveToken;

                    LogApiTokenErrorOnce(apiTokenErrorMessage, loadingHintVisible);

                    _hintLabel.Text = loadingHintVisible
                        ? "Loading..."
                        : $"{apiToken.CreateApiTokenErrorLabelText()} Retry every {UpdateLoop.WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS / 1000}s";

                    _hintLabel.BasicTooltipText = loadingHintVisible 
                        ? "" 
                        : apiTokenErrorMessage;

                    return;
                }

                if(_services.Drf.DrfConnectionStatus != DrfConnectionStatus.Connected) 
                {
                    _errorHintVisible = true;
                    _drfErrorLabel.Text = DrfConnectionStatusService.GetTrackerWindowDrfConnectionStatusText(_services.Drf.DrfConnectionStatus);
                    return;
                }

                if(_errorHintVisible)
                {
                    _errorHintVisible = false;
                    _drfErrorLabel.Text = NO_DRF_ERROR_TEXT;
                    _hintLabel.Text = "";
                    _hintLabel.BasicTooltipText = "";
                }

                if (!_isTrackStatsRunning)
                {
                    _isTrackStatsRunning = true;
                    Task.Run(async () =>
                    {
                        await TrackStats();
                        _isTrackStatsRunning = false;
                    });
                }
            }
        }

        private async Task TrackStats()
        {
            try
            {
                var drfMessages = _services.Drf.GetDrfMessages();
                drfMessages = Drf.RemoveInvalidMessages(drfMessages);
                if (drfMessages.Count == 0)
                    return;

                _hintLabel.Text = "updating..."; // todo loading spinner? vorsicht: dann müssen gw2 api error hints anders gelöscht werden
                await UpdateStats(drfMessages);
                UiUpdater.UpdateUi(_currencyById, _itemById, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel, _services);
                _hintLabel.Text = "";
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message);
                _updateLoop.UseRetryAfterApiFailureUpdateInterval();
                _hintLabel.Text = $"GW2 API error. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, "track items failed.");
                _hintLabel.Text = $"Module crash. :-("; // todo was tun?
            }
        }

        private void LogApiTokenErrorOnce(string apiTokenErrorMessage, bool loadingHintVisible)
        {
            if (loadingHintVisible)
                return;

            if (_oldApiTokenErrorTooltip != apiTokenErrorMessage)
                Module.Logger.Debug(apiTokenErrorMessage);

            _oldApiTokenErrorTooltip = apiTokenErrorMessage;
        }

        private async Task UpdateStats(List<DrfMessage> drfMessages)
        {      
            DrfResultAdder.UpdateCurrencyById(drfMessages, _currencyById);
            DrfResultAdder.UpdateItemById(drfMessages, _itemById);

            await _statDetailsSetter.SetDetailsFromApi(_currencyById, _itemById, _services.Gw2ApiManager);

            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_currencyById);
            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_itemById);

            CoinSplitter.ReplaceCoinWithGoldSilverCopper(_currencyById);
            Debug_LogItemsWithoutDetailsFromApi(); // todo weg
        }

        private void Debug_LogItemsWithoutDetailsFromApi()  // todo weg
        {
            // missing currency check ist jetzt in SetDetailsFromApi
            var missingItems = _itemById.Values.Where(c => c.NotFoundByApi).Select(i => i.ApiId).ToList();

            if (missingItems.Any())
                Module.Logger.Info("items      api MISS   " + string.Join(" ", missingItems));
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

            var drfErrorPanel = new Panel
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel,
            };

            _drfErrorLabel = new Label
            {
                Text = NO_DRF_ERROR_TEXT,
                Font = services.FontService.Fonts[FontSize.Size18],
                TextColor = Color.Yellow, 
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Left = 20,
                Parent = drfErrorPanel
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
       
                _elapsedFarmingTimeLabel.ResetTime();
                _hintLabel.Text = "";
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

            _elapsedFarmingTimeLabel = new ElapsedFarmingTimeLabel(services, _controlsFlowPanel);

            _hintLabel = new Label
            {
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

        private bool _isTrackStatsRunning;
        private ElapsedFarmingTimeLabel _elapsedFarmingTimeLabel;
        private Label _hintLabel;
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private readonly UpdateLoop _updateLoop = new UpdateLoop();
        private readonly Dictionary<int, Stat> _itemById = new Dictionary<int, Stat>();
        private readonly Dictionary<int, Stat> _currencyById = new Dictionary<int, Stat>();
        private FlowPanel _farmedCurrenciesFlowPanel;
        private FlowPanel _farmedItemsFlowPanel;
        private FlowPanel _rootFlowPanel;
        private Label _drfErrorLabel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private string _oldApiTokenErrorTooltip = string.Empty;
        private bool _errorHintVisible;
        private readonly StatDetailsSetter _statDetailsSetter = new StatDetailsSetter();
        private readonly Texture2D _windowEmblemTexture;
        private readonly Services _services;
        private const string NO_DRF_ERROR_TEXT = " "; // because empty string would collapse the label height
    }
}
