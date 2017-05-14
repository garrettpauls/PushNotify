using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PushNotify.Core.Services.Pushover.Responses
{
    public static class Json
    {
        public static TValue Read<TValue>(string json)
            where TValue : class
        {
            try
            {
                var settings = new DataContractJsonSerializerSettings
                {
                    UseSimpleDictionaryFormat = true
                };
                var serializer = new DataContractJsonSerializer(typeof(TValue), settings);
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
    }
}
