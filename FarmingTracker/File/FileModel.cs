using System.Collections.Generic;

namespace FarmingTracker
{
    // WARNING:
    // when this is changed for a new release, it has to be migrated to not crash when old fileModel is loaded from file.
    // the model.json name may need an update too (e.g. modelV2.json), to prevent that downgrading to an older module version
    // triggers a crash because the newer model.json  cannot be loaded by the older module version.
    // by using different names for different fileModel versions a version property may not be required here.
    public class FileModel
    {
        public List<FileStat> FileItems { get; set; } = new List<FileStat>();
        public List<FileStat> FileCurrencies { get; set; } = new List<FileStat>();
        public List<int> IgnoredItemApiIds { get; set; } = new List<int>();
        public List<int> FavoriteItemApiIds { get; set; } = new List<int>();
        public List<CustomStatProfit> CustomStatProfits { get; set; } = new List<CustomStatProfit>();
    }
}
