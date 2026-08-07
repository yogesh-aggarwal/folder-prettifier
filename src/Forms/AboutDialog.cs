using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace FolderPrettifier
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            versionLabel.Text = "Version " + Application.ProductVersion;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void feedbackButton_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://yogeshaggarwal.in");
            Process.Start(sInfo);
        }
    }
}
