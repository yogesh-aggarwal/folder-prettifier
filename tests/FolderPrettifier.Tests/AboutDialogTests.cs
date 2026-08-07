using FolderPrettifier;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class AboutDialogTests
    {
        private static T Find<T>(AboutDialog dialog, string name) where T : Control
        {
            Control[] controls = dialog.Controls.Find(name, false);
            Assert.That(controls.Length, Is.EqualTo(1), "Control '" + name + "' not found.");
            return (T)controls[0];
        }

        [Test]
        public void Constructor_SetsVersionLabelWithProductVersion()
        {
            using (AboutDialog dialog = new AboutDialog())
            {
                Label version = Find<Label>(dialog, "versionLabel");

                StringAssert.StartsWith("Version ", version.Text);
                StringAssert.EndsWith(Application.ProductVersion, version.Text);
            }
        }

        [Test]
        public void OkButton_Click_HidesDialog()
        {
            using (AboutDialog dialog = new AboutDialog())
            {
                dialog.Show();

                Assert.That(dialog.Visible, Is.True);
                Find<Button>(dialog, "okButton").PerformClick();

                Assert.That(dialog.Visible, Is.False);
            }
        }

        [Test]
        public void FeedbackButton_Click_OpensFeedbackUrl()
        {
            List<string> opened = new List<string>();
            using (AboutDialog dialog = new AboutDialog(url => opened.Add(url)))
            {
                dialog.Show();

                Find<Button>(dialog, "feedbackButton").PerformClick();

                Assert.That(opened, Does.Contain("https://yogeshaggarwal.in/aka/folder-prettifier-feedback"));
            }
        }
    }
}
