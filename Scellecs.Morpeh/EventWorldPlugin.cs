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
            EventWorldExtensions.SetupEventRegistry(world);
        }

        public void Deinitialize(World world)
        {
            EventWorldExtensions.CleanupEventRegistry(world);
        }
    }
}