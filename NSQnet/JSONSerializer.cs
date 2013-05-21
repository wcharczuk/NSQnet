using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace NSQnet
{
    /// <summary>
    /// Represents a json array.
    /// </summary>
    public sealed class JsonArray : List<object>
    {
        /// <summary>
        /// Json string representation of <see cref="T:Destrier.JsonArray" />.
        /// </summary>
        /// <returns>
        /// Returns the Json string representation of <see cref="T:Destrier.JsonArray" />.
        /// </returns>
        public override string ToString()
        {
            string result = JsonSerializer.Current.SerializeObject(this);
            string text = result ?? string.Empty;
            return text;
        }
    }

    /// <summary>
    /// Class to convert json text to objects and back again.
    /// </summary>
    public sealed class JsonSerializer
    {
        private static JsonSerializer _current = null;
        private static object _initlock = new object();
        public static JsonSerializer Current
        {
            get
            {
                if (_current == null)
                {
                    lock (_initlock)
                    {
                        _current = new JsonSerializer();
                    }
                }

                return _current;
            }
        }

        public JsonSerializer()
        {
            IsoDateTimeConverter isoDate = new IsoDateTimeConverter();
            isoDate.DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.Converters = (_serializerSettings.Converters ?? new List<JsonConverter>());
            _serializerSettings.Converters.Add(isoDate);
            _serializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            _serializerSettings.NullValueHandling = NullValueHandling.Include;
            _serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            _serializerSettings.TypeNameHandling = TypeNameHandling.None;
            _serializerSettings.ConstructorHandling = ConstructorHandling.Default;
        }

        private JsonSerializerSettings _serializerSettings { get; set; }

        /// <summary>
        /// Converts the <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Object" /></summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// Returns the object.
        /// </returns>
        private static object ConvertJTokenToDictionary(JToken token)
        {
            if (token == null)
            {
                return null;
            }

            //single value
            JValue jValue = token as JValue;
            if (jValue != null)
            {
                return jValue.Value;
            }

            //an array of values
            JArray jContainer = token as JArray;
            if (jContainer != null)
            {
                JsonArray jsonList = new JsonArray();
                foreach (JToken arrayItem in (IEnumerable<JToken>)jContainer)
                {
                    jsonList.Add(ConvertJTokenToDictionary(arrayItem));
                }
                return jsonList;
            }

            //a sub object
            AgileObject jsonObject = new AgileObject();
            IDictionary<string, object> jsonDict = jsonObject;
            (
                from childToken in token
                where childToken is JProperty
                select childToken as JProperty).ToList<JProperty>().ForEach(delegate(JProperty property)
                {
                    jsonDict.Add(property.Name, ConvertJTokenToDictionary(property.Value));
                }
            );
            return jsonObject;
        }

        /// <summary>
        /// Serializes the object to json string.
        /// </summary>
        /// <param name="obj">
        /// The value.
        /// </param>
        /// <returns>
        /// The json string.
        /// </returns>
        public string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, this._serializerSettings);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns />
        public T DeserializeObject<T>(string json)
        {
            return (T)this.DeserializeObject(json, typeof(T));
        }

        /// <summary>
        /// Deserialize the json string to object.
        /// </summary>
        /// <param name="json">
        /// The json string.
        /// </param>
        /// <returns>
        /// The object.
        /// </returns>
        public object DeserializeObject(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            object obj;
            try
            {
                obj = JsonConvert.DeserializeObject(json, _serializerSettings);
            }
            catch (JsonSerializationException ex)
            {
                throw new SerializationException(ex.Message, ex);
            }
            JToken jToken = obj as JToken;
            if (jToken != null)
            {
                return ConvertJTokenToDictionary(jToken);
            }
            return obj;
        }

        /// <summary>
        /// Deserializes the json string.
        /// </summary>
        /// <param name="json">
        /// The json string.
        /// </param>
        /// <param name="type">
        /// The type of object.
        /// </param>
        /// <returns>
        /// The object.
        /// </returns>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// Occurs when deserialization fails.
        /// </exception>
        public object DeserializeObject(string json, Type type)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            object obj;
            try
            {
                obj = JsonConvert.DeserializeObject(json, type, _serializerSettings);
            }
            catch (JsonSerializationException ex)
            {
                throw new SerializationException(ex.Message, ex);
            }
            JToken jToken = obj as JToken;
            if (jToken != null)
            {
                return ConvertJTokenToDictionary(jToken);
            }
            return obj;
        }

        public static bool IsJson(string text)
        {
            var trimmed = text.Trim();

            if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            {
                try
                {
                    Newtonsoft.Json.Schema.JsonSchema.Parse(text);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
                return false;
        }
    }
}
