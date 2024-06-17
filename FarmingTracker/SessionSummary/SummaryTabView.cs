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
            var resetAndDrfButtonsOffset = 70;
            _collapsibleHelp.UpdateSize(buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET - resetAndDrfButtonsOffset);

            buildPanel.ContentResized += (s,e) =>
            {
                var width = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
                _statsPanels.CurrenciesFlowPanel.Width = width;
                _statsPanels.ItemsFlowPanel.Width = width;
                _statsPanels.ItemsFilterIcon.SetLeft(width);
                _statsPanels.CurrencyFilterIcon.SetLeft(width);
                _searchPanel.UpdateSize(width);
                _collapsibleHelp.UpdateSize(e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET - resetAndDrfButtonsOffset);
            };
        }

        public void Update(GameTime gameTime)
        {
            _services.UpdateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);
            _saveModelRunningTimeMs += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_services.UpdateLoop.HasToUpdateUi())
            {
                var snapshot = _services.Model.StatsSnapshot;
                var items = snapshot.ItemById.Values.Where(s => s.Count != 0).ToList();
                var currencies = snapshot.CurrencyById.Values.Where(s => s.Count != 0).ToList();

                _profitPanels.UpdateProfit(snapshot, _services.Model.IgnoredItemApiIds, _services.Model.FarmingDuration.Elapsed);
                UiUpdater.UpdateStatPanels(_statsPanels, snapshot, _services);
                return; // that is enough work for a single update loop iteration.
            }

            if (HasToPerformAutomaticReset())
                _resetState = ResetState.ResetRequired;

            if (_resetState != ResetState.NoResetRequired) // at loop start to prevent that reset is delayed by drf or api issues or hintLabel is overriden by api issues
            {
                _hintLabel.Text = $"{Constants.RESETTING_HINT_TEXT} (this may take a few seconds)";

                if (!_isTaskRunning) // prevents that reset and update modify stats at the same time
                {
                    _resetState = ResetState.Resetting;
                    Task.Run(() =>
                    {
                        ResetStats();
                        _elapsedFarmingTimeLabel.RestartTime();
                        _services.UpdateLoop.TriggerUpdateUi();
                        _services.UpdateLoop.TriggerSaveModel();
                        _resetButton.Enabled = true;
                        _resetState = ResetState.NoResetRequired; // may override state change from automatic reset. But that is okay, because it just resetted anyway.
                        _isTaskRunning = false;
                    });
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
            
            _profitPanels.UpdateProfitPerHourEveryFiveSeconds(_services.Model.FarmingDuration.Elapsed);
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
                _services.Model.UpdateStatsSnapshot();
                _lastStatsUpdateSuccessfull = true; // in case a previous update failed. Because that doesnt matter anymore after the reset.
                _hintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
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
                await UpdateStatsInModel(drfMessages, _services);
                _services.Model.UpdateStatsSnapshot();
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                _lastStatsUpdateSuccessfull = true;
                _hintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
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

        private async Task UpdateStatsInModel(List<DrfMessage> drfMessages, Services services)
        {      
            DrfResultAdder.UpdateCountsOrAddNewStats(drfMessages, services.Model.ItemById, services.Model.CurrencyById);
            await _statsSetter.SetDetailsAndProfitFromApi(services.Model.ItemById, services.Model.CurrencyById, services.Gw2ApiManager);
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
                ControlPadding = new Vector2(0, 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = rootFlowPanel
            };

            CreateHelpResetDrfButtons(services);
            CreateTimeAndHintLabels(services);
            _profitPanels = new ProfitPanels(_services.TextureService, _services.FontService, _farmingRootFlowPanel);
            _searchPanel = new SearchPanel(_services, _farmingRootFlowPanel);
            CreateStatsPanels(services);

            return rootFlowPanel;
        }

        private void CreateStatsPanels(Services services)
        {
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

            _statsPanels.CurrenciesFlowPanel = new FlowPanel()
            {
                Title = CURRENCIES_PANEL_TITLE,
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = Constants.PANEL_WIDTH,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = currenciesFilterIconPanel
            };

            _statsPanels.ItemsFlowPanel = new FlowPanel()
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

            new HintLabel(_statsPanels.CurrenciesFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");
            new HintLabel(_statsPanels.ItemsFlowPanel, $"{Constants.HINT_IN_PANEL_PADDING}Loading...");
        }

        private void CreateTimeAndHintLabels(Services services)
        {
            _timeAndHintFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.LeftToRight,
                ControlPadding = new Vector2(20, 0),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _farmingRootFlowPanel
            };

            _elapsedFarmingTimeLabel = new ElapsedFarmingTimeLabel(services, _timeAndHintFlowPanel);

            _hintLabel = new Label
            {
                Text = Constants.FULL_HEIGHT_EMPTY_LABEL,
                Font = services.FontService.Fonts[FontSize.Size14],
                Width = 250, // prevents that when window width is small the empty label moves behind the elapsed time label causing the whole UI to move up.
                AutoSizeHeight = true,
                Parent = _timeAndHintFlowPanel
            };
        }

        private void CreateHelpResetDrfButtons(Services services)
        {
            var buttonFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5, 0),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _farmingRootFlowPanel
            };

            _collapsibleHelp = new CollapsibleHelp(
                "DRF setup instructions and DRF and module troubleshooting can be found in the settings tab.\n" +
                $"\n" +
                $"PROFIT:\n" +
                "15% trading post fee is already deducted.\n" +
                "Profit also includes changes in 'raw gold'. In other words coins spent or gained. " +
                $"'raw gold' changes are also visible in the '{CURRENCIES_PANEL_TITLE}' panel.\n" +
                "Lost items reduce the profit accordingly.\n" +
                "Currencies are not included in the profit calculation (except 'raw gold').\n" +
                "rough profit = raw gold + item count * tp sell price * 0.85 + ...for all items.\n" +
                "When tp sell price does not exist, tp buy price will be used. " +
                "Vendor price will be used when it is higher than tp sell/buy price * 0.85.\n" +
                "Module and DRF live tracking website profit calculation may differ because different profit formulas are used.\n" +
                $"Profit per hour is updated every {Constants.PROFIT_PER_HOUR_UPDATE_INTERVAL_IN_SECONDS} seconds.\n" +
                $"The profit is only a rough estimate because the trading post buy/sell prices can change over time and " +
                $"only the highest tp buy price and tp sell price for an item are considered. The tp buy/sell prices are a snapshot from " +
                $"when the item was tracked for the first time during a blish sesssion.\n" +
                $"\n" +
                $"RESIZE:\n" +
                $"You can resize the window by dragging the bottom right window corner. " +
                $"Some UI elements might be cut off when the window becomes too small.",
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

            _resetButton = new StandardButton()
            {
                Text = "Reset",
                BasicTooltipText = "Start new farming session by resetting tracked items and currencies.",
                Width = 90,
                Parent = subButtonFlowPanel,
            };

            _resetButton.Click += (s, e) =>
            {
                _resetButton.Enabled = false;
                _resetState = ResetState.ResetRequired;
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
        }

        private void OnRarityIconBorderVisibleSettingChanged(object sender, Blish_HUD.ValueChangedEventArgs<bool> e)
        {
            _services.UpdateLoop.TriggerUpdateUi();
        }

        private bool _isTaskRunning;
        private Label _hintLabel;
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private ProfitPanels _profitPanels;
        private SearchPanel _searchPanel;
        private readonly FlowPanel _rootFlowPanel;
        private Label _drfErrorLabel;
        private OpenSettingsButton _openSettingsButton;
        private FlowPanel _farmingRootFlowPanel;
        private StandardButton _resetButton;
        private FlowPanel _timeAndHintFlowPanel;
        private ElapsedFarmingTimeLabel _elapsedFarmingTimeLabel;
        private string _oldApiTokenErrorTooltip = string.Empty;
        private bool _apiErrorHintVisible;
        private bool _lastStatsUpdateSuccessfull = true;
        private ResetState _resetState = ResetState.NoResetRequired;
        private bool _isModuleStartForReset = true;
        private readonly StatsSetter _statsSetter = new StatsSetter();
        private readonly Services _services;
        private readonly StatsPanels _statsPanels = new StatsPanels();
        private double _saveModelRunningTimeMs;
        private CollapsibleHelp _collapsibleHelp;
        private readonly double SAVE_MODEL_INTERVAL_MS = TimeSpan.FromMinutes(1).TotalMilliseconds;
        public const string GW2_API_ERROR_HINT = "GW2 API error";
        private const string CURRENCIES_PANEL_TITLE = "Currencies";
    }
}
