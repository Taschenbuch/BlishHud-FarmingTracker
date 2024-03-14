using System.Collections.Generic;
using System.ComponentModel.Composition;
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
            _farmingTrackerWindow = new StandardWindow(
                AsyncTexture2D.FromAssetId(155979),
                new Rectangle(40, 26, 913, 691),
                new Rectangle(70, 71, 839, 605))
            {
                Title = "Farming Tracker",
                SavesPosition = true,
                Id = "Ecksofa.FarmingTracker: error window",
                Location = new Point(300, 300),
                Parent = GameService.Graphics.SpriteScreen,
            };

            _farmedItemsFlowPanel = new FlowPanel()
            {
                Title = "farmed items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = 862,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _farmingTrackerWindow
            };

            _trackerCornerIcon = new TrackerCornerIcon(ContentsManager, _farmingTrackerWindow);
        }

        protected override void Update(GameTime gameTime)
        {
            _updateRunningTimeInMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateRunningTimeInMilliseconds > 5 * 1000) // todo sinnvolles intervall wählen. 2min? 5min? keine ahnung
            {
                _updateRunningTimeInMilliseconds = 0;

                if (!Gw2ApiManager.HasPermissions(new List<TokenPermission> { TokenPermission.Account })) // todo sauberer lösen
                {
                    Logger.Debug("token waiting..."); // todo weg
                    return;
                }

                Logger.Debug("TrackItems try..."); // todo weg

                if (!_taskIsRunning)
                {
                    _taskIsRunning = true;
                    Task.Run(() => TrackItems()); // todo verriegelung, dass nicht ständig hier reingeht, obwohl vorheriger Task noch läuft
                }
                else
                {
                    Logger.Debug("TrackItems already running"); // todo weg
                }
            }
        }

        protected override void Unload()
        {
            _trackerCornerIcon?.Dispose();
            _farmingTrackerWindow?.Dispose();
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

                // initialize snapshotget all items on account
                var isFirstSnapshot = !_itemsWhenTrackingStarted.Any();
                if (isFirstSnapshot)
                {
                    Logger.Debug("set init snapshot"); // todo weg
                    _itemsWhenTrackingStarted.AddRange(items);
                    _currenciesWhenTrackingStarted.AddRange(currencies);
                    return;
                }

                Logger.Debug("create diff"); // todo weg
                var farmedItems = DetermineFarmedItems(items, _itemsWhenTrackingStarted);
                var farmedCurrencies = DetermineFarmedItems(currencies, _currenciesWhenTrackingStarted);

                var hasFarmedNothing = !farmedItems.Any() && !farmedCurrencies.Any(); // todo das kein fehler fall!
                if (hasFarmedNothing)
                {
                    _farmedItems.Clear(); 
                    UpdateUi();
                    return;
                }

                _farmedItems.Clear();
                _farmedItems.AddRange(farmedCurrencies);
                _farmedItems.AddRange(farmedItems);

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

                Logger.Debug("update ui"); // todo weg
                UpdateUi();
            }
            finally
            {
                Logger.Debug("TrackItems end"); // todo weg
                _taskIsRunning = false;
            }
        }

        private List<ItemX> DetermineFarmedItems(List<ItemX> newItems, List<ItemX> oldItems)
        {
            var oldItemIds = oldItems.Select(i => i.ApiId).ToList();
            var newItemIds = newItems.Select(i => i.ApiId).ToList();

            var idsMissingInOldItems = newItemIds.Except(oldItemIds).ToList();
            var idsMissingInNewItems = oldItemIds.Except(newItemIds).ToList();

            foreach (var missingId in idsMissingInOldItems)
                oldItems.Add(new ItemX { ApiId = missingId });

            foreach (var missingId in idsMissingInNewItems)
                newItems.Add(new ItemX { ApiId = missingId }); // todo kann man das effizienter lösen? problem: per groupBy kriegt man Vorzeichen nicht raus 

            var items = new List<ItemX>();
            items.AddRange(newItems);
            items.AddRange(oldItems);
            var farmedItems = items
                .GroupBy(i => i.ApiId)
                .Select(g =>
                    new ItemX
                    {
                        ApiId = g.Key,
                        Count = g.First().Count - g.Last().Count,
                    })
                .Where(i => i.Count != 0)
                .ToList();
            return farmedItems;
        }

        private void UpdateUi()
        {
            _farmedItemsFlowPanel.ClearChildren();

            foreach (var farmedItem in _farmedItems)
            {
                var itemContainer = new LocationContainer()
                {
                    WidthSizingMode = SizingMode.AutoSize,
                    HeightSizingMode = SizingMode.AutoSize,
                    Parent = _farmedItemsFlowPanel
                };

                new Image(AsyncTexture2D.FromAssetId(farmedItem.IconAssetId))
                {
                    BasicTooltipText = $"{farmedItem.Name}\n{farmedItem.Description}",
                    Size = new Point(40),
                    Parent = itemContainer
                };

                new Label
                {
                    Text = farmedItem.Count.ToString(),
                    Parent = itemContainer
                };
            }
        }

        private StandardWindow _farmingTrackerWindow;
        private FlowPanel _farmedItemsFlowPanel;

        private TrackerCornerIcon _trackerCornerIcon;
        private bool _taskIsRunning; // todo anders lösen
        private double _updateRunningTimeInMilliseconds;
        private readonly List<ItemX> _itemsWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _currenciesWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _farmedItems = new List<ItemX>();
    }
}
