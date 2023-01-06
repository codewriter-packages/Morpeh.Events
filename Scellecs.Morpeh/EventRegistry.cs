using System;
using System.Collections.Generic;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    internal class EventRegistry
    {
        internal readonly Dictionary<Type, EventBase> RegisteredEvents = new Dictionary<Type, EventBase>();

        internal FastList<EventBase> DispatchedEvents = new FastList<EventBase>();
        internal FastList<EventBase> ExecutingEvents = new FastList<EventBase>();
    }
}