using System;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh
{
    public class Event<TData> : IEventInternal where TData : struct, IEventData
    {
        private readonly EventRegistry _registry;

        [PublicAPI] public FastList<TData> BatchedChanges { get; } = new FastList<TData>();
        [PublicAPI] public FastList<TData> ScheduledChanges { get; } = new FastList<TData>();

        [PublicAPI] public bool IsPublished { get; private set; }
        [PublicAPI] public bool IsScheduled { get; private set; }

        internal event Action<FastList<TData>> Callback;

        internal Event(EventRegistry registry)
        {
            _registry = registry;
        }

        [PublicAPI]
        public void NextFrame(TData data)
        {
            ScheduledChanges.Add(data);

            if (!IsPublished && !IsScheduled)
            {
                _registry.DispatchedEvents.Add(this);
            }

            IsScheduled = true;
        }

        [PublicAPI]
        public IDisposable Subscribe(Action<FastList<TData>> callback)
        {
            return new Subscription(this, callback);
        }

        internal class Subscription : IDisposable
        {
            private readonly Event<TData> _owner;
            private readonly Action<FastList<TData>> _callback;

            public Subscription(Event<TData> owner, Action<FastList<TData>> callback)
            {
                _owner = owner;
                _callback = callback;

                _owner.Callback += _callback;
            }

            public void Dispose()
            {
                _owner.Callback -= _callback;
            }
        }

        public void OnFrameEnd()
        {
            if (IsPublished)
            {
                Callback?.Invoke(BatchedChanges);

                IsPublished = false;
                BatchedChanges.Clear();
            }

            if (IsScheduled)
            {
                IsPublished = true;
                IsScheduled = false;
                BatchedChanges.AddListRange(ScheduledChanges);
                ScheduledChanges.Clear();

                _registry.DispatchedEvents.Add(this);
            }
        }
    }

    public interface IEventInternal
    {
        void OnFrameEnd();
    }
}