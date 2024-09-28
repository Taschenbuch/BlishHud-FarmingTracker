using Newtonsoft.Json;
using System.Collections.Generic;

// Problem: modifing and reading of CurrencyById/ItemById (= stats) must not be done by multiple thread at the same time.
// Reason:
// - thread safety. e.g. crashes when one thread is modifing the stats while the other thread is running a foreach on them.
// - out of sync / corrupt stats because multiple threads are modifing the stats while another thread reads the stats to update the ui.
// E.g. profit is updated for half of the stats while the ui updater is already running. It will show half updated data. In that case wrong total profits.
// e.g. a thread updates the items and the currencies. But he only updated half of the items and no currencies yet and the ui updater thread already starts
// This will result in no update currencies and only half updated items in the ui.
// Solution:
// - modify stats / update snapshot: only happens in Module.Update() and only when no other thread is already doing that. 
// - Read Snapshot: To prevent that itemById and currencyById are out of sync, a local variable of StatsSnapshot has to be created first, when trying to access both.
// This data is for reading. It must not be written to. Writing is done by replacing the whole snapshot instance.
namespace FarmingTracker
{
    public class Stats
    {
        public Dictionary<int, Stat> CurrencyById { get; } = new Dictionary<int, Stat>();
        public Dictionary<int, Stat> ItemById { get; } = new Dictionary<int, Stat>();
        public StatsSnapshot StatsSnapshot { get; set; } = new StatsSnapshot();

        // Must not be called while other threads modify CurrencyById/ItemById.
        public void UpdateStatsSnapshot()
        {
            var newSnapshot = new StatsSnapshot
            {
                ItemById = ItemById,
                CurrencyById = CurrencyById
            };

            var statsSnapshot = JsonConvert.DeserializeObject<StatsSnapshot>(JsonConvert.SerializeObject(newSnapshot));

            if (statsSnapshot == null)
            {
                Module.Logger.Error("Failed to copy statsSnapshot.");
                return;
            }

            StatsSnapshot = statsSnapshot;
        }
    }
}
