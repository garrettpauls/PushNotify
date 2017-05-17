using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

using LanguageExt;

namespace PushNotify.Core.Services.Pushover.Responses
{
    public static class Json
    {
        private static DataContractJsonSerializer _CreateSerializer<TValue>() where TValue : class
        {
            var settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };
            var serializer = new DataContractJsonSerializer(typeof(TValue), settings);
            return serializer;
        }

        public static Option<TValue> Read<TValue>(string json)
            where TValue : class
        {
            try
            {
                var serializer = _CreateSerializer<TValue>();
                using(var reader = new MemoryStream(Encoding.UTF8.GetBytes(json ?? "")))
                {
                    return (TValue) serializer.ReadObject(reader);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public static string Write<TValue>(TValue target)
            where TValue : class
        {
            var serializer = _CreateSerializer<TValue>();
            using(var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, target);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
