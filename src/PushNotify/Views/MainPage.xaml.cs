using PushNotify.Framework.Xaml;

namespace PushNotify.Views
{
    public sealed partial class MainPage : MainPageImpl
    {
        public MainPage()
        {
            InitializeComponent();
        }
    }

    public abstract class MainPageImpl : Page<MainPageViewModel>
    {
    }
}
