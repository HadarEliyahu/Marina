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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Marina.Classes;

namespace Marina
{
    /// <summary>
    /// Interaction logic for ShareLink.xaml
    /// </summary>
    public partial class ShareLink : Window
    {
        public ShareLink()
        {
            InitializeComponent();
        }

        // make the app draggable
        private void dragging(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Share(object sender, RoutedEventArgs e)
        {
            if(LinkBox.Text != "")
            {
                if (Uri.IsWellFormedUriString(LinkBox.Text, UriKind.Absolute)) // check if request is a URL
                {
                    if(Execute.ShareLink(LinkBox.Text))
                    {
                        MessageBox.Show("Sent. check out your phone");
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Not a valid URL");
                }
            }
            else
            {
                MessageBox.Show("Empty textbox");
            }
        }
    }
}
