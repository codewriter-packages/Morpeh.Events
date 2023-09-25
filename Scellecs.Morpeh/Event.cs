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
        public readonly FastList<TData> publishedChanges = new FastList<TData>();
        public readonly FastList<TData> scheduledChanges = new FastList<TData>();

        public bool isPublished;
        public bool isScheduled;

#if !MORPEH_STRICT_MODE
#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use publishedChanges instead.")]
#endif
        [PublicAPI]
        public FastList<TData> BatchedChanges => publishedChanges;

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use scheduledChanges instead.")]
#endif
        [PublicAPI]
        public FastList<TData> ScheduledChanges => scheduledChanges;

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use isPublished instead.")]
#endif
        [PublicAPI]
        public bool IsPublished => isPublished;

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use isScheduled instead.")]
#endif
        [PublicAPI]
        public bool IsScheduled => isScheduled;
#endif

        internal event Action<FastList<TData>> Callback;

        [PublicAPI]
        public void NextFrame(TData data)
        {
            scheduledChanges.Add(data);

            if (!isPublished && !isScheduled)
            {
                registry.DispatchedEvents.Add(this);
            }

            isScheduled = true;
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
            if (isPublished)
            {
                if (Callback != null)
                {
                    TryCatchInvokeCallback();
                    ForwardInvokeCallback();
                }

                isPublished = false;
                publishedChanges.Clear();
            }

            if (isScheduled)
            {
                isPublished = true;
                isScheduled = false;
                publishedChanges.AddListRange(scheduledChanges);
                scheduledChanges.Clear();

                registry.DispatchedEvents.Add(this);
            }
        }

        [Conditional("MORPEH_DEBUG_DISABLED")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ForwardInvokeCallback()
        {
            Callback?.Invoke(publishedChanges);
        }

        [Conditional("MORPEH_DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TryCatchInvokeCallback()
        {
            try
            {
                Callback?.Invoke(publishedChanges);
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