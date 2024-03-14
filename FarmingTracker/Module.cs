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
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmingTracker // todo x rename (überall dann anpassen
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public class Module : Blish_HUD.Modules.Module // todo x rename
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

        protected override void DefineSettings(SettingCollection settings)
        {
           
        }

        protected override async Task LoadAsync()
        {
            _exampleWindow = new StandardWindow(
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

            _exampleWindow.Show();

            _farmedItemsFlowPanel = new FlowPanel()
            {
                Title = "farmed items",
                FlowDirection = ControlFlowDirection.LeftToRight,
                CanCollapse = true,
                Width = 862,
                HeightSizingMode = SizingMode.AutoSize,
                Parent = _exampleWindow
            };
        }
        
        protected override void Update(GameTime gameTime)
        {
            _updateRunningTimeInMilliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_updateRunningTimeInMilliseconds > 5 * 1000)
            {
                _updateRunningTimeInMilliseconds = 0;

                if (!Gw2ApiManager.HasPermissions(new List<TokenPermission> { TokenPermission.Account })) // todo x sauberer lösen
                {
                    Logger.Debug("token waiting..."); // todo x weg
                    return;
                }

                Logger.Debug("TrackItems try..."); // todo x weg

                if (!_taskIsRunning)
                {
                    _taskIsRunning = true;
                    Task.Run(() => TrackItems()); // todo verriegelung, dass nicht ständig hier reingeht, obwohl vorheriger Task noch läuft
                }
                else
                {
                    Logger.Debug("TrackItems already running"); // todo x weg
                }
            }
        }

        private async void TrackItems()
        {
            Logger.Debug("TrackItems start"); // todo x weg

            try
            {
                // get all items on account
                var charactersTask = Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                var bankTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Bank.GetAsync();
                var sharedInventoryTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Inventory.GetAsync();
                var materialStorageTask = Gw2ApiManager.Gw2ApiClient.V2.Account.Materials.GetAsync();

                var apiResponseTasks = new List<Task>
                {
                    charactersTask,
                    bankTask,
                    sharedInventoryTask,
                    materialStorageTask
                };

                await Task.WhenAll(apiResponseTasks); // todo x log warn exception wie session tracker
                Logger.Debug("get items done"); // todo x weg
                var items = ItemSearchService.GetItemIdsAndCounts(charactersTask, bankTask, sharedInventoryTask, materialStorageTask);

                // initialize snapshotget all items on account
                var isFirstSnapshot = !_allAccountItemsWhenTrackingStarted.Any();
                if (isFirstSnapshot)
                {
                    Logger.Debug("set init snapshot"); // todo x weg
                    _allAccountItemsWhenTrackingStarted.AddRange(items);
                    return;
                }

                Logger.Debug("create diff"); // todo x weg
                var oldAndNewItems = new List<ItemX>();
                oldAndNewItems.AddRange(items);
                oldAndNewItems.AddRange(_allAccountItemsWhenTrackingStarted);

                var farmedItems = oldAndNewItems
                    .GroupBy(i => i.ApiItemId)
                    .Select(g =>
                        new ItemX
                        {
                            ApiItemId = g.Key,
                            Count = g.First().Count - g.Last().Count,
                        })
                    .Where(i => i.Count != 0)
                    .ToList();

                // add AssetId, Name, Description
                var farmedItemIds = farmedItems.Select(i => i.ApiItemId).ToList();
                var hasFarmedNoItems = !farmedItemIds.Any();
                if (hasFarmedNoItems)
                    return;

                var apiItems = await Gw2ApiManager.Gw2ApiClient.V2.Items.ManyAsync(farmedItemIds);
                foreach (var apiItem in apiItems)
                {
                    var matchingItem = farmedItems.Single(i => i.ApiItemId == apiItem.Id); // todo x null check danach nötig?
                    matchingItem.AssetId = int.Parse(Path.GetFileNameWithoutExtension(apiItem.Icon.Url.AbsoluteUri));
                    matchingItem.Name = apiItem.Name;
                    matchingItem.Description = apiItem.Description;
                }

                Logger.Debug("update ui"); // todo x weg
                // show items in UI
                _farmedItems.Clear();
                _farmedItems.AddRange(farmedItems);
                UpdateUi();
            }
            finally
            {
                Logger.Debug("TrackItems end"); // todo x weg
                _taskIsRunning = false;
            }
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

                new Image(AsyncTexture2D.FromAssetId(farmedItem.AssetId))
                {
                    BasicTooltipText = $"{farmedItem.Name}\n{farmedItem.Description}",
                    Parent = itemContainer
                };

                new Label
                {
                    Text = farmedItem.Count.ToString(),
                    Parent = itemContainer
                };
            }
        }

        protected override void Unload()
        {
            _exampleWindow?.Dispose();
        }

        private StandardWindow _exampleWindow;
        private FlowPanel _farmedItemsFlowPanel;
        private bool _taskIsRunning; // todo x anders lösen
        private double _updateRunningTimeInMilliseconds;
        private readonly List<ItemX> _allAccountItemsWhenTrackingStarted = new List<ItemX>();
        private readonly List<ItemX> _farmedItems = new List<ItemX>();
    }
}
