#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class Event<TData> : EventBase where TData : struct, IEventData
    {
        [PublicAPI] public FastList<TData> BatchedChanges { get; } = new FastList<TData>();
        [PublicAPI] public FastList<TData> ScheduledChanges { get; } = new FastList<TData>();

        [PublicAPI] public bool IsPublished { get; private set; }
        [PublicAPI] public bool IsScheduled { get; private set; }

        internal event Action<FastList<TData>> Callback;

        [PublicAPI]
        public void NextFrame(TData data)
        {
            ScheduledChanges.Add(data);

            if (!IsPublished && !IsScheduled)
            {
                registry.DispatchedEvents.Add(this);
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

        internal sealed override void OnFrameEnd()
        {
            if (IsPublished)
            {
                if (Callback != null)
                {
                    TryCatchInvokeCallback();
                    ForwardInvokeCallback();
                }

                IsPublished = false;
                BatchedChanges.Clear();
            }

            if (IsScheduled)
            {
                IsPublished = true;
                IsScheduled = false;
                BatchedChanges.AddListRange(ScheduledChanges);
                ScheduledChanges.Clear();

                registry.DispatchedEvents.Add(this);
            }
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ForwardInvokeCallback()
        {
            Callback?.Invoke(BatchedChanges);
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryCatchInvokeCallback()
        {
            try
            {
                Callback?.Invoke(BatchedChanges);
            }
            catch (Exception ex)
            {
                MLogger.LogError($"Can not invoke callback Event<{typeof(TData)}>");
                MLogger.LogException(ex);
            }
        }
    }

    public abstract class EventBase
    {
        internal EventRegistry registry;

        internal abstract void OnFrameEnd();
    }
}