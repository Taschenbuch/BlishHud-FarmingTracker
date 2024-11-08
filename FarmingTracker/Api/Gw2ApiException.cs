using System;
using System.Runtime.Serialization;

namespace FarmingTracker
{
    [Serializable]
    public class Gw2ApiException : Exception
    {
        public Gw2ApiException(string message) 
            : base(message) { }

        public Gw2ApiException(string message, Exception innerException)
            : base(message, innerException) { }

        protected Gw2ApiException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
