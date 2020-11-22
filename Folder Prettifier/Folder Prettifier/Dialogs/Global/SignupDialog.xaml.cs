using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

        async private void UserBenefits(object sender, RoutedEventArgs e)
        {
            this.Hide();
            UserBenefits userBenefits = new UserBenefits(ReopenSignup);
            await userBenefits.ShowAsync();
        }
    }
}
