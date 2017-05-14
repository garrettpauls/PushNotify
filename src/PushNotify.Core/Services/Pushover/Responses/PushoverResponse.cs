using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public abstract class PushoverResponse
    {
        [IgnoreDataMember]
        public bool IsSuccessful => Status == 1;

        [DataMember(Name = "status")]
        public int Status { get; set; }
    }
}
