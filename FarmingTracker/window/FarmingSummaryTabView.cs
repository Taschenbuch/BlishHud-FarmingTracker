using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Blish_HUD.ContentService;

namespace FarmingTracker
{
    public class FarmingSummaryTabView : View
    {
        public FarmingSummaryTabView(int flowPanelWidth, Services services) 
        {
            _flowPanelWidth = flowPanelWidth;
            _services = services;
            _timeSinceModuleStartStopwatch.Restart();
        }

        protected override void Unload()
        {
            // todo ? events?
        }

        protected override void Build(Container buildPanel)
        {
            CreateUi(_flowPanelWidth, _services, buildPanel);
        }

        public void Update(GameTime gameTime)
        {
            _updateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_hasToResetStats) // at loop start to prevent that reset is delayed by drf or api issues
            {
                _hintLabel.Text = "resetting...";

                if (_isUpdateStatsRunning)
                    return; // prevents farming time updates and prevents hintText from being overriden

                _hasToResetStats = false;
                ResetStats();
                _elapsedFarmingTimeLabel.RestartTime();
                _resetButton.Enabled = true;
                return; // a reset is enough work for a single update loop iteration.
            }

            _profitService.UpdateProfitPerHourEveryFiveSeconds(_services.FarmingTimeStopwatch.Elapsed);
            _elapsedFarmingTimeLabel.UpdateTimeEverySecond();

            if (_updateLoop.UpdateIntervalEnded())
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
                    _drfErrorLabel.Text = Constants.EMPTY_LABEL;
                    _hintLabel.Text = "";
                    _hintLabel.BasicTooltipText = "";
                }

                if (!_isUpdateStatsRunning && !_hasToResetStats)
                {
                    _isUpdateStatsRunning = true;
                    Task.Run(async () =>
                    {
                        await UpdateStats();
                        _isUpdateStatsRunning = false;
                    });
                }
            }
        }

        private void ResetStats()
        {
            try
            {
                _services.Stats.ItemById.Clear();
                _services.Stats.CurrencyById.Clear();
                UiUpdater.UpdateStatsInUi(_statsPanels, _services);
                _profitService.ResetProfit();
                _lastStatsUpdateSuccessfull = true; // in case a previous update failed. Because that doesnt matter anymore after the reset.
                _hintLabel.Text = "";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(ResetStats)} failed.");
                _hintLabel.Text = $"Module crash. :-("; // todo was tun?
            }
        }

        private async Task UpdateStats() 
        {
            try
            {
                var drfMessages = _services.Drf.GetDrfMessages();
                if (drfMessages.IsEmpty() && _lastStatsUpdateSuccessfull)
                    return;

                _hintLabel.Text = "updating..."; // todo loading spinner? vorsicht: dann müssen gw2 api error hints anders gelöscht werden
                await UpdateStatsInModel(drfMessages);
                UiUpdater.UpdateStatsInUi(_statsPanels, _services);
                _profitService.UpdateProfit(_services.Stats, _services.FarmingTimeStopwatch.Elapsed);
                _lastStatsUpdateSuccessfull = true;
                _hintLabel.Text = "";
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message);
                _updateLoop.UseRetryAfterApiFailureUpdateInterval();
                _lastStatsUpdateSuccessfull = false;
                _hintLabel.Text = $"GW2 API error. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(UpdateStats)} failed.");
                _lastStatsUpdateSuccessfull = false;
                _hintLabel.Text = $"Module crash. :-("; // todo was tun?
            }
        }

        private void LogApiTokenErrorOnce(string apiTokenErrorMessage, bool loadingHintVisible)
        {
            if (loadingHintVisible)
                return;

            if (_oldApiTokenErrorTooltip != apiTokenErrorMessage)
                Module.Logger.Info(apiTokenErrorMessage);

            _oldApiTokenErrorTooltip = apiTokenErrorMessage;
        }

        private async Task UpdateStatsInModel(List<DrfMessage> drfMessages)
        {      
            DrfResultAdder.UpdateCurrencyById(drfMessages, _services.Stats.CurrencyById);
            DrfResultAdder.UpdateItemById(drfMessages, _services.Stats.ItemById);

            await _statDetailsSetter.SetDetailsFromApi(_services.Stats, _services.Gw2ApiManager);

            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_services.Stats.CurrencyById);
            IconAssetIdAndTooltipSetter.SetTooltipAndMissingIconAssetIds(_services.Stats.ItemById);

            CoinSplitter.SplitCoinIntoGoldSilverCopperStats(_services.Stats.CurrencyById);
            Debug_LogItemsWithoutDetailsFromApi(); // todo debug?
        }

        private void Debug_LogItemsWithoutDetailsFromApi()  // todo debug?
        {
            // missing currency check ist jetzt in SetDetailsFromApi
            var missingItems = _services.Stats.ItemById.Values.Where(c => c.NotFoundByApi).Select(i => i.ApiId).ToList();

            if (missingItems.Any())
                Module.Logger.Info("items      api MISS   " + string.Join(" ", missingItems));
        }

        private void CreateUi(int flowPanelWidth, Services services, Container buildPanel)
        {
            _rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = buildPanel,
            };

            var drfErrorPanel = new Panel
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel,
            };

            _drfErrorLabel = new Label
            {
                Text = Constants.EMPTY_LABEL,
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
                BasicTooltipText = "Start new farming session by resetting tracked items and currencies.",
                Width = 90,
                Left = 460,
                Parent = _rootFlowPanel,
            };

            _resetButton.Click += (s, e) =>
            {
                _resetButton.Enabled = false;
                _hasToResetStats = true;
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
                Font = services.FontService.Fonts[FontSize.Size14],
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = _controlsFlowPanel
            };

            var profitTooltip = 
                "Rough profit when selling everything to vendor or on trading post (listing).\n" +
                "15% trading post fee is already deducted.\n" +
                $"Profit per hour is updated every {Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS} seconds.";

            var font = services.FontService.Fonts[FontSize.Size16];
            var totalProfitPanel = new ProfitPanel("Profit", profitTooltip, font, _rootFlowPanel);
            var profitPerHourPanel = new ProfitPanel("Profit per hour", profitTooltip, font, _rootFlowPanel);
            _profitService = new ProfitService(totalProfitPanel, profitPerHourPanel);

            _statsPanels.FarmedCurrenciesFlowPanel = new FlowPanel()
            {
                Title = "Currencies",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            _statsPanels.FarmedItemsFlowPanel = new FlowPanel()
            {
                Title = "Items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = flowPanelWidth,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _rootFlowPanel
            };

            UiUpdater.UpdateStatsInUi(_statsPanels, _services);
        }

        private bool _isUpdateStatsRunning;
        private Label _hintLabel;
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private readonly UpdateLoop _updateLoop = new UpdateLoop();
        private ProfitService _profitService;
        private FlowPanel _rootFlowPanel;
        private Label _drfErrorLabel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private ElapsedFarmingTimeLabel _elapsedFarmingTimeLabel;
        private string _oldApiTokenErrorTooltip = string.Empty;
        private bool _errorHintVisible;
        private bool _lastStatsUpdateSuccessfull = true;
        private bool _hasToResetStats;
        private readonly StatDetailsSetter _statDetailsSetter = new StatDetailsSetter();
        private readonly int _flowPanelWidth;
        private readonly Services _services;
        private readonly StatsPanels _statsPanels = new StatsPanels();
    }
}
