using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Masticore
{
    /// <summary>
    /// Provides helpers for uniform serialization behavior across the app
    /// </summary>
    public static class JsonUtils

    {
        /// <summary>
        /// Returns the lowerCamel JSON of the given object
        /// </summary>
        /// <remarks>If the given object is null, then this will throw a <see cref="ArgumentNullException"/></remarks>
        /// <param name="objectToSerializeToJson"></param>
        /// <param name="throwOnNullReference">Optional flag indicating whether to throw an exception if the <paramref name="objectToSerializeToJson"/> is <c>null</c>. Default is <c>true</c></param>
        /// <param name="forceTypeInfo">Optional flag that sets the type name handling to default on for all objects. (default is <c>false</c>).</param>
        /// <returns>a string containing the JSON representation of <paramref name="objectToSerializeToJson"/> or <c>null</c> if <paramref name="objectToSerializeToJson"/> is <c>null</c> and <paramref name="throwOnNullReference"/> is <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="objectToSerializeToJson"/> is null and null reference are not allowed.</exception>
        public static string ToLowerCamelJson(this object objectToSerializeToJson, bool throwOnNullReference = true, bool forceTypeInfo = false)
        {
            if (objectToSerializeToJson is null)
            {
                if (throwOnNullReference)
                {
                    throw new ArgumentNullException(nameof(objectToSerializeToJson));
                }
                else
                {
                    return null;
                }
            }

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = forceTypeInfo ? TypeNameHandling.All : TypeNameHandling.Auto
            };

            string json = JsonConvert.SerializeObject(objectToSerializeToJson, serializerSettings);
            return json;
        }

        /// <summary>
        /// Instantiates a new <typeparamref name="T"/> instance based on the specifeid JSON using
        /// settings that assume lower-cased property names.
        /// </summary>
        /// <typeparam name="T">.NET type of the object to create from the JSON.</typeparam>
        /// <param name="json">JSON string from which to construct the object.</param>
        /// <param name="throwIfNull">Flag to indicate if an exception should be thrown when json is null</param>
        /// <returns>an instance of <typeparamref name="T"/> populated from the specified JSON.</returns>
        public static T FromLowerCamelJson<T>(string json, bool throwIfNull = true)
        {
            // Let null slip through if they said it's ok
            if (json == null && !throwIfNull)
            {
                return default(T);
            }

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
            };
            return JsonConvert.DeserializeObject<T>(json, serializerSettings);
        }

        /// <summary>
        /// Instantiates a new <typeparamref name="T"/> instance based on the specifeid JSON.
        /// </summary>
        /// <typeparam name="T">.NET type of the object to create from the JSON.</typeparam>
        /// <param name="json">JSON string from which to construct the object.</param>
        /// <returns>an instance of <typeparamref name="T"/> populated from the specified JSON.</returns>
        public static T FromJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Provides a way to quickly operate on an object that is stored as JSON. The JSON that is 
        /// passed into the function is deserialized and passed into a method where it can be 
        /// operated on before being reserialized and returned as the result of this function.
        /// </summary>
        /// <typeparam name="T">.NET type of the object serialized to JSON.</typeparam>
        /// <param name="json">JSON representation of the object.</param>
        /// <param name="action">Actions to perform on the object before re-serialization.</param>
        /// <param name="create">Optional function that creates the object if JSON is null/empty or deserializes to a null instance.</param>
        /// <returns>a JSON string with any updates to the object applied.</returns>
        public static string WithJson<T>(string json, Action<T> action, Func<T> create = null)
            where T : class, new()
        {
            T item = null;

            // Attempt to parse the JSON if it exists
            if (!string.IsNullOrEmpty(json))
            {
                item = FromLowerCamelJson<T>(json);
            }

            // Ensure there is an object with which to work
            item = item ?? create?.Invoke() ?? new T();

            // Work with the object and re-serialize it when done
            action?.Invoke(item);
            json = item.ToLowerCamelJson();
            return json;
        }
    }
}
