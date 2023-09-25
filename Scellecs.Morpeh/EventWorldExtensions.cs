using System;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    public static class EventWorldExtensions
    {
        [PublicAPI]
        public static Event<TData> GetEvent<TData>(this World world)
            where TData : struct, IEventData
        {
            var type = typeof(TData);
            var registry = world.CodeWriterEventsRegistry;

            if (registry.RegisteredEvents.TryGetValue(type, out var registeredEvent))
            {
                return (Event<TData>) registeredEvent;
            }

            registeredEvent = new Event<TData>();
            registeredEvent.registry = registry;

            registry.RegisteredEvents.Add(type, registeredEvent);

            return (Event<TData>) registeredEvent;
        }

        [PublicAPI]
        public static EventBase GetReflectionEvent(this World world, Type type)
        {
            var registry = world.CodeWriterEventsRegistry;

            if (registry.RegisteredEvents.TryGetValue(type, out var registeredEvent))
            {
                return registeredEvent;
            }

            var constructedType = typeof(Event<>).MakeGenericType(type);
            registeredEvent = (EventBase) Activator.CreateInstance(constructedType, true);
            registeredEvent.registry = registry;

            registry.RegisteredEvents.Add(type, registeredEvent);

            return registeredEvent;
        }
    }
}