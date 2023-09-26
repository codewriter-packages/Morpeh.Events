#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Scellecs.Morpeh
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class Request<TData> : RequestBase where TData : struct, IRequestData
    {
        internal readonly FastList<TData> changes = new FastList<TData>();

#if MORPEH_DEBUG
        internal int lastConsumeFrame;
#endif
        internal int lastConsumedIndex;

        [PublicAPI]
        public void Publish(in TData request, bool allowNextFrame = false)
        {
#if MORPEH_DEBUG
            if (!allowNextFrame && lastConsumeFrame == Time.frameCount)
            {
                MLogger.LogError(
                    "The request was already consumed in the current frame. " +
                    "Reorder systems or set allowNextFrame parameter");
            }
#endif

            changes.Add(request);
        }

        [PublicAPI]
        public Consumer Consume()
        {
#if MORPEH_DEBUG
            lastConsumeFrame = Time.frameCount;
#endif

            if (lastConsumedIndex > 0)
            {
                Cleanup();
            }

            Consumer consumer;
            consumer.request = this;
            return consumer;
        }

        internal void Cleanup()
        {
            if (lastConsumedIndex >= changes.length)
            {
                changes.Clear();
                lastConsumedIndex = 0;
                return;
            }

            Array.Copy(changes.data, lastConsumedIndex, changes.data, 0, changes.length - lastConsumedIndex);

            changes.length -= lastConsumedIndex;
            changes.lastSwappedIndex = -1;

            for (var i = 0; i < lastConsumedIndex; i++)
            {
                changes.data[i + changes.length] = default;
            }

            lastConsumedIndex = 0;
        }

        public struct Consumer
        {
            public Request<TData> request;

            public Enumerator GetEnumerator()
            {
                Enumerator e;
                e.request = request;
                e.current = default;
                return e;
            }
        }

        public struct Enumerator
        {
            public Request<TData> request;
            public TData current;

            public bool MoveNext()
            {
                if (request.lastConsumedIndex >= request.changes.length)
                {
                    return false;
                }

                current = request.changes.data[request.lastConsumedIndex++];
                return true;
            }

            public TData Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.current;
            }
        }
    }

    public abstract class RequestBase
    {
        internal RequestRegistry registry;
    }
}