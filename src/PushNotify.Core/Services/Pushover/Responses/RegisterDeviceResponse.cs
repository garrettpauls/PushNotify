using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public sealed class RegisterDeviceResponse : PushoverResponse
    {
        [DataMember(Name = "errors")]
        public Dictionary<string, string[]> Errors { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }
    }
}
