using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Blish_HUD.ContentService;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmingTracker // todo rename (überall dann anpassen
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module // todo rename
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
        }

        protected override async Task LoadAsync()
        {
            _windowEmblemTexture = ContentsManager.GetTexture(@"windowEmblem.png");

            var windowWidth = 560;
            var windowHeight = 640;
            var flowPanelWidth = windowWidth - 50;

            _farmingTrackerWindow = new StandardWindow(
                AsyncTexture2D.FromAssetId(155997),
                new Rectangle(25, 26, windowWidth, windowHeight),
                new Rectangle(40, 50, windowWidth - 20, windowHeight - 50))
            {
                Title = "Farming Tracker (Beta)", // todo remove "beta"
                Emblem = _windowEmblemTexture,
                SavesPosition = true,
                Id = "Ecksofa.FarmingTracker: error window",
                Location = new Point(300, 300),
                Parent = GameService.Graphics.SpriteScreen,
            };

            _farmingTrackerWindow.Show(); // todo weg

            _rootFlowPanel = new FlowPanel()
            {
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                CanScroll = true,
                ControlPadding = new Vector2(0, 10),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill,
                Parent = _farmingTrackerWindow
            };

            _resetButton = new StandardButton()
            {
                Text = "Reset",
                BasicTooltipText = "Start new farming session by resetting farmed items and currencies.",
                Enabled = false,
                Width = 90,
                Left = 460,
                Parent = _rootFlowPanel,
            };

            _resetButton.Click += (s, e) =>
            {
                _isStartingNewFarmingSession = true;
                _resetButton.Enabled = false;
                _updateLoop.TiggerUpdateInstantly();
                // todo items clear und UpdateUi() woanders hinschieben, ist hier falsch. poentielle racing conditions.
                // Es muss aber weiterhin instant die flowpanels clearen.
                _farmingTimeStopwatch.Reset();
                _nextUpdateTimeStopwatch.Reset();
                _elapsedFarmingTimeLabel.Text = "updating...";
                _nextUpdateCountdownLabel.Text = "updating...";
                _farmedItems.Clear(); 
                _farmedCurrencies.Clear();
                UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                // todo kill running update processes.
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
                Text = "next update in -:--", // todo getElapsedTimeDisplayText() oder so, weil an vielen stellen vorhanden 
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

            _trackerCornerIcon = new TrackerCornerIcon(ContentsManager, _farmingTrackerWindow);
            _timeSinceModuleStartStopwatch.Start();
        }

        protected override void Update(GameTime gameTime)
        {
            var farmingTime = _farmingTimeStopwatch.Elapsed;
            if (farmingTime >= _oldFarmingTime + ONE_SECOND)
            {
                // todo next update time vs elappsed farming time has 1 second difference
                _oldFarmingTime = farmingTime;
                _elapsedFarmingTimeLabel.Text = $"farming for {farmingTime:h':'mm':'ss}";
                var nextUpdateTime = UpdateLoop.FARMING_UPDATE_INTERVAL_TIME - _nextUpdateTimeStopwatch.Elapsed;
                if (_nextUpdateTimeStopwatch.IsRunning)
                    _nextUpdateCountdownLabel.Text = $"next update in {nextUpdateTime:m':'ss}";
            }

            _updateLoop.AddToRunningTime(gameTime.ElapsedGameTime.TotalMilliseconds);

            if (_updateLoop.UpdateIntervalEnded()) // todo sinnvolles intervall wählen. 2min? 5min? keine ahnung
            {
                _updateLoop.ResetRunningTime();
                _updateLoop.UseFarmingUpdateInterval();

                var apiToken = new ApiToken(REQUIRED_API_TOKEN_PERMISSIONS, Gw2ApiManager);
                if (!apiToken.CanAccessApi)
                {
                    var apiTokenErrorTooltip = apiToken.CreateApiTokenErrorTooltipText();
                    Logger.Debug(apiTokenErrorTooltip); // todo weg
                    var isGivingBlishSomeTimeToGiveToken = (apiToken.ApiTokenState == ApiTokenState.ApiTokenMissing) && (_timeSinceModuleStartStopwatch.Elapsed.TotalSeconds < 20);
                    _nextUpdateCountdownLabel.Text = isGivingBlishSomeTimeToGiveToken
                        ? "Loading..."
                        : $"{apiToken.CreateApiTokenErrorLabelText()} Retry every {UpdateLoop.WAIT_FOR_API_TOKEN_UPDATE_INTERVALL_MS/1000}s";
                    _nextUpdateCountdownLabel.BasicTooltipText = isGivingBlishSomeTimeToGiveToken ? "" : apiTokenErrorTooltip;
                    
                    _nextUpdateTimeStopwatch.Stop();
                    _updateLoop.UseWaitForApiTokenUpdateInterval();
                    return;
                }
                _nextUpdateCountdownLabel.BasicTooltipText = "";

                Logger.Debug("TrackItems try..."); // todo weg

                if (!_taskIsRunning)
                {
                    _taskIsRunning = true;
                    _nextUpdateTimeStopwatch.Stop();
                    _nextUpdateCountdownLabel.Text = "updating...";
                    Task.Run(() => TrackItems());
                }
                else
                    Logger.Debug("TrackItems already running"); // todo weg
            }
        }

        protected override void Unload()
        {
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindow?.Dispose();
            _windowEmblemTexture?.Dispose();
        }

        private async void TrackItems()
        {
            Logger.Debug("TrackItems start"); // todo weg

            try
            {
                // get all items on account
                var charactersTask = Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                var bankTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync();
                var sharedInventoryTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync();
                var materialStorageTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Materials.GetAsync();
                var walletTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Wallet.GetAsync();

                var apiResponseTasks = new List<Task>
                {
                    charactersTask,
                    bankTask,
                    sharedInventoryTask,
                    materialStorageTask,
                    walletTask
                };

                try
                {
                    await Task.WhenAll(apiResponseTasks);
                }
                catch (Exception e)
                {
                    throw new Gw2ApiException("API error: get all account items and currencies", e);
                }

                Logger.Debug("TrackItems get items"); // todo weg
                var items = ItemSearcher.GetItemIdsAndCounts(charactersTask.Result, bankTask.Result, sharedInventoryTask.Result, materialStorageTask.Result);
                var currencies = CurrencySearcher.GetCurrencyIdsAndCounts(walletTask.Result).ToList();

                if (_isStartingNewFarmingSession) // dont replace with .Any(). A new account may have no item/currencies yet
                {
                    Logger.Debug("TrackItems new session"); // todo weg
                    _isStartingNewFarmingSession = false;
                    _itemsWhenTrackingStarted.Clear();
                    _itemsWhenTrackingStarted.AddRange(items);
                    _currenciesWhenTrackingStarted.Clear();
                    _currenciesWhenTrackingStarted.AddRange(currencies);
                    _farmingTimeStopwatch.Restart();
                    _oldFarmingTime = _farmingTimeStopwatch.Elapsed;
                    UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                    _resetButton.Enabled = true;
                    return;
                }

                Logger.Debug("TrackItems create diff"); // todo weg
                var farmedItems = FarmedItems.DetermineFarmedItems(items, _itemsWhenTrackingStarted);
                var farmedCurrencies = FarmedItems.DetermineFarmedItems(currencies, _currenciesWhenTrackingStarted);

                var hasFarmedNothing = !farmedItems.Any() && !farmedCurrencies.Any();
                if (hasFarmedNothing)
                {
                    _farmedItems.Clear();
                    _farmedCurrencies.Clear();
                    // todo rest als method extrahieren, so dass stopwatch und UpdateUi nicht hier drin stehen müssen
                    UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel); 
                    _nextUpdateTimeStopwatch.Restart();
                    return;
                }

                Logger.Debug("TrackItems update items (name, description, iconAssetid)"); // todo weg
                if (farmedCurrencies.Any())
                {
                    var farmedCurrencyIds = farmedCurrencies.Select(i => i.ApiId).ToList();
                    IReadOnlyList<Currency> apiCurrencies;

                    try
                    {
                        apiCurrencies = await Gw2ApiManager.Gw2ApiClient.V2.Currencies.ManyAsync(farmedCurrencyIds);
                    }
                    catch (Gw2ApiException e)
                    {
                        throw new Gw2ApiException("API error: update currencies", e);
                    }

                    foreach (var apiCurrency in apiCurrencies)
                    {
                        var matchingCurrency = farmedCurrencies.Single(i => i.ApiId == apiCurrency.Id); // todo null check danach nötig?
                        matchingCurrency.Name = apiCurrency.Name;
                        matchingCurrency.Description = apiCurrency.Description;
                        matchingCurrency.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiCurrency.Icon.Url.AbsoluteUri));
                    }
                }

                if (farmedItems.Any())
                {
                    var farmedItemIds = farmedItems.Select(i => i.ApiId).ToList();
                    IReadOnlyList<Item> apiItems;

                    try
                    {
                        apiItems = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(farmedItemIds);
                    }
                    catch (Gw2ApiException e)
                    {
                        throw new Gw2ApiException("API error: update items", e);
                    }

                    foreach (var apiItem in apiItems)
                    {
                        var matchingItem = farmedItems.Single(i => i.ApiId == apiItem.Id); // todo null check danach nötig?
                        matchingItem.Name = apiItem.Name;
                        matchingItem.Description = apiItem.Description;
                        matchingItem.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiItem.Icon.Url.AbsoluteUri));
                    }
                }

                CurrencySearcher.ReplaceCoinItemWithGoldSilverCopperItems(farmedCurrencies);

                Logger.Debug("TrackItems update ui"); // todo weg
                // todo wenn man das weiter nach oben schiebt, werden neu gefarmte item früher angezeigt, ABER ohne icon+name. müsste für den Fall placeholder hinterlegen
                _farmedItems.Clear();
                _farmedItems.AddRange(farmedItems);
                _farmedCurrencies.Clear();
                _farmedCurrencies.AddRange(farmedCurrencies);
                UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                _nextUpdateTimeStopwatch.Restart();
            }
            catch (Gw2ApiException exception)
            {
                Logger.Warn(exception, exception.Message); // todo keine exception loggen? zu spammy?
                _updateLoop.UseRetryAfterApiFailureUpdateInterval();
                _nextUpdateCountdownLabel.Text = $"API error. Retry every {UpdateLoop.RETRY_AFTER_API_FAILURE_UPDATE_INTERVAL_MS / 1000}s (TODO: display countdown)"; // todo countdown
                _nextUpdateTimeStopwatch.Stop();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "track items failed.");
                _nextUpdateCountdownLabel.Text = $"Module crash. :-("; // todo was tun?
                _nextUpdateTimeStopwatch.Restart();
                // todo was tun?
            }
            finally
            {
                Logger.Debug("TrackItems end"); // todo weg
                _updateLoop.ResetRunningTime();
                _taskIsRunning = false;
            }
        }              

        private Texture2D _windowEmblemTexture;
        private StandardWindow _farmingTrackerWindow;
        private FlowPanel _rootFlowPanel;
        private StandardButton _resetButton;
        private FlowPanel _controlsFlowPanel;
        private Label _elapsedFarmingTimeLabel;
        private Label _nextUpdateCountdownLabel;
        private FlowPanel _farmedCurrenciesFlowPanel;
        private FlowPanel _farmedItemsFlowPanel;
        private readonly Stopwatch _farmingTimeStopwatch = new Stopwatch(); // extract farming time and next update
        private readonly Stopwatch _nextUpdateTimeStopwatch = new Stopwatch();
        private readonly Stopwatch _timeSinceModuleStartStopwatch = new Stopwatch();
        private TrackerCornerIcon _trackerCornerIcon;
        private bool _taskIsRunning; // todo anders lösen
        private bool _isStartingNewFarmingSession = true;
        private readonly UpdateLoop _updateLoop = new UpdateLoop();
        private TimeSpan _oldFarmingTime;
        private readonly List<ItemX> _itemsWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _currenciesWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _farmedItems = new List<ItemX>();
        private readonly List<ItemX> _farmedCurrencies = new List<ItemX>();
        private readonly TimeSpan ONE_SECOND = TimeSpan.FromSeconds(1);
        private readonly IReadOnlyList<TokenPermission> REQUIRED_API_TOKEN_PERMISSIONS = new List<TokenPermission>
        {
            TokenPermission.Account,
            TokenPermission.Inventories,
            TokenPermission.Characters,
            TokenPermission.Wallet,
            TokenPermission.Builds,
            TokenPermission.Tradingpost,
        }.AsReadOnly();
    }
}
