using System;

namespace FarmingTracker
{
    [Serializable]
    public class Gw2ApiException : Exception
    {
        public Gw2ApiException(string message) 
            : base(message) { }

        public Gw2ApiException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
