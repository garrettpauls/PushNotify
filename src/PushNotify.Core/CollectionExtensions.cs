using System.Collections.Generic;

namespace PushNotify.Core
{
    public static class CollectionExtensions
    {
        public static TValue AddTo<TValue>(this TValue value, ICollection<TValue> target)
        {
            target.Add(value);
            return value;
        }
    }
}
