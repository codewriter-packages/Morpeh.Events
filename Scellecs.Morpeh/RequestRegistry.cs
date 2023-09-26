using System;
using System.Collections.Generic;

namespace Scellecs.Morpeh
{
    internal class RequestRegistry
    {
        internal readonly Dictionary<Type, RequestBase> RegisteredRequests = new Dictionary<Type, RequestBase>();
    }
}