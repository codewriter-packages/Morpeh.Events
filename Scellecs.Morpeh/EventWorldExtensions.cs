using System;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    public static class EventWorldExtensions
    {
        private static readonly IntHashMap<EventRegistry> Registries = new IntHashMap<EventRegistry>();

        internal static void SetupEventRegistry(World world)
        {
            var registry = new EventRegistry();

            var eventSystemGroup = world.CreateSystemsGroup();
            eventSystemGroup.AddSystem(new ProcessEventsSystem(registry));
            world.AddPluginSystemsGroup(eventSystemGroup);

            Registries.Add(world.identifier, registry, out _);
        }

        internal static void CleanupEventRegistry(World world)
        {
            Registries.Remove(world.identifier, out _);
        }

        [PublicAPI]
        public static Event<TData> GetEvent<TData>(this World world)
            where TData : struct, IEventData
        {
            var type = typeof(TData);
            var registry = Registries.GetValueByKey(world.identifier);

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
            var registry = Registries.GetValueByKey(world.identifier);

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