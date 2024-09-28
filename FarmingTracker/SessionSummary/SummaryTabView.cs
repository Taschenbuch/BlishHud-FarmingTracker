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

            _controls = new SummaryTabViewControls(model, services);
            _controls.ResetButton.Click += (s, e) =>
            {
                _controls.ResetButton.Enabled = false;
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
            _controls.RootFlowPanel.Parent = buildPanel;
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
            _controls.StatsPanels.CurrenciesFlowPanel.Width = width;
            _controls.StatsPanels.ItemsFlowPanel.Width = width;
            _controls.StatsPanels.FavoriteItemsFlowPanel.Width = width;
            _controls.StatsPanels.ItemsFilterIcon.SetLeft(width);
            _controls.StatsPanels.CurrencyFilterIcon.SetLeft(width);
            _controls.SearchPanel.UpdateSize(width);
            _controls.CollapsibleHelp.UpdateSize(width - resetAndDrfButtonsOffset);
        }

        public void Update(GameTime gameTime)
        {
            _services.UpdateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (!_isUiUpdateTaskRunning && _services.UpdateLoop.HasToUpdateUi()) // short circuit method call to prevent resetting its bool
            {
                _isUiUpdateTaskRunning = true;
                Task.Run(() =>
                {
                    var snapshot = _model.Stats.StatsSnapshot;
                    _services.ProfitCalculator.CalculateProfits(snapshot, _model.CustomStatProfits, _model.IgnoredItemApiIds, _services.FarmingDuration.Elapsed);
                    _controls.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                    _profitWindow.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                    UiUpdater.UpdateStatPanels(_controls.StatsPanels, snapshot, _model, _services);

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
                _controls.HintLabel.Text = $"{Constants.RESETTING_HINT_TEXT} (this may take a few seconds)";

                if (!_statsAccessLocked) // prevents that reset and update modify stats at the same time
                {
                    _statsAccessLocked = true;
                    _resetState = ResetState.Resetting;
                    Task.Run(() =>
                    {
                        ResetStats();
                        _automaticResetService.UpdateNextResetDateTime(); // on manual resets this will effectively not change the next automatic reset dateTime.
                        _controls.ElapsedFarmingTimeLabel.RestartTime();
                        _services.UpdateLoop.TriggerUpdateUi();
                        _services.UpdateLoop.TriggerSaveModel();
                        _controls.ResetButton.Enabled = true;
                        _resetState = ResetState.NoResetRequired; // may override state change from automatic reset. But that is okay, because it just resetted anyway.
                        _statsAccessLocked = false;
                    });
                }
                return; // that is enough work for a single update loop iteration. And prevents farming time updates and prevents hintText from being overriden.
            }

            if(!_statsAccessLocked && _services.UpdateLoop.HasToSaveModel())
            {
                _statsAccessLocked = true;

                Task.Run(async () =>
                {
                    await _services.FileSaver.SaveModelToFile(_model);
                    _statsAccessLocked = false;
                });
                // do not return here because saving the model should not disturb other parts of Update().
            }

            if (_profitPerHourUpdateInterval.HasEnded())
            {
                _services.ProfitCalculator.CalculateProfitPerHour(_services.FarmingDuration.Elapsed);
                _controls.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
                _profitWindow.ProfitPanels.ShowProfits(_services.ProfitCalculator.ProfitInCopper, _services.ProfitCalculator.ProfitPerHourInCopper);
            }

            _controls.ElapsedFarmingTimeLabel.UpdateTimeEverySecond(); // not sure if this can use Interval class, too. But it must not update when farming time is not running (happens when resetting?)

            if (_services.UpdateLoop.UpdateIntervalEnded()) // todo guard stattdessen?
            {
                _services.UpdateLoop.ResetRunningTime();
                _services.UpdateLoop.UseFarmingUpdateInterval();

                ShowOrHideDrfErrorLabelAndStatPanels(_services.Drf.DrfConnectionStatus, _controls.DrfErrorLabel, _controls.OpenSettingsButton, _controls.FarmingRootFlowPanel);

                var apiToken = new ApiToken(_services.Gw2ApiManager);
                ShowOrHideApiErrorHint(apiToken, _controls.HintLabel, _timeSinceModuleStartStopwatch.Elapsed.TotalSeconds);
                if (!apiToken.CanAccessApi)
                    return; // dont continue to prevent api error hint being overriden by "update..." etc.

                if (!_statsAccessLocked)
                {
                    _statsAccessLocked = true;
                    Task.Run(async () =>
                    {
                        await UpdateStats();
                        _statsAccessLocked = false;
                    });
                }
            }
        }

        private void ResetStats()
        {
            try
            {
                StatsService.ResetCounts(_model.Stats.ItemById);
                StatsService.ResetCounts(_model.Stats.CurrencyById);
                _model.Stats.UpdateStatsSnapshot();
                _lastStatsUpdateSuccessfull = true; // in case a previous update failed. Because that doesnt matter anymore after the reset.
                _controls.HintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(ResetStats)} failed.");
                _controls.HintLabel.Text = $"Module crash. :-("; // todo was tun?
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

                _controls.HintLabel.Text = $"{Constants.UPDATING_HINT_TEXT} (this may take a few seconds)";
                await UpdateStatsInModel(drfMessages, _services);
                _model.Stats.UpdateStatsSnapshot();
                _services.UpdateLoop.TriggerUpdateUi();
                _services.UpdateLoop.TriggerSaveModel();
                _lastStatsUpdateSuccessfull = true;
                _controls.HintLabel.Text = Constants.FULL_HEIGHT_EMPTY_LABEL;
            }
            catch (Gw2ApiException exception)
            {
                Module.Logger.Warn(exception, exception.Message);
                _services.UpdateLoop.UseRetryAfterApiFailureUpdateInterval();
                _lastStatsUpdateSuccessfull = false;
                _controls.HintLabel.Text = $"{Constants.GW2_API_ERROR_HINT}. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s";
            }
            catch (Exception exception)
            {
                Module.Logger.Error(exception, $"{nameof(UpdateStats)} failed.");
                _lastStatsUpdateSuccessfull = false;
                _controls.HintLabel.Text = $"Module crash. :-("; // todo was tun?
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
            DrfResultAdder.UpdateCountsOrAddNewStats(drfMessages, _model.Stats.ItemById, _model.Stats.CurrencyById);
            await _statsSetter.SetDetailsAndProfitFromApi(_model.Stats.ItemById, _model.Stats.CurrencyById, services.Gw2ApiManager);
        }

        private bool _statsAccessLocked;
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
        private readonly SummaryTabViewControls _controls;
        private readonly AutomaticResetService _automaticResetService;
        private readonly Interval _profitPerHourUpdateInterval = new Interval(TimeSpan.FromMilliseconds(5000));
        private readonly Interval _saveFarmingDurationInterval = new Interval(TimeSpan.FromMinutes(1));
        private readonly Interval _automaticResetCheckInterval = new Interval(TimeSpan.FromMinutes(1), true); // end first interval to enforce check right on module start.
    }
}
