using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Firebase.Auth;
using System.IO;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace Marina
{
    /// <summary>
    /// Interaction logic for Sign_In.xaml
    /// </summary>
    public partial class Sign_In : Window
    {
        public const string FirebaseAppUri = "https://marina-a2458.firebaseio.com";
        public const string FirebaseAppKey = "AIzaSyD7wew7GJT9ewLUtXwMeagt7PIW-FamFEo";
        public bool run = false;
        App global = Application.Current as App;


        public Sign_In()
        {
            this.Width = System.Windows.SystemParameters.PrimaryScreenHeight * 0.50;
            this.Height = System.Windows.SystemParameters.PrimaryScreenWidth * 0.35;
            
            
            
            InitializeComponent();
        }

        private void CheckIfFileExists(object sender, RoutedEventArgs e)
        {
            if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt")))
            {
                System.IO.StreamReader file =
                new System.IO.StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt"));
                
                string username = "";
                string password = "";
                username = file.ReadLine();
                password = file.ReadLine();
                
                file.Close();
                if(username != null && password != null)
                {
                    trySignIn(username, password, true);
                }
                
            }  
        }

        private void trySignIn(string Username, string Password, bool FromFile)
        {
            
            Task t;
            bool connected = false;

            if (Username != "" && Password != "")
            {

                t = Task.Run(async () =>
                {
                    try
                    {
                        await ConnectToFirebase(Username, Password);
                        connected = true;
                    }
                    catch
                    {
                        connected = false;
                    }
                });
                t.Wait(2000);

                if (connected)
                {

                    StreamWriter sr = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt"));
                    sr.WriteLine(Username);
                    sr.WriteLine(Password);
                    sr.Close();

                    MainWindow w = new MainWindow();
                    w.Show();
                    this.Close();

                }
                else
                {
                    if(FromFile)
                    {
                        File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt"));
                        MainWindow w = new MainWindow();
                        w.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Username or password are incorrect");
                    }
                }
            }
        }

        private void SignIn(object sender, RoutedEventArgs e){
            string Username = UsernameBox.Text.ToLower();
            string Password = PasswordBox.Password;
            
            if (Username == "" || Password == "")
            {
                MessageBox.Show("Email or password are empty");
                return;
            }
            
            trySignIn(Username, Password, false);
        }

       

        private async Task ConnectToFirebase(string username, string password)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseAppKey));
            global._FirebaseAuth = await authProvider.SignInWithEmailAndPasswordAsync(username, password);

            global._FirebaseClient = new FirebaseClient(FirebaseAppUri,
            new FirebaseOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(global._FirebaseAuth.FirebaseToken)
            });
        }


        private void Minimize(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BackWindow(object sender, RoutedEventArgs e)
        {
            MainWindow w = new MainWindow();
            w.Show();
            this.Close();
        }

        // make the app draggable
        private void dragging(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void SignUp(object sender, MouseButtonEventArgs e)
        {
            SignUp w = new SignUp();
            w.Show();
            this.Close();
        }

        
    }
}
