using System;

namespace FarmingTracker
{
    public class GenericEventArgs<T> : EventArgs
    {
        public GenericEventArgs(T data)
        {
            Data = data;
        }

        public T Data { get; }
    }
}
