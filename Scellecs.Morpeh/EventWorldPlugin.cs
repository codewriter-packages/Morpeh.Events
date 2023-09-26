using UnityEngine;
using UnityEngine.Scripting;

namespace Scellecs.Morpeh
{
    [Preserve]
    internal sealed class EventWorldPlugin : IWorldPlugin
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void RuntimeInitialize()
        {
            WorldExtensions.AddWorldPlugin(new EventWorldPlugin());
        }

        [Preserve]
        public void Initialize(World world)
        {
            world.CodeWriterEventsRegistry = new EventRegistry();
            world.CodeWriterRequestsRegistry = new RequestRegistry();

            var eventSystemGroup = world.CreateSystemsGroup();
            eventSystemGroup.AddSystem(new ProcessEventsSystem(world.CodeWriterEventsRegistry));
            world.AddPluginSystemsGroup(eventSystemGroup);
        }

        public void Deinitialize(World world)
        {
        }
    }
}