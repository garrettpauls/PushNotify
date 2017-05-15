using System;
using PushNotify.Core.Services.Pushover.Responses;

namespace PushNotify.Core.Services.Pushover
{
    public interface IPushoverMessage
    {
        DateTimeOffset Date { get; }

        string Icon { get; }

        int Id { get; }

        bool IsHtml { get; }

        string Message { get; }

        PushoverMessagePriority Priority { get; }

        string SendingApp { get; }

        string Sound { get; }

        string SupplementaryUrl { get; }

        string SupplementaryUrlTitle { get; }

        string Title { get; }
    }
}
