namespace FarmingTracker
{
    public enum StatDetailsState
    {
        MissingBecauseApiNotCalledYet,
        MissingBecauseUnknownByApi, // e.g. some items from reknown hearts
        SetByApi
    }
}
