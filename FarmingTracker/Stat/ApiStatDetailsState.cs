namespace FarmingTracker
{
    public enum ApiStatDetailsState
    {
        MissingBecauseApiNotCalledYet,
        MissingBecauseUnknownByApi, // e.g. some items from reknown hearts or lvl 80 boost.
        SetByApi,
        GoldCoinCustomStat,
        SilveCoinCustomStat,
        CopperCoinCustomStat,
    }
}
