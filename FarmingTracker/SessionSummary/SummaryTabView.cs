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
    public class SummaryTabView : View, IDisposable
    {
        public SummaryTabView(FarmingTrackerWindowService farmingTrackerWindowService, Services services) 
        {
            _services = services;
            _rootFlowPanel = CreateUi(farmingTrackerWindowService, _services);
            _timeSinceModuleStartStopwatch.Restart();
            services.UpdateLoop.TriggerUpdateStats();
            services.SettingService.RarityIconBorderIsVisibleSetting.SettingChanged += OnRarityIconBorderVisibleSettingChanged;
        }

        public void Dispose()
        {
            _services.SettingService.RarityIconBorderIsVisibleSetting.SettingChanged -= OnRarityIconBorderVisibleSettingChanged;
        }

        protected override void Unload()
        {
            // NOOP because CreateUi() is called in ctor instead of Build()
        }

        protected override void Build(Container buildPanel)
        {
            _rootFlowPanel.Parent = buildPanel;
            buildPanel.ContentResized += (s,e) =>
            {
                var width = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
                _statsPanels.FarmedCurrenciesFlowPanel.Width = width;
                _statsPanels.FarmedItemsFlowPanel.Width = width;
                _statsPanels.ItemsFilterIcon.SetLeft(width);
                _statsPanels.CurrencyFilterIcon.SetLeft(width);
                _searchPanel.UpdateSize(width);
            };
        }

        public void Update(GameTime gameTime)
        {
            _services.UpdateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);
            _saveModelRunningTimeMs += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_services.UpdateLoop.HasToUpdateUi())
            {
                _profitService.UpdateProfit(_services.Model, _services.Model.FarmingDuration.Elapsed);
                UiUpdater.UpdateStatPanels(_statsPanels, _services);
                return; // that is enough work for a single update loop iteration.
            }

            if (HasToPerformAutomaticReset())
                _hasToResetStats = true;

            if (_hasToResetStats) // at loop start to prevent that reset is delayed by drf or api issues or hintLabel is overriden by api issues
            {
                _hintLabel.Text = $"{Constants.RESETTING_HINT_TEXT} (this may take a few seconds)";

                if (!_isTaskRunning) // prevents that reset and update modify stats at the same time
                {
                    _hasToResetStats = false;
                    ResetStats();
                    _services.UpdateLoop.TriggerUpdateUi();
                    _services.UpdateLoop.TriggerSaveModel();
                    _elapsedFarmingTimeLabel.RestartTime();
                    _resetButton.Enabled = true;
                }
                return; // that is enough work for a single update loop iteration. And prevents farming time updates and prevents hintText from being overriden.
            }

            if(!_isTaskRunning)
            {
                // save in certain intervals because farming duration has to be saved too.
                if (_services.UpdateLoop.HasToSaveModel() || _saveModelRunningTimeMs > SAVE_MODEL_INTERVAL_MS)
                {
                    _saveModelRunningTimeMs = 0;
                    _isTaskRunning = true;
                    Task.Run(async () =>
                    {
                        await _services.FileSaveService.SaveModelToFile(_services.Model);
                        _isTaskRunning = false;
                    });
                    // do not return here because saving the model should not disturb other parts of Update().
                }
            }
            
            _profitService.UpdateProfitPerHourEveryFiveSeconds(_services.Model.FarmingDuration.Elapsed);
            _elapsedFarmingTimeLabel.UpdateTimeEverySecond();

            if (_services.UpdateLoop.UpdateIntervalEnded()) // todo guard stattdessen?
            {
                _services.UpdateLoop.ResetRunningTime();
                _services.UpdateLoop.UseFarmingUpdateInterval();

                ShowOrHideDrfErrorLabelAndStatPanels(_services.Drf.DrfConnectionStatus, _drfErrorLabel, _openSettingsButton, _farmingRootFlowPanel);

                var apiToken = new ApiToken(_services.Gw2ApiManager);
                ShowOrHideApiErrorHint(apiToken, _hintLabel, _timeSinceModuleStartStopwatch.Elapsed.TotalSeconds);
                if (!apiToken.CanAccessApi)
                    return; // dont continue to prevent api error hint being overriden by "update..." etc.

                if (!_isTaskRunning)
                {
                    _isTaskRunning = true;
                    Task.Run(async () =>
                    {
                        await UpdateStats();
                        _isTaskRunning = false;
                    });
                }
            }
        }

        private bool HasToPerformAutomaticReset()
        {
            var isModuleStart = _isModuleStartForReset;
            _isModuleStartForReset = false;

            return _services.SettingService.AutomaticResetSetting.Value switch
            {
                AutomaticReset.OnModuleStart => isModuleStart,
                _ => false, // includes .Never
            };
        }

        private void ResetStats()
        {
            try
            {
                StatsService.ResetCounts(_services.Model.ItemById);
                StatsService.ResetCounts(_services.Model.CurrencyById);
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
                var hasToUpdateStats = drfMessages.Any() || !_lastStatsUpdateSuccessfull || _services.UpdateLoop.HasToUpdateStats();
                if (!hasToUpdateStats)
                    return;

                _hintLabel.Text = $"{Constants.UPDATING_HINT_TEXT} (this may take a few seconds)";
                await UpdateStatsInModel(drfMessages);
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                _lastStatsUpdateSuccessfull = true;
                _hintLabel.Text = "";
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message);
                _services.UpdateLoop.UseRetryAfterApiFailureUpdateInterval();
                _lastStatsUpdateSuccessfull = false;
                _hintLabel.Text = $"{GW2_API_ERROR_HINT}. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(UpdateStats)} failed.");
                _lastStatsUpdateSuccessfull = false;
                _hintLabel.Text = $"Module crash. :-("; // todo was tun?
            }
        }

        private static void ShowOrHideDrfErrorLabelAndStatPanels(
            DrfConnectionStatus drfConnectionStatus,
            Label drfErrorLabel,
            OpenSettingsButton openSettingsButton,
            FlowPanel farmingRootFlowPanel)
        {
            drfErrorLabel.Text = drfConnectionStatus == DrfConnectionStatus.Connected
                ? Constants.ZERO_HEIGHT_EMPTY_LABEL
                : DrfConnectionStatusService.GetSummaryTabDrfConnectionStatusText(drfConnectionStatus);

            if (drfConnectionStatus == DrfConnectionStatus.AuthenticationFailed)
            {
                openSettingsButton.Show();
                farmingRootFlowPanel.Hide();
            }
            else
            {
                openSettingsButton.Hide();
                farmingRootFlowPanel.Show();
            }
        }

        private void ShowOrHideApiErrorHint(ApiToken apiToken, Label hintLabel, double timeSinceModuleStartInSeconds)
        {
            if (!apiToken.CanAccessApi)
            {
                _apiErrorHintVisible = true;
                var apiTokenErrorMessage = apiToken.CreateApiTokenErrorTooltipText();
                var isGivingBlishSomeTimeToGiveToken = timeSinceModuleStartInSeconds < 20;
                var loadingHintVisible = apiToken.ApiTokenMissing && isGivingBlishSomeTimeToGiveToken;

                LogApiTokenErrorOnce(apiTokenErrorMessage, loadingHintVisible);

                hintLabel.Text = loadingHintVisible
                    ? "Loading... (this may take a few seconds)"
                    : $"{apiToken.CreateApiTokenErrorLabelText()} Retry every {UpdateLoop.WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS / 1000}s";

                hintLabel.BasicTooltipText = loadingHintVisible
                    ? ""
                    : apiTokenErrorMessage;

                return;
            }

            if (_apiErrorHintVisible) // only reset hintLabel when api error hint is currently visible because. This prevents overriding other hints
            {
                _apiErrorHintVisible = false;
                hintLabel.Text = "";
                hintLabel.BasicTooltipText = "";
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
            DrfResultAdder.UpdateCurrencyById(drfMessages, _services.Model.CurrencyById);
            DrfResultAdder.UpdateItemById(drfMessages, _services.Model.ItemById);
            await _statDetailsSetter.SetDetailsAndProfitFromApi(_services.Model, _services.Gw2ApiManager);
        }

        private FlowPanel CreateUi(FarmingTrackerWindowService farmingTrackerWindowService, Services services)
        {
            var rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
            };

            _drfErrorLabel = new Label
            {
                Text = Constants.ZERO_HEIGHT_EMPTY_LABEL,
                Font = services.FontService.Fonts[FontSize.Size18],
                TextColor = Color.Yellow,
                StrokeText = true,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                Parent = rootFlowPanel
            };

            _openSettingsButton = new OpenSettingsButton("Open settings tab to setup DRF", farmingTrackerWindowService, rootFlowPanel);
            _openSettingsButton.Hide();

            _farmingRootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
            };

            _resetButton = new StandardButton()
            {
                Text = "Reset",
                BasicTooltipText = "Start new farming session by resetting tracked items and currencies.",
                Width = 90,
                Left = 460,
                Parent = _farmingRootFlowPanel,
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
                Parent = _farmingRootFlowPanel
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
                "Profit also includes changes in 'raw gold'. In other words coins spent or gained.\n" +
                "'raw gold' changes are also visible in the currency panel below.\n" +
                "Lost items reduce the profit accordingly.\n" +
                "Module and DRF website profit calculation may differ because different algorithms are used.\n" +
                $"Profit per hour is updated every {Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS} seconds.";

            var font = services.FontService.Fonts[FontSize.Size16];
            var totalProfitPanel = new ProfitPanel("Profit", profitTooltip, font, _services, _farmingRootFlowPanel);
            var profitPerHourPanel = new ProfitPanel("Profit per hour", profitTooltip, font, _services, _farmingRootFlowPanel);
            _profitService = new ProfitService(totalProfitPanel, profitPerHourPanel);

            _searchPanel = new SearchPanel(_services, _farmingRootFlowPanel);

            var currenciesFilterIconPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _farmingRootFlowPanel
            };

            var itemsFilterIconPanel = new Panel
            {
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _farmingRootFlowPanel
            };

            _statsPanels.FarmedCurrenciesFlowPanel = new FlowPanel()
            {
                Title = "Currencies",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = currenciesFilterIconPanel
            };

            _statsPanels.FarmedItemsFlowPanel = new FlowPanel()
            {
                Title = "Items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = itemsFilterIconPanel
            };

            _statsPanels.CurrencyFilterIcon = new ClickThroughImage(services.TextureService.FilterTabIconTexture, new Point(380, 3), currenciesFilterIconPanel);
            _statsPanels.ItemsFilterIcon = new ClickThroughImage(services.TextureService.FilterTabIconTexture, new Point(380, 3), itemsFilterIconPanel);

            new HintLabel(_statsPanels.FarmedCurrenciesFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");
            new HintLabel(_statsPanels.FarmedItemsFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");

            return rootFlowPanel;
        }

        private void OnRarityIconBorderVisibleSettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            _services.UpdateLoop.TriggerUpdateUi();
        }

        private bool _isTaskRunning;
        private Label _hintLabel;
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private ProfitService _profitService;
        private SearchPanel _searchPanel;
        private readonly FlowPanel _rootFlowPanel;
        private Label _drfErrorLabel;
        private OpenSettingsButton _openSettingsButton;
        private FlowPanel _farmingRootFlowPanel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private ElapsedFarmingTimeLabel _elapsedFarmingTimeLabel;
        private string _oldApiTokenErrorTooltip = string.Empty;
        private bool _apiErrorHintVisible;
        private bool _lastStatsUpdateSuccessfull = true;
        private bool _hasToResetStats;
        private bool _isModuleStartForReset = true;
        private readonly StatDetailsSetter _statDetailsSetter = new StatDetailsSetter();
        private readonly Services _services;
        private readonly StatsPanels _statsPanels = new StatsPanels();
        private double _saveModelRunningTimeMs;
        private readonly double SAVE_MODEL_INTERVAL_MS = TimeSpan.FromMinutes(1).TotalMilliseconds;
        public const string GW2_API_ERROR_HINT = "GW2 API error";
    }
}
