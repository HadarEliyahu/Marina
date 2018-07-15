using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;


using Marina.Classes;
using System.Windows.Controls;

namespace Marina
{
    /// <summary>
    /// Interaction logic for MarinaLauncher.xaml
    /// </summary>
    public partial class MarinaLauncher : Window
    {
        public MarinaLauncher()
        {
            InitializeComponent();
        }

        private void StartSignIn(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            timer.Start();
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                Start();
            };
        }

        public void Start()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "userDetails.txt")))
            {

                Sign_In SignIWindow = new Sign_In() // make the sign in window invisible
                {
                    Width = 0,
                    Height = 0,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false
                };
                SignIWindow.Closed += SignInSucced;
                SignIWindow.Show();
            }
            else
            {
                MainWindow w = new MainWindow();
                w.Show();
                this.Close();
            }
        }

        public void SignInSucced(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
