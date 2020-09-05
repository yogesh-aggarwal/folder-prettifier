using System;
using Windows.UI.Xaml.Controls;

namespace Folder_Prettifier.Dialogs
{
    public sealed partial class UserBenefits : ContentDialog
    {
        Action callback;

        public void DefaultCallback()
        {

        }

        public UserBenefits(Action callback=null)
        {
            this.InitializeComponent();
            if (callback == null) callback = DefaultCallback;
            this.callback = callback;
        }

        private void CloseDialog(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
            callback();
        }
    }
}
