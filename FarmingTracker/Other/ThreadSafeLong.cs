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
        public void Add(ThreadSafeLong threadSafeLong) => Interlocked.Add(ref _value, threadSafeLong.Value);
        public override string ToString() => Value.ToString();

        private long _value;
    }
}
