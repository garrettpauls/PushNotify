using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Template10.Services.NavigationService;

namespace PushNotify.Framework.Xaml
{
    public abstract class Page<TViewModel> : Page, IHasViewModel<TViewModel>
        where TViewModel : INavigable
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel", typeof(TViewModel), typeof(Page<TViewModel>), new PropertyMetadata(default(TViewModel)));

        protected Page()
        {
            DataContextChanged += _HandleDataContextChanged;
        }

        public TViewModel ViewModel
        {
            get => (TViewModel) GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        private void _HandleDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if(DataContext is TViewModel)
            {
                ViewModel = (TViewModel) DataContext;
            }
        }
    }
}
