using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace FarmingTracker
{
    // details from gw2 api. DRF has no info about that.
    public class ApiStatDetails
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemRarity Rarity { get; set; } = ItemRarity.Unknown;
        public ItemType Type { get; set; } = ItemType.Unknown;
        public ApiFlags<ItemFlag> ItemFlags { get; set; } = new ApiFlags<ItemFlag>(new List<ApiEnum<ItemFlag>>() { ItemFlag.Unknown });
        public int IconAssetId { get; set; } = TextureService.MISSING_ASSET_ID;
        public long VendorValueInCopper { get; set; }
        public long SellsUnitPriceInCopper { get; set; }
        public long BuysUnitPriceInCopper { get; set; }
        public string WikiSearchTerm { get; set; } = string.Empty;
        public bool HasWikiSearchTerm => !string.IsNullOrWhiteSpace(WikiSearchTerm);
        public ApiStatDetailsState State { get; set; } = ApiStatDetailsState.MissingBecauseApiNotCalledYet;
        public bool IsCustomCoinStat =>
            State == ApiStatDetailsState.GoldCoinCustomStat
            || State == ApiStatDetailsState.SilveCoinCustomStat
            || State == ApiStatDetailsState.CopperCoinCustomStat;
    }
}
