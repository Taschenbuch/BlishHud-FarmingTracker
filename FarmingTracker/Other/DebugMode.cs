using Blish_HUD;

namespace FarmingTracker
{
    public class DebugMode // blish core already uses DebugService name
    {
        // using this as condition for debug messages prevents that complex debug messages are created though they are not logged in release mode.
        public static bool DebugLoggingRequired => VisualStudioRunningInDebugMode || GameService.Debug.EnableDebugLogging.Value;
#if DEBUG
        public const bool VisualStudioRunningInDebugMode = true;
#else 
        public const bool VisualStudioRunningInDebugMode = false;
#endif
    }
}
