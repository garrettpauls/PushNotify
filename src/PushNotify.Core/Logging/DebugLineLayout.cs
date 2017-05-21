using System;
using System.Text;

using MetroLog;
using MetroLog.Layouts;

namespace PushNotify.Core.Logging
{
    public sealed class DebugLineLayout : Layout
    {
        public override string GetFormattedString(LogWriteContext context, LogEventInfo info)
        {
            var builder = new StringBuilder();
            builder.Append(info.Level.ToString().ToUpper());
            builder.Append("|");
            builder.Append(Environment.CurrentManagedThreadId);
            builder.Append("|");
            builder.Append(info.Logger);
            builder.Append("|");
            builder.Append(info.Message);
            if(info.Exception != null)
            {
                builder.Append(" --> ");
                builder.Append(info.Exception);
            }

            return builder.ToString();
        }
    }
}
