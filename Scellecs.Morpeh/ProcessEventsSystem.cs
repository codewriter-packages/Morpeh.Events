using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class ProcessEventsSystem : ICleanupSystem
    {
        private readonly EventRegistry _registry;

        public World World { get; set; }

        public ProcessEventsSystem(EventRegistry registry)
        {
            _registry = registry;
        }

        public void OnAwake()
        {
        }

        public void Dispose()
        {
        }

        public void OnUpdate(float deltaTime)
        {
            var list = _registry.DispatchedEvents;
            if (list.length == 0)
            {
                return;
            }

            _registry.DispatchedEvents = _registry.ExecutingEvents;
            _registry.ExecutingEvents = list;

            foreach (var evt in list)
            {
                evt.OnFrameEnd();
            }

            list.Clear();
        }
    }
}