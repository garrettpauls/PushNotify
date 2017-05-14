using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public sealed class LoginResponse : PushoverResponse
    {
        [DataMember(Name = "secret")]
        public string Secret { get; set; }
    }
}
