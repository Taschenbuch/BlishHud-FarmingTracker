using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FarmingTracker
{
    public class SummaryTabView : View, IDisposable
    {
        public SummaryTabView(ProfitWindow profitWindow, Model model, Services services) 
        {
            _profitWindow = profitWindow;
            _model = model;
            _services = services;

            _ui = new SummaryUi(model, services);
            _ui.ResetButton.Click += (s, e) =>
            {
                _ui.ResetButton.Enabled = false;
                _resetState = ResetState.ResetRequired;
            };

            var automaticResetService = new AutomaticResetService(services);
            _automaticResetService = automaticResetService;

            _timeSinceModuleStartStopwatch.Restart();
            services.UpdateLoop.TriggerUpdateStats();
        }

        public void Dispose()
        {
            _automaticResetService?.Dispose();
        }

        protected override void Unload()
        {
            // NOOP because CreateUi() is called in ctor instead of Build()
        }

        protected override void Build(Container buildPanel)
        {
            _ui.RootFlowPanel.Parent = buildPanel;
            var resetAndDrfButtonsOffset = 70;
            var width = buildPanel.ContentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
            ResizeToViewWidth(resetAndDrfButtonsOffset, width);

            buildPanel.ContentResized += (s,e) =>
            {
                var width = e.CurrentRegion.Width - Constants.SCROLLBAR_WIDTH_OFFSET;
                ResizeToViewWidth(resetAndDrfButtonsOffset, width);
            };
        }

        private void ResizeToViewWidth(int resetAndDrfButtonsOffset, int width)
        {
            _ui.StatsPanels.CurrenciesFlowPanel.Width = width;
            _ui.StatsPanels.ItemsFlowPanel.Width = width;
            _ui.StatsPanels.FavoriteItemsFlowPanel.Width = width;
            _ui.StatsPanels.ItemsFilterIcon.SetLeft(width);
            _ui.StatsPanels.CurrencyFilterIcon.SetLeft(width);
            _ui.SearchPanel.UpdateSize(width);
            _ui.CollapsibleHelp.UpdateSize(width - resetAndDrfButtonsOffset);
        }

        public void Update(GameTime gameTime)
        {
            _services.UpdateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (!_isUiUpdateTaskRunning && _services.UpdateLoop.HasToUpdateUi()) // short circuit method call to prevent resetting its bool
            {
                _isUiUpdateTaskRunning = true;
                Task.Run(() =>
                {
                    var snapshot = _model.StatsSnapshot;
                    _services.ProfitCalculator.CalculateProfits(snapshot, _model.IgnoredItemApiIds, _services.FarmingDuration.Elapsed);
                    _ui.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                    _profitWindow.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                    UiUpdater.UpdateStatPanels(_ui.StatsPanels, snapshot, _model, _services);

                    _isUiUpdateTaskRunning = false;
                });
            }

            if(_saveFarmingDurationInterval.HasEnded())
                _services.FarmingDuration.SaveFarmingTime();

            if (_automaticResetCheckInterval.HasEnded())
            {
                if (_automaticResetService.HasToResetAutomatically())
                    _resetState = ResetState.ResetRequired;

                _automaticResetService.UpdateNextResetDateTimeForMinutesUntilResetAfterModuleShutdown(); // must be called AFTER automatic reset check! Not before!
            }

            if (_resetState != ResetState.NoResetRequired) // at loop start to prevent that reset is delayed by drf or api issues or hintLabel is overriden by api issues
            {
                _ui.HintLabel.Text = $"{Constants.RESETTING_HINT_TEXT} (this may take a few seconds)";

                if (!_isTaskRunning) // prevents that reset and update modify stats at the same time
                {
                    _isTaskRunning = true;
                    _resetState = ResetState.Resetting;
                    Task.Run(() =>
                    {
                        ResetStats();
                        _automaticResetService.UpdateNextResetDateTime(); // on manual resets this will effectively not change the next automatic reset dateTime.
                        _ui.ElapsedFarmingTimeLabel.RestartTime();
                        _services.UpdateLoop.TriggerUpdateUi();
                        _services.UpdateLoop.TriggerSaveModel();
                        _ui.ResetButton.Enabled = true;
                        _resetState = ResetState.NoResetRequired; // may override state change from automatic reset. But that is okay, because it just resetted anyway.
                        _isTaskRunning = false;
                    });
                }
                return; // that is enough work for a single update loop iteration. And prevents farming time updates and prevents hintText from being overriden.
            }

            if(!_isTaskRunning)
            {
                if (_services.UpdateLoop.HasToSaveModel())
                {
                    _isTaskRunning = true;

                    Task.Run(async () =>
                    {
                        await _services.FileSaver.SaveModelToFile(_model);
                        _isTaskRunning = false;
                    });
                    // do not return here because saving the model should not disturb other parts of Update().
                }
            }

            if (_profitPerHourUpdateInterval.HasEnded())
            {
                _services.ProfitCalculator.CalculateProfitPerHour(_services.FarmingDuration.Elapsed);
                _ui.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                _profitWindow.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
            }

            _ui.ElapsedFarmingTimeLabel.UpdateTimeEverySecond(); // not sure if this can use Interval class, too. But it must not update when farming time is not running (happens when resetting?)

            if (_services.UpdateLoop.UpdateIntervalEnded()) // todo guard stattdessen?
            {
                _services.UpdateLoop.ResetRunningTime();
                _services.UpdateLoop.UseFarmingUpdateInterval();

                ShowOrHideDrfErrorLabelAndStatPanels(_services.Drf.DrfConnectionStatus, _ui.DrfErrorLabel, _ui.OpenSettingsButton, _ui.FarmingRootFlowPanel);

                var apiToken = new ApiToken(_services.Gw2ApiManager);
                ShowOrHideApiErrorHint(apiToken, _ui.HintLabel, _timeSinceModuleStartStopwatch.Elapsed.TotalSeconds);
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

        private void ResetStats()
        {
            try
            {
                StatsService.ResetCounts(_model.ItemById);
                StatsService.ResetCounts(_model.CurrencyById);
                _model.UpdateStatsSnapshot();
                _lastStatsUpdateSuccessfull = true; // in case a previous update failed. Because that doesnt matter anymore after the reset.
                _ui.HintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(ResetStats)} failed.");
                _ui.HintLabel.Text = $"Module crash. :-("; // todo was tun?
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

                _ui.HintLabel.Text = $"{Constants.UPDATING_HINT_TEXT} (this may take a few seconds)";
                await UpdateStatsInModel(drfMessages, _services);
                _model.UpdateStatsSnapshot();
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                _lastStatsUpdateSuccessfull = true;
                _ui.HintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message);
                _services.UpdateLoop.UseRetryAfterApiFailureUpdateInterval();
                _lastStatsUpdateSuccessfull = false;
                _ui.HintLabel.Text = $"{Constants.GW2_API_ERROR_HINT}. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(UpdateStats)} failed.");
                _lastStatsUpdateSuccessfull = false;
                _ui.HintLabel.Text = $"Module crash. :-("; // todo was tun?
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
            DrfResultAdder.UpdateCountsOrAddNewStats(drfMessages, _model.ItemById, _model.CurrencyById);
            await _statsSetter.SetDetailsAndProfitFromApi(_model.ItemById, _model.CurrencyById, services.Gw2ApiManager);
        }

        private bool _isTaskRunning;
        private bool _isUiUpdateTaskRunning;
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private string _oldApiTokenErrorTooltip = string.Empty;
        private bool _apiErrorHintVisible;
        private bool _lastStatsUpdateSuccessfull = true;
        private ResetState _resetState = ResetState.NoResetRequired;
        private readonly StatsSetter _statsSetter = new StatsSetter();
        private readonly ProfitWindow _profitWindow;
        private readonly Model _model;
        private readonly Services _services;
        private readonly SummaryUi _ui;
        private readonly AutomaticResetService _automaticResetService;
        private readonly Interval _profitPerHourUpdateInterval = new Interval(TimeSpan.FromMilliseconds(5000));
        private readonly Interval _saveFarmingDurationInterval = new Interval(TimeSpan.FromMinutes(1));
        private readonly Interval _automaticResetCheckInterval = new Interval(TimeSpan.FromMinutes(1), true); // end first interval to enforce check right on module start.
    }
}
