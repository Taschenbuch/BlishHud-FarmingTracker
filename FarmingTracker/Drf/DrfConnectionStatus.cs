namespace FarmingTracker
{
    public enum DrfConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        AuthenticationFailed,
        TryReconnect,
        ModuleError,
    }
}
