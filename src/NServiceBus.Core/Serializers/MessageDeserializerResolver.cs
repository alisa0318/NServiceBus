﻿namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using Serialization;

    class MessageDeserializerResolver
    {
        public MessageDeserializerResolver(IMessageSerializer defaultSerializer, IEnumerable<IMessageSerializer> additionalDeserializers)
        {
            this.defaultSerializer = defaultSerializer;
            serializersMap = additionalDeserializers.ToDictionary(key => key.ContentType, value => value);
        }

        public IMessageSerializer Resolve(Dictionary<string, string> headers)
        {
            string contentType;
            if (headers.TryGetValue(Headers.ContentType, out contentType))
            {
                IMessageSerializer serializer;
                if (serializersMap.TryGetValue(contentType, out serializer))
                {
                    return serializer;
                }
            }
            return defaultSerializer;
        }

        IMessageSerializer defaultSerializer;
        Dictionary<string, IMessageSerializer> serializersMap;
    }
}