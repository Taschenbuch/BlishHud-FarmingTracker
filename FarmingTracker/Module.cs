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
                _runningTimeInMilliseconds = NEXT_UPDATE_INTERVAL_IN_MILLISECONDS + CHECK_API_KEY_INTERVALL_IN_MILLISECONDS; // trigger items update immediately
                // todo items clear und UpdateUi() woanders hinschieben, ist hier falsch. poentielle racing conditions.
                // Es muss aber weiterhin instant die flowpanels clearen.
                _elapsedFarmingTimeStopwatch.Reset();
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
        }

        protected override void Update(GameTime gameTime)
        {
            var elapsedFarmingTime = _elapsedFarmingTimeStopwatch.Elapsed;
            if (elapsedFarmingTime >= _oldElapsedFarmingTime + ONE_SECOND)
            {
                // todo next update time vs elappsed farming time has 1 second difference
                _oldElapsedFarmingTime = elapsedFarmingTime;
                _elapsedFarmingTimeLabel.Text = $"farming for {elapsedFarmingTime:h':'mm':'ss}";
                var nextUpdateTime = TIME_INTERVAL_FOR_NEXT_UPDATE - _nextUpdateTimeStopwatch.Elapsed;
                if (_nextUpdateTimeStopwatch.IsRunning)
                    _nextUpdateCountdownLabel.Text = $"next update in {nextUpdateTime:m':'ss}";
            }

            _runningTimeInMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_runningTimeInMilliseconds >= _updateIntervalInMilliseconds) // todo sinnvolles intervall wählen. 2min? 5min? keine ahnung
            {
                _runningTimeInMilliseconds = 0;

                if (!Gw2ApiManager.HasPermissions(new List<TokenPermission> { TokenPermission.Account })) // todo sauberer lösen, das wartet sich hier zu tode
                {
                    Logger.Debug("token waiting..."); // todo weg
                    _updateIntervalInMilliseconds = CHECK_API_KEY_INTERVALL_IN_MILLISECONDS;
                    return;
                }

                _updateIntervalInMilliseconds = NEXT_UPDATE_INTERVAL_IN_MILLISECONDS;
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

                await Task.WhenAll(apiResponseTasks); // todo log warn exception wie session tracker
                Logger.Debug("get items done"); // todo weg
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
                    _elapsedFarmingTimeStopwatch.Restart();
                    _oldElapsedFarmingTime = _elapsedFarmingTimeStopwatch.Elapsed;
                    UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                    _resetButton.Enabled = true;
                    return;
                }

                Logger.Debug("create diff"); // todo weg
                var farmedItems = DetermineFarmedItems(items, _itemsWhenTrackingStarted);
                var farmedCurrencies = DetermineFarmedItems(currencies, _currenciesWhenTrackingStarted);

                var hasFarmedNothing = !farmedItems.Any() && !farmedCurrencies.Any();
                if (hasFarmedNothing)
                {
                    _farmedItems.Clear();
                    _farmedCurrencies.Clear();
                    UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
                    return;
                }

                Logger.Debug("get name description icon asset id"); // todo weg
                if (farmedCurrencies.Any())
                {
                    var farmedCurrencyIds = farmedCurrencies.Select(i => i.ApiId).ToList();
                    var apiCurrencies = await Gw2ApiManager.Gw2ApiClient.V2.Currencies.ManyAsync(farmedCurrencyIds);
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
                    var apiItems = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(farmedItemIds);
                    foreach (var apiItem in apiItems)
                    {
                        var matchingItem = farmedItems.Single(i => i.ApiId == apiItem.Id); // todo null check danach nötig?
                        matchingItem.Name = apiItem.Name;
                        matchingItem.Description = apiItem.Description;
                        matchingItem.IconAssetId = int.Parse(Path.GetFileNameWithoutExtension(apiItem.Icon.Url.AbsoluteUri));
                    }
                }

                CurrencySearcher.ReplaceCoinItemWithGoldSilverCopperItems(farmedCurrencies);

                Logger.Debug("update ui"); // todo weg
                // todo wenn man das weiter nach oben schiebt, werden neu gefarmte item früher angezeigt, ABER ohne icon+name. müsste für den Fall placeholder hinterlegen
                _farmedItems.Clear();
                _farmedItems.AddRange(farmedItems);
                _farmedCurrencies.Clear();
                _farmedCurrencies.AddRange(farmedCurrencies);
                UiUpdater.UpdateUi(_farmedCurrencies, _farmedItems, _farmedCurrenciesFlowPanel, _farmedItemsFlowPanel);
            }
            finally
            {
                Logger.Debug("TrackItems end"); // todo weg
                _nextUpdateTimeStopwatch.Restart();
                _runningTimeInMilliseconds = 0;
                _taskIsRunning = false;
            }
        }
   
        private List<ItemX> DetermineFarmedItems(List<ItemX> newItems, List<ItemX> oldItems)
        {
            var itemById = new Dictionary<int, ItemX>();

            foreach (var newItem in newItems)
                itemById[newItem.ApiId] = newItem;

            foreach (var oldItem in oldItems)
            {
                if (itemById.TryGetValue(oldItem.ApiId, out var item))
                    item.Count -= oldItem.Count;
                else
                    itemById[oldItem.ApiId] = new ItemX // do not set oldItem here. oldItem must not be modified
                    {
                        ApiId = oldItem.ApiId,
                        Count = -oldItem.Count,
                    };
            }

            return itemById.Values.Where(i => i.Count != 0).ToList();
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
        private readonly Stopwatch _elapsedFarmingTimeStopwatch = new Stopwatch();
        private readonly Stopwatch _nextUpdateTimeStopwatch = new Stopwatch();
        private TrackerCornerIcon _trackerCornerIcon;
        private bool _taskIsRunning; // todo anders lösen
        private bool _isStartingNewFarmingSession = true;
        private double _runningTimeInMilliseconds = CHECK_API_KEY_INTERVALL_IN_MILLISECONDS; // to start immediately
        private double _updateIntervalInMilliseconds = CHECK_API_KEY_INTERVALL_IN_MILLISECONDS; // to start immediately
        private TimeSpan _oldElapsedFarmingTime;
        private readonly List<ItemX> _itemsWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _currenciesWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _farmedItems = new List<ItemX>();
        private readonly List<ItemX> _farmedCurrencies = new List<ItemX>();
        private static readonly TimeSpan ONE_SECOND = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan TIME_INTERVAL_FOR_NEXT_UPDATE = TimeSpan.FromMilliseconds(NEXT_UPDATE_INTERVAL_IN_MILLISECONDS);
        private const int CHECK_API_KEY_INTERVALL_IN_MILLISECONDS = 2 * 1000;
        private const int NEXT_UPDATE_INTERVAL_IN_MILLISECONDS = 5 * 1000; // todo sinnvolle zeit angeben
    }
}
