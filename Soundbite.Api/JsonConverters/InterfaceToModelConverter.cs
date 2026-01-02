using Newtonsoft.Json;
using System;

namespace Soundbite.Api
{
    /// <summary>
    /// Defines the interface to concrete type mapping that allows the JSON deserializer to 
    /// process a request to deserialize a JSON string to an interface.
    /// </summary>
    /// <typeparam name="TInterface">.NET type of the interface that JSON deserializer must handle.</typeparam>
    /// <typeparam name="TConcrete">.NET type of the concrete type to use when the specified interface type is requested.</typeparam>
    internal class InterfaceToModelConverter<TInterface, TConcrete> : JsonConverter where TConcrete : TInterface
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">.NET type of the object.</param>
        /// <returns><c>true</c> if this instance can convert the specified object type; otherwise <c>false</c>.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TInterface);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The Newtonsoft.Json.JsonReader to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<TConcrete>(reader);
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The Newtonsoft.Json.JsonWriter to write to.</param>
        /// <param name="value">The value</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value, typeof(TConcrete));
        }
    }
}