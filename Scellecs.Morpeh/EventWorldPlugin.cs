using UnityEngine.Scripting;

namespace Scellecs.Morpeh
{
    [Preserve]
    internal class EventWorldPlugin : IWorldPlugin
    {
        [Preserve]
        public EventWorldPlugin()
        {
        }

        [Preserve]
        public void Initialize(World world)
        {
            EventWorldExtensions.SetupEventRegistry(world);
        }
    }
}