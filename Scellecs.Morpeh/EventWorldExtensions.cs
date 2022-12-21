using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    public static class EventWorldExtensions
    {
        private static readonly IntHashMap<EventRegistry> Registries = new IntHashMap<EventRegistry>();

        internal static void CleanupEventRegistry(World world)
        {
            var worldIdentifier = world.identifier;

            Registries.Remove(worldIdentifier, out _);
        }

        [PublicAPI]
        public static Event<TData> GetEvent<TData>(this World world)
            where TData : struct, IEventData
        {
            var worldIdentifier = world.identifier;
            var eventIdentifier = EventIdentifier<TData>.identifier;

            if (!Registries.TryGetValue(worldIdentifier, out var registry))
            {
                registry = new EventRegistry();

                var eventSystemGroup = world.CreateSystemsGroup();
                eventSystemGroup.AddSystem(new ProcessEventsSystem(registry));
                world.AddSystemsGroup(9999, eventSystemGroup);

                Registries.Add(worldIdentifier, registry, out _);
            }

            if (!registry.RegisteredEvents.TryGetValue(eventIdentifier, out var registeredEvent))
            {
                registeredEvent = new Event<TData>(registry);

                registry.RegisteredEvents.Add(eventIdentifier, registeredEvent, out _);
            }

            return (Event<TData>) registeredEvent;
        }
    }
}