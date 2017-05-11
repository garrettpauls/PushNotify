using Template10.Services.NavigationService;

namespace PushNotify.Framework.Xaml
{
    public interface IHasViewModel<TViewModel>
        where TViewModel : INavigable
    {
        TViewModel ViewModel { get; }
    }
}
