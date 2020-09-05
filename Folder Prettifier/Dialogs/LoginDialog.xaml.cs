using System;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        public LoginDialog()
        {
            this.InitializeComponent();
        }

        async private void LoginUser(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            ContentDialog dialog = new ContentDialog
            {
                Title = "Login Successful",
                Content = "Login Successful",
                CloseButtonText = "Okay"
            };
            await dialog.ShowAsync();
        }

        async private void SignupUser(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            SignupDialog signup= new SignupDialog();
            await signup.ShowAsync();
        }
        async private void ReopenLogin()
        {
            await this.ShowAsync();
        }

        async private void UserBenefits(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Hide();
            UserBenefits userBenefits = new UserBenefits(ReopenLogin);
            await userBenefits.ShowAsync();
        }
    }
}
