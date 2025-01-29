using System.Threading;

namespace FarmingTracker
{
    public class ThreadSafeLong
    {
        public long Value
        {
            get => Interlocked.Read(ref _value);
            set => Interlocked.Exchange(ref _value, value);
        }

        public void Add(long value) => Interlocked.Add(ref _value, value);
        
        private long _value;
    }
}
