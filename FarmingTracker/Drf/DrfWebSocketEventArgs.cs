using System;

namespace FarmingTracker
{
    public  class DrfWebSocketEventArgs : EventArgs
    {
        public DrfWebSocketEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
