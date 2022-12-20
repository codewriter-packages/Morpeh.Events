using System.Threading;

namespace Scellecs.Morpeh
{
    internal class EventIdentifier<TData> where TData : struct, IEventData
    {
        public static int identifier;

        static EventIdentifier()
        {
            identifier = CommonEventIdentifier.GetID();
        }
    }

    internal class CommonEventIdentifier
    {
        private static int _counter;

        public static int GetID()
        {
            return Interlocked.Increment(ref _counter);
        }
    }
}