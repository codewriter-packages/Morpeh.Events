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
            var eventIdentifier = EventIdentifier<TData>.identifier;
            var registry = Registries.GetValueByKey(world.identifier);

            if (!registry.RegisteredEvents.TryGetValue(eventIdentifier, out var registeredEvent))
            {
                registeredEvent = new Event<TData>(registry);

                registry.RegisteredEvents.Add(eventIdentifier, registeredEvent, out _);
            }

            return (Event<TData>) registeredEvent;
        }
    }
}