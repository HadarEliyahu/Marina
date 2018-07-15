using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using Firebase.Auth;
using System.IO;


namespace Marina
{
    /// <summary>
    /// Interaction logic for SignUp.xaml
    /// </summary>
    public partial class SignUp : Window
    {
        public const string FirebaseAppUri = "https://marina-a2458.firebaseio.com";
        public const string FirebaseAppKey = "AIzaSyD7wew7GJT9ewLUtXwMeagt7PIW-FamFEo";
        public bool run = false;

        App global = Application.Current as App;

        public struct MessageToFirebase
        {
            public string message;
        };

        public SignUp()
        {
            this.Width = System.Windows.SystemParameters.PrimaryScreenHeight * 0.50;
            this.Height = System.Windows.SystemParameters.PrimaryScreenWidth * 0.35;
            InitializeComponent();
        }


        private void SignUpButtonClick(object sender, RoutedEventArgs e)
        {

            string Username = UsernameBox.Text.ToLower();
            string Password = PasswordBox.Password;
            string DisplayName = NameBox.Text;

            Task t;
            bool connected = false;

            if (Username == "" || Password == "")
            {
                MessageBox.Show("Email or password are empty");
                return;
            }

            if (DisplayName == "")
            {
                MessageBox.Show("Username can't be blank");
                return;
            }

            



            t = Task.Run(async () =>
            {
                try
                {
                    await ConnectToFirebase(Username, Password, DisplayName);
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
                global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "false"});
                global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/PC").PutAsync(new MessageToFirebase { message = "false" });

                StreamWriter sr = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt"));
                sr.WriteLine(Username);
                sr.WriteLine(Password);
                sr.Close();


                MarinaLauncher w = new MarinaLauncher();
                w.Show();
                this.Close();    
                
                
            }
            else
            {
                MessageBox.Show("Failed to sign up");
            }
        }



        private async Task ConnectToFirebase(string username, string password, string displayName)
        {
            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseAppKey));
            global._FirebaseAuth = await authProvider.CreateUserWithEmailAndPasswordAsync(username, password, displayName);

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

        private void BackWindow(object sender, RoutedEventArgs e)
        {
            Sign_In w = new Sign_In();
            w.Show();
            this.Close();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
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
    }
}
