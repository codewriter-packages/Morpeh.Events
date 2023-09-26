using System;
using JetBrains.Annotations;

namespace Scellecs.Morpeh
{
    public static class RequestWorldExtensions
    {
        [PublicAPI]
        public static Request<TData> GetRequest<TData>(this World world)
            where TData : struct, IRequestData
        {
            var type = typeof(TData);
            var registry = world.CodeWriterRequestsRegistry;

            if (registry.RegisteredRequests.TryGetValue(type, out var registeredRequest))
            {
                return (Request<TData>) registeredRequest;
            }

            registeredRequest = new Request<TData>();
            registeredRequest.registry = registry;

            registry.RegisteredRequests.Add(type, registeredRequest);

            return (Request<TData>) registeredRequest;
        }

        [PublicAPI]
        public static RequestBase GetReflectionRequest(this World world, Type type)
        {
            var registry = world.CodeWriterRequestsRegistry;

            if (registry.RegisteredRequests.TryGetValue(type, out var registeredRequest))
            {
                return registeredRequest;
            }

            var constructedType = typeof(Request<>).MakeGenericType(type);
            registeredRequest = (RequestBase) Activator.CreateInstance(constructedType, true);
            registeredRequest.registry = registry;

            registry.RegisteredRequests.Add(type, registeredRequest);

            return registeredRequest;
        }
    }
}