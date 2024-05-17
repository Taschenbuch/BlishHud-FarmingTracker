using Gw2Sharp.WebApi.V2.Models;
using System.Collections.Generic;

namespace FarmingTracker
{
    // details from gw2 api. DRF has no info about that
    public class ApiStatDetails
    {
        public ApiStatDetailsState State { get; set; } = ApiStatDetailsState.MissingBecauseApiNotCalledYet;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ItemRarity Rarity { get; set; } = ItemRarity.Unknown;
        public ItemType Type { get; set; } = ItemType.Unknown;
        public ApiFlags<ItemFlag> Flag { get; set; } = new ApiFlags<ItemFlag>(new List<ApiEnum<ItemFlag>>() { ItemFlag.Unknown });
        public int IconAssetId { get; set; } = Constants.BUG_TEXTURE_ASSET_ID; // prevents that FromAssetId() returns null and crashes new Image()
    }
}
