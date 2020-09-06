using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Dialogs
{
    public sealed partial class SignupDialog : ContentDialog
    {
        public SignupDialog()
        {
            this.InitializeComponent();
        }

        async private void SignupUser(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            ContentDialog dialog = new ContentDialog
            {
                Title = "Signup Successful",
                Content = "Signup Successful",
                CloseButtonText = "Okay"
            };
            await dialog.ShowAsync();
        }

        async private void LoginUser(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            LoginDialog login = new LoginDialog();
            await login.ShowAsync();
        }

        async private void ReopenSignup()
        {
            await this.ShowAsync();
        }

        async private void UserBenefits(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Hide();
            UserBenefits userBenefits = new UserBenefits(ReopenSignup);
            await userBenefits.ShowAsync();
        }
    }
}
