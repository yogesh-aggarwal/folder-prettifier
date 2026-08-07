using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace FolderPrettifier
{
    public partial class AboutDialog : Form
    {
        private readonly Action<string> _openUrl;

        public AboutDialog(Action<string> openUrl = null)
        {
            InitializeComponent();
            _openUrl = openUrl ?? (url => Process.Start(new ProcessStartInfo(url)));
            versionLabel.Text = "Version " + Application.ProductVersion;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void feedbackButton_Click(object sender, EventArgs e)
        {
            _openUrl("https://yogeshaggarwal.in/aka/folder-prettifier-feedback");
        }
    }
}
