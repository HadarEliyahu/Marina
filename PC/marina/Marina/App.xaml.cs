using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Firebase.Auth;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.IO;

namespace Marina
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public FirebaseClient _FirebaseClient;
        public FirebaseAuthLink _FirebaseAuth;

        public void ShowToastNotification(string title, string stringContent)
        {
            try
            {


                // Get a toast XML template
                XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                // Fill in the text elements
                XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
                stringElements[0].AppendChild(toastXml.CreateTextNode("Marina"));
                stringElements[1].AppendChild(toastXml.CreateTextNode(title));
                stringElements[2].AppendChild(toastXml.CreateTextNode(stringContent));

                // Specify the absolute path to an image
                String imagePath = "file:///" + Path.GetFullPath("toastImageAndText.png");
                XmlNodeList imageElements = toastXml.GetElementsByTagName("image");

                ToastNotification toast = new ToastNotification(toastXml);


                ToastNotificationManager.CreateToastNotifier(System.Reflection.Assembly.GetEntryAssembly().Location).Show(toast);

            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
