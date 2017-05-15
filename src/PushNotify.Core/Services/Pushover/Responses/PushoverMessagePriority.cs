using System.Runtime.Serialization;

namespace PushNotify.Core.Services.Pushover.Responses
{
    [DataContract]
    public enum PushoverMessagePriority
    {
        [EnumMember(Value = "-2")] Lowest = -2,
        [EnumMember(Value = "-1")] Low = -1,
        [EnumMember(Value = "0")] Normal = 0,
        [EnumMember(Value = "1")] High = 1,
        [EnumMember(Value = "2")] Emergency = 2
    }
}
