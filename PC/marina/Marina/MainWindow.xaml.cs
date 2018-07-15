using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Web;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Input;
using System.IO;
using System.Windows.Media;
using Marina.Classes;
using System.Windows.Controls;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Firebase.Auth;
using Windows.ApplicationModel;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Microsoft.Win32;

namespace Marina
{
    public partial class MainWindow : Window
    {

        private void AddAppOnStartUp()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Window\\CurrentVersion\\Run", true);
            key.SetValue("Marina", System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void RemoveAppOnStartUp()
        {
            Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Window\\CurrentVersion\\Run", true);
            key.SetValue("Marina", false);
        }

        // dictuinary of functions
        public static Dictionary<string, Action<StackPanel, Dictionary<string, Entity>>> Functions = new Dictionary<string, Action<StackPanel, Dictionary<string, Entity>>>()
        {
            { "Time.What", Execute.ShowTime },
            { "OpenApp", Execute.OpenApp },
            { "Search", Execute.Search },
            { "FeaturesList", Execute.FeaturesList },
            { "PlayMusic", Execute.PlayMusic },
            { "SignIn", Execute.SignIn },
            { "ShutDown", Execute.ShutDown },
            { "Call", Execute.Call },
            { "CloseApp", Execute.CloseApp },
            { "CreateShortCut", Execute.CreateShortCut },
            { "Clear", Execute.clearScreen },
        };


        // TextBox focus variable - helps to handle the hint
        bool BoxFocus = false;


        // Firebase Variables
        public const string FirebaseAppUri = "https://marina-a2458.firebaseio.com";
        public const string FirebaseAppKey = "AIzaSyD7wew7GJT9ewLUtXwMeagt7PIW-FamFEo";
        public IDisposable subscribtion = null;
        App global = Application.Current as App;

        public struct MessageToFirebase
        {
            public string message;
        };


        // I'm not sure about that - used to update the display
        private delegate void NoArgDelegate();


        // Tray icon tooltip
        public string TBText { get; set; }

        // messages types
        public const string ANSWER_STYLE = "Answer";
        public const string REQUEST_STYLE = "Request";

        // this function pushes messages to the chat view
        public void PushMessage(string text, string AnsOrReq)
        {
            Label queryLabel = new Label();
            queryLabel.Style = (Style)FindResource(AnsOrReq);
            queryLabel.Content = text;
            messages.Children.Add(queryLabel);

            // this line forces UI to update
            messages.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
            Task.Delay(10);
        }


        // start the right function
        public void Muxer(string LuisResult)
        {
            // parse JSON
            string Intent = LuisParser.getIntent(LuisResult);
            Dictionary<string, Entity> Entities = LuisParser.getEntities(LuisResult);

            if (Functions.ContainsKey(Intent))
            {
                Functions[Intent](messages, Entities);
            }
            else
            {
                PushMessage("Sorry, I was not able to understand that", ANSWER_STYLE);
            }

        }


        public void StartRegularRequest(string query)
        {
            // check if the query is empty
            if (query == "") return;

            // clean the command box
            commandBox.Text = "";
            PushMessage(query, REQUEST_STYLE);

            // make connection with LUIS
            try
            {
                var result = Task.Run(async () => { return await LuisParser.MakeRequest(query); }).Result;
                Muxer(result);
            }

            catch (Exception)
            {
                PushMessage("Sorry, I'm having a problem with the internet connection", ANSWER_STYLE);
            }
        }

        public void StartServerRequest(string query)
        {
            if (query == "") return;



            string KeyWord = query.IndexOf(" ") > -1
                  ? query.Substring(0, query.IndexOf(" "))
                  : query;


            string helperString;

            switch (KeyWord)
            {
                case "link":
                    helperString = query.IndexOf(" ") > -1
                      ? query.Split(' ')[1]
                      : null;
                    if (helperString != null)
                        Execute.openLink(helperString);
                    break;

                case "copy":
                    helperString = query.IndexOf(" ") > -1
                      ? query.Split(' ')[1]
                      : query;
                    if (helperString != null)
                        Execute.copyToClipboard(helperString);
                    break;

                default:
                    StartRegularRequest(query);
                    break;
            }
        }




        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (key.GetValue("Marina") != null)
            {
                if(key.GetValue("Marina").ToString() == "false")
                {
                    StartCheckBox.IsChecked = false;
                }
                else
                {
                    StartCheckBox.IsChecked = true;
                }
            }

            // check if user is connected    
            if (global._FirebaseAuth != null)
            {
                TBText = "Listening";

                // get user name. If user name is not specified use the email adress
                string NameToShow = (global._FirebaseAuth.User.DisplayName == "") ? global._FirebaseAuth.User.Email : global._FirebaseAuth.User.DisplayName;

                PushMessage("Hi, " + NameToShow, ANSWER_STYLE);
                PushMessage("I'm Marina, your personal assistant. You can ask me what I can help you with using the textbox below", ANSWER_STYLE);
                PushMessage("You can execute commands here using other devices by adding 'on my pc' at the end of your sentence", ANSWER_STYLE);
                StartListen();
            }
            else
            {
                PushMessage("You are not logged in, please log in using the button above to unlock my full potential", ANSWER_STYLE);
            }
        }



        /*
         * Handle Firebase client
         */

        // Listen to machine branch in firebase
        private void StartListen()
        {
            SignInButton.Visibility = Visibility.Hidden;
            SignOutButton.Visibility = Visibility.Visible;

            // create a firebase branch listener for new messages
            try
            {
                if (global._FirebaseClient != null)
                {
                    subscribtion = global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/PC")
                    .AsObservable<string>()
                    .Subscribe(data =>
                    {
                        try
                        {
                            HandleServerRequest(data, global._FirebaseClient, global._FirebaseAuth);
                        }
                        catch(Exception)
                        {
                            subscribtion.Dispose();
                            StartListen();
                        }
                    });
                }
                else
                {
                    PushMessage("Listening to other devices requires you to be logged in", ANSWER_STYLE);
                }
            }
            catch
            {
                PushMessage("I have to restart. Be right back ;)", ANSWER_STYLE);
                MarinaLauncher launcher = new MarinaLauncher();
                launcher.Show();
                this.Close();
            }
        }

        // stop listen
        private void StopListen()
        {
            if (subscribtion != null)
            {
                PushMessage("Stopped listening", ANSWER_STYLE);
                subscribtion.Dispose();
                subscribtion = null;
            }
            else
            {
                PushMessage("Listening", ANSWER_STYLE);
                StartListen();
            }
        }




        // handle new messages from firebase
        private async void HandleServerRequest(FirebaseEvent<string> data, FirebaseClient Firebase, FirebaseAuthLink auth)
        {

            if (data.EventType == FirebaseEventType.InsertOrUpdate && data.Object.ToLower() != "false") // new message arrives
            {
                await Dispatcher.BeginInvoke((Action)(() => StartServerRequest(data.Object)));

                await Firebase.Child("Users/" + auth.User.LocalId + "/PC/message").PutAsync("false");
            }
        }





        // show windows 10 notification



        /*
         * DESIGN ENGINES
         *  
         */
        // Close app button
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // handle text box hint - because wpf has no built in option for hint
        private void CommandBoxGotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox)
            {
                //If nothing has been entered yet.
                if (((TextBox)sender).Foreground == Brushes.Gray)
                {
                    ((TextBox)sender).Text = "";
                    ((TextBox)sender).Foreground = Brushes.Black;
                    BoxFocus = true;

                }
            }
        }

        private void CommandBoxLostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //Make sure sender is the correct Control.
            if (sender is TextBox)
            {
                //If nothing was entered, reset default text.
                if (((TextBox)sender).Text.Trim().Equals(""))
                {
                    ((TextBox)sender).Foreground = Brushes.Gray;
                    ((TextBox)sender).Text = "Type your request";
                    BoxFocus = false;
                }
            }
        }

        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void StopListenButton(object sender, RoutedEventArgs e)
        {
            StopListen();
        }

        // handle requests from the text box
        public void HandleEnterDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (BoxFocus)
                {
                    sendButton.IsEnabled = false;
                    string query = commandBox.Text;
                    StartRegularRequest(query);
                    sendButton.IsEnabled = true;
                    scrollMessages.ScrollToEnd();
                }
            }

        }

        public void StartRequestUsingTextBox(object sender, RoutedEventArgs e)
        {
            if (BoxFocus)
            {
                sendButton.IsEnabled = false;
                string query = commandBox.Text;
                StartRegularRequest(query);
                sendButton.IsEnabled = true;
                scrollMessages.ScrollToEnd();
            }
        }

        // Handle SignIn and SignOut

        // sign in and sign out
        private void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            Sign_In SignIWindow = new Sign_In();
            SignIWindow.Show();
            this.Close();
        }

        // this function signs out the user
        private void SignOutButtonClick(object sender, RoutedEventArgs e)
        {

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt"))) // check if user deatails file is exists
            {
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt")); // clear userDetails file
            }

            // clear firebase variables
            global._FirebaseClient = null;
            global._FirebaseAuth = null;

            StopListen();

            SignInButton.Visibility = Visibility.Visible;
            SignOutButton.Visibility = Visibility.Hidden;
            PushMessage("You are no longer connected", ANSWER_STYLE);
        }

        // make the app draggable
        private void dragging(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        // GUI functions
        public MainWindow()
        {
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth * 0.35;
            this.Height = System.Windows.SystemParameters.PrimaryScreenWidth * 0.4;



            TBText = "Not Listening";
            InitializeComponent();
        }

        private void ShareLinkButton(object sender, RoutedEventArgs e)
        {
            ShareLink shareLink = new ShareLink();
            shareLink.Show();
        }

        private void ShareClipboardButton(object sender, RoutedEventArgs e)
        {
            ShareClipboard shareClipboard = new ShareClipboard();
            shareClipboard.Show();
        }

        public void StartOnStartUp(object sender, RoutedEventArgs e)
        {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                key.SetValue("Marina", path);
                PushMessage("Gotcha, I will remember to start on startup from now on", ANSWER_STYLE);
        }

        private void DisStartOnStartUp(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            key.SetValue("Marina", "false");
            PushMessage("Okay, I hope you will remember to open me yourself", ANSWER_STYLE);
        }
    }
}
