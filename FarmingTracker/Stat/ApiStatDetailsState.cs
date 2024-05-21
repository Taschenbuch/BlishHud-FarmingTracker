namespace FarmingTracker
{
    public enum ApiStatDetailsState
    {
        MissingBecauseApiNotCalledYet,
        MissingBecauseUnknownByApi, // e.g. some items from reknown hearts
        SetByApi
    }
}
