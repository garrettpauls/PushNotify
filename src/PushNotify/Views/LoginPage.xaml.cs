using Windows.UI.Xaml;

using PushNotify.Framework.Xaml;

namespace PushNotify.Views
{
    public sealed partial class LoginPage : LoginPageImpl
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void _HandleLoaded(object sender, RoutedEventArgs e)
        {
            EmailTextBox.Focus(FocusState.Keyboard);
        }
    }

    public abstract class LoginPageImpl : Page<LoginPageViewModel>
    {
    }
}
