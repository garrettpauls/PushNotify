using System;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace PushNotify.Framework.Xaml.Converters
{
    public sealed class StringJoinConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var objects = value as IEnumerable<object>;
            return string.Join(parameter?.ToString() ?? "", objects ?? new[] {value});
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
