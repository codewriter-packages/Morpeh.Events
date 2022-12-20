using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    internal class EventRegistry
    {
        internal readonly IntHashMap<IEventInternal> RegisteredEvents = new IntHashMap<IEventInternal>();

        internal FastList<IEventInternal> DispatchedEvents = new FastList<IEventInternal>();
        internal FastList<IEventInternal> ExecutingEvents = new FastList<IEventInternal>();
    }
}