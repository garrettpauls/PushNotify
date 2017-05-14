using System.Collections.Specialized;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Template10.Interfaces.Validation;

namespace PushNotify.Controls
{
    public sealed class HeaderedValidationWrapper : ContentControl
    {
        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            "ErrorMessage", typeof(string), typeof(HeaderedValidationWrapper), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(HeaderedValidationWrapper), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register(
            "PropertyName", typeof(string), typeof(HeaderedValidationWrapper), new PropertyMetadata(default(string), _HandlePropertyNameChanged));

        public static readonly DependencyProperty PropertyProperty = DependencyProperty.Register(
            "Property", typeof(IProperty), typeof(HeaderedValidationWrapper), new PropertyMetadata(default(IProperty), _HandlePropertyChanged));

        public HeaderedValidationWrapper()
        {
            DefaultStyleKey = typeof(HeaderedValidationWrapper);
            DataContextChanged += (sender, args) => _UpdateProperty();
        }

        public string ErrorMessage
        {
            get { return (string) GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        public object Header
        {
            get => (object) GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public IProperty Property
        {
            get => (IProperty) GetValue(PropertyProperty);
            private set => SetValue(PropertyProperty, value);
        }

        public string PropertyName
        {
            get => (string) GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        private void _HandlePropertyChanged(IProperty oldProperty, IProperty newProperty)
        {
            if(oldProperty != null)
            {
                oldProperty.Errors.CollectionChanged -= _HandlePropertyErrorsChanged;
            }

            if(newProperty != null)
            {
                newProperty.Errors.CollectionChanged += _HandlePropertyErrorsChanged;
            }
        }

        private void _HandlePropertyErrorsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ErrorMessage = string.Join(", ", Property?.Errors.ToArray() ?? new string[0]);
        }

        private void _UpdateProperty()
        {
            Property = _GetProperty(DataContext as IValidatableModel, PropertyName);
        }

        private static IProperty _GetProperty(IValidatableModel context, string propertyName)
        {
            if(context == null)
            {
                return null;
            }

            if(context.Properties.TryGetValue(propertyName, out IProperty property))
            {
                return property;
            }

            return null;
        }

        private static void _HandlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HeaderedValidationWrapper)?._HandlePropertyChanged(e.OldValue as IProperty, e.NewValue as IProperty);
        }

        private static void _HandlePropertyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as HeaderedValidationWrapper)?._UpdateProperty();
        }
    }
}
