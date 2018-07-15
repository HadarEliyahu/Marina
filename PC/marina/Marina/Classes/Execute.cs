using System;
using Marina.Classes;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;
using System.Net;
using System.Net.Http;
using System.Web;



namespace Marina.Classes
{

    public class Execute
    {

        public static App global = Application.Current as App;

        public struct MessageToFirebase
        {
            public string message;
        };

        public static Dictionary<string, string> SearchEngines = new Dictionary<string, string>()
        {
            {"google", @"https://www.google.co.il/search?q="},
            {"youtube", @"https://www.youtube.com/results?search_query="},
            {"amazon", @"https://www.amazon.com/s/field-keywords="},
            {"duckduckgo", @"https://www.duckduckgo.com/?q="}
        };

        public const string ANSWER_STYLE = "Answer";
        public const string REQUEST_STYLE = "Request";


        private delegate void NoArgDelegate();

        // This function pushes messages to the chat screen
        public static void PushAnswer(StackPanel MessagesPanel, string text)
        {
            Label queryLabel = new Label();
            queryLabel.Style = (Style)Application.Current.FindResource(ANSWER_STYLE);
            queryLabel.Content = text;
            MessagesPanel.Children.Add(queryLabel);

            MessagesPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
            Task.Delay(200);

        }

        public static void PushRequest(StackPanel MessagesPanel, string text)
        {
            Label queryLabel = new Label();
            queryLabel.Style = (Style)Application.Current.FindResource(REQUEST_STYLE);
            queryLabel.Content = text;
            MessagesPanel.Children.Add(queryLabel);
            MessagesPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
            Task.Delay(200);
        }





        public static void PushButton(StackPanel MessagesPanel, string content, Action method)
        {
            Button btn = new Button();
            btn.Style = (Style)Application.Current.FindResource("AssureButton");
            btn.Content = content;
            btn.Click += (sr, e) => { method.DynamicInvoke(); };
            MessagesPanel.Children.Add(btn);
            MessagesPanel.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
            Task.Delay(200);
        }


        public static void clearScreen(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            MessagesPanel.Children.Clear();

            return;
        }



        //This function show the features list 
        public static void FeaturesList(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            PushAnswer(MessagesPanel, "There are many things I can do. You can ask me directly or use the buttons below");
            PushAnswer(MessagesPanel, "To do things on your phone, just add 'on my phone' at the end of your sentence");

            Dictionary<string, Entity> d = new Dictionary<string, Entity>();
            d.Add("SongName", new Entity("rolling in the deep", "SongName"));
            PushButton(MessagesPanel, "play a song", () => { PushRequest(MessagesPanel, "play rolling in the deep"); PlayMusic(MessagesPanel, d); });


            Dictionary<string, Entity> d1 = new Dictionary<string, Entity>();
            d1.Add("SearchQuery", new Entity("rolling in the deep", "SearchQuery"));
            PushButton(MessagesPanel, "search on the internet", () => { PushRequest(MessagesPanel, "search for rolling in the deep"); Search(MessagesPanel, d1); });

            Dictionary<string, Entity> d2 = new Dictionary<string, Entity>();
            d2.Add("AppName", new Entity("excel", "AppName"));
            PushButton(MessagesPanel, "open an app", () => { PushRequest(MessagesPanel, "open excel"); OpenApp(MessagesPanel, d2); });
        }


        public static void CloseApp(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            if (entities.ContainsKey("AppName"))
            {

                string AppName = entities["AppName"].getValue();

                bool found = false;
                Process[] allProcesses = Process.GetProcesses();
                foreach (Process workingProcess in allProcesses)
                {
                    if (workingProcess.MainWindowTitle.Length > 0)
                    {
                        if (workingProcess.ProcessName.ToLower().Contains(AppName) && !workingProcess.ProcessName.Contains("windows"))
                        {
                            PushAnswer(MessagesPanel, "Okay, closing " + workingProcess.ProcessName);
                            PushAnswer(MessagesPanel, "Pssst... you can close them using the [X] button");
                            workingProcess.Kill();
                            found = true;
                            break;
                        }

                    }
                }
                if (!found)
                {
                    PushAnswer(MessagesPanel, AppName + " is not running");
                }
            }
            else
            {
                PushAnswer(MessagesPanel, "Sorry... I can't find any app name in your last request");
            }

            return;
        }

        //Turns off the app and returns true if the shutdown process was successful (and false otherwise)
        public static void ShutDown(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            if (entities.ContainsKey("FromPhone"))
            {
                PushAnswer(MessagesPanel, "Self destructing in 5 seconds");
                RunCMD("shutdown -f");
            }
            else
            {
                PushAnswer(MessagesPanel, "Are you sure?");
                PushButton(MessagesPanel, "yes", () => RunCMD("shutdown -f"));
                Task.Delay(200);

            }
            return;
        }

        // This function searches the internet on the requested search engine 
        public static void Search(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            string query;
            string RequestedSearchEngine = SearchEngines["google"];
            string RequestedSearchEngineName = "google";

            if (entities.ContainsKey("SearchQuery")) // check if LUIS recognized the search query
            {

                query = entities["SearchQuery"].getValue();

                // check if LUIS recognized a search engine that exists in our list 
                if (entities.ContainsKey("SearchEngine") && SearchEngines.ContainsKey(entities["SearchEngine"].getValue()))
                {
                    RequestedSearchEngine = SearchEngines[entities["SearchEngine"].getValue()];
                    RequestedSearchEngineName = entities["SearchEngine"].getValue();
                }


                if (entities.ContainsKey("OnPhone")) // handle search on phone
                {
                    if (global._FirebaseAuth != null)
                    {
                        PushAnswer(MessagesPanel, "The command was sent to your phone. If you are logged in on your phone client, the command should be executed shortly.");
                        global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "search for " + query + " on " + RequestedSearchEngineName });
                    }
                    else
                    {
                        PushAnswer(MessagesPanel, "This feature is only available when you are logged in. you can log in using the button above");
                    }
                    return;
                }
                else
                {
                    PushAnswer(MessagesPanel, "Okay, here is you search for \"" + query + "\" on " + RequestedSearchEngineName);
                    Task.Delay(2000);

                    // open default browser with the search
                    System.Diagnostics.Process.Start(RequestedSearchEngine + query);
                    return;
                }
            }
            else
            {
                PushAnswer(MessagesPanel, "Sorry, I can't understand what should I search");
                return;
            }

        }

        //This function prints the time
        public static void ShowTime(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            PushAnswer(MessagesPanel, "The time now is " + DateTime.Now.ToString("hh:mm tt"));
            return;
        }

        public static void Call(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            if (global._FirebaseAuth != null)
            {
                if (entities.ContainsKey("ContactName"))
                {
                    string ContactName = entities["ContactName"].getValue();

                    PushAnswer(MessagesPanel, "Sent. Phone call should be intiated shortly if this contact exists in you cantacts list.");

                    // push a phone call command to firebase
                    global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "call " + ContactName });
                }

            }
            else
            {
                PushAnswer(MessagesPanel, "This feature is only available when you are logged in. you can log in using the button above");
                return;
            }
            return;
        }

        public static string FindYoutube(string query)
        {
            WebClient w = new WebClient();
            string s = w.DownloadString("https://www.youtube.com/results?search_query=" + query);

            foreach (LinkItem i in LinkFinder.Find(s))
            {
                if (i.Href.StartsWith("/watch"))
                {
                    return ("https://www.youtube.com/" + i.Href);
                }
            }
            return null;
        }


        public static void CreateShortCut(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            try
            {
                string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

                using (StreamWriter writer = new StreamWriter(deskDir + "\\" + "Marina" + ".url"))
                {
                    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    string icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception)
            {
                PushAnswer(MessagesPanel, "Couldn't create the shortcut");
            }
            finally
            {
                PushAnswer(MessagesPanel, "Shortcut created seccessfully");
            }
            return;
        }

        public static bool ShareLink(string link)
        {
            if (global._FirebaseAuth != null)
            {
                global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "link " + link });
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool ShareClipboard(string link)
        {
            if (global._FirebaseAuth != null)
            {
                global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "copy " + link });
            }
            else
            {
                return false;
            }
            return true;
        }

        public static void openLink(string link)
        {
            if (Uri.IsWellFormedUriString(link, UriKind.Absolute)) // check if request is a URL
            {
                Uri address = new Uri(link);
                global.ShowToastNotification("Opening link from phone", address.Host);
                System.Diagnostics.Process.Start(address.AbsoluteUri);
            }
        }

        public static void copyToClipboard(string text)
        {
            Clipboard.SetText(text);
            global.ShowToastNotification("A new text was copied to your clipboard", text);
        }

        /*
        // open programs and play music
        */
        public static void OpenApp(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            if (entities.ContainsKey("AppName"))
            {
                string AppName = entities["AppName"].getValue();

                if (entities.ContainsKey("OnPhone"))
                {
                    if (global._FirebaseAuth != null)
                    {
                        PushAnswer(MessagesPanel, "Sent. app should be opened shortly if it exists on your phone");
                        global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = "open " + AppName });
                    }
                    else
                    {
                        PushAnswer(MessagesPanel, "This feature is only available when you are logged in. you can log in using the button above");
                    }
                    return;
                }

                if (AppName == "marina")
                {
                    PushAnswer(MessagesPanel, "You like recursion, eh?");
                    return;
                }

                List<string> files = new List<string>();



                // search for the program
                files = FindProgram(AppName);

                if (files.Count == 0)
                {

                    PushAnswer(MessagesPanel, "I couldn't find the program you are looking for");
                    return;
                }
                else
                {
                    PushAnswer(MessagesPanel, "Opening " + Path.GetFileNameWithoutExtension(files[0]));

                    Task.Delay(500).ContinueWith(t => RunProgram(files[0]));

                    return;
                }
            }
            PushAnswer(MessagesPanel, "Could not find the program you wanted me to run");


            return;

        }

        //opns a new SignIn window
        public static void SignIn(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            PushAnswer(MessagesPanel, "Okay");

            Sign_In n = new Sign_In();
            n.Show();
            Application.Current.MainWindow.Close();

            return;
        }
/*
         
    */
        public static void PlayMusic(StackPanel MessagesPanel, Dictionary<string, Entity> entities)
        {
            string SearchEngine = "";
            if (!entities.ContainsKey("SongName"))
            {
                PushAnswer(MessagesPanel, "Sorry, I can't understand the name of the song you are looking for");
                return;
            }

            string SongName = entities["SongName"].getValue();
            if (entities.ContainsKey("SearchEngine"))
            {
                SearchEngine = entities["SearchEngine"].getValue();
            }


            if (entities.ContainsKey("OnPhone"))
            {
                if (global._FirebaseAuth != null)
                {
                    PushAnswer(MessagesPanel, "Sent. Song should be played shortly if it exists on your phone");
                    string command = SearchEngine == "" ? "play " + SongName : "link " + FindYoutube(SongName);
                    global._FirebaseClient.Child("Users/" + global._FirebaseAuth.User.LocalId + "/Phone").PutAsync(new MessageToFirebase { message = command });
                }
                else
                {
                    PushAnswer(MessagesPanel, "This feature is only available when you are logged in. you can log in using the button above");
                }
                return;
            }
            else
            {
                if(SearchEngine != "")
                {
                    if (SearchEngine == "youtube")
                    {
                        PushAnswer(MessagesPanel, "Okay, playing " + SongName + " on youtube");
                        System.Diagnostics.Process.Start(FindYoutube(SongName));
                        return;
                    }
                    else
                    {
                        PushAnswer(MessagesPanel, "I don't know this site");
                    }
                }
                else
                {
                    if (SongName == "music" || SongName == "song" || SongName == "a song")
                    {
                        SongName = "";
                    }

                    List<string> songs = FindMusic(SongName);

                    if (songs.Count == 0)
                    {
                        PushAnswer(MessagesPanel, "Sorry, the song could not be found");
                        PushButton(MessagesPanel, "play \"" + SongName + "\" on YouTube", () => { System.Diagnostics.Process.Start(FindYoutube(SongName)); });
                    }
                    else
                    {
                        PushAnswer(MessagesPanel, "Playing " + Path.GetFileNameWithoutExtension(songs[0]));
                        Task.Delay(1000).ContinueWith(t => RunProgram(songs[0]));
                        return;
                    }
                }  
            }   
        }

        // Open Files and Find Music helper functions
        public static List<string> FindMusic(string search)
        {
            string sDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            List<string> files = new List<string>(Directory.GetFiles(sDir, "*" + search + "*.mp3", SearchOption.AllDirectories));
            return files;
        }

        // this function finds program in windows special folder for programs
        public static List<string> FindProgram(string search)
        {
            string sDir = @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs";
            string sDir2 = @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs";
            List<string> files = new List<string>(Directory.GetFiles(sDir, "*" + search + "*.*", SearchOption.AllDirectories));
            files.AddRange(new List<string>(Directory.GetFiles(sDir2, "*" + search + "*.*", SearchOption.AllDirectories)));
            return files;
        }

        // this function start a program by its path
        public static void RunProgram(string path)
        {
            StreamWriter w = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "file.bat"));
            w.WriteLine("start \"\" \"" + path + "\"");
            w.Close();
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "file.bat");
            p.Start();
            p.WaitForExit();
        }

        //this function executes a command on cmd
        public static void RunCMD(string command)
        {
            StreamWriter w = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "file.bat"));
            w.WriteLine(command);
            w.Close();
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "file.bat");
            p.Start();
            p.WaitForExit();
        }

    }
}
