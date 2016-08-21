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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Overwatch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public LoginControl loginControl;
        public LobbyControl lobbyControl;
        public bool IsClosing = false;
        public MainWindow()
        {
            InitializeComponent();
            loginControl = new LoginControl();
            controlHost.Children.Add(loginControl);

            lobbyControl = new LobbyControl();
            lobbyControl.Visibility = System.Windows.Visibility.Hidden;
            controlHost.Children.Add(lobbyControl);

            App.Chat.OnChatError += Chat_OnChatError;
            App.Chat.OnFriendsListReceived += Chat_OnFriendsListReceived;
            this.KeyUp += MainWindow_KeyUp;
            this.Closing += MainWindow_Closing;
            this.Activated += MainWindow_Activated;
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            App.DebugWindow.Topmost = true;
            App.DebugWindow.Topmost = false;
            this.Focus();
            this.Topmost = true;
            this.Topmost = false;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Chat.SaveRecordedNames();
            this.IsClosing = true;
            App.DebugWindow.Close();
        }

        void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F11)
                return;
            if (App.DebugWindow.IsVisible)
                App.DebugWindow.Hide();
            else
            {
                App.DebugWindow.Show();
                App.DebugWindow.ScrollToBottom();
                App.Window.Focus();
            }

        }

        void Chat_OnFriendsListReceived(object sender, Dictionary<agsXMPP.Jid, SummonerStatus> currentStatuses)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                //Set our title to include the account name we are on and show the lobby control.
                this.Title = "Overwatch - " + App.Chat.Username;
                if(App.DebugWindow != null)
                    App.DebugWindow.Title = "Overwatch Debug Console - " + App.Chat.Username;
                loginControl.Visibility = System.Windows.Visibility.Hidden;
                lobbyControl.Visibility = System.Windows.Visibility.Visible;
            }));
        }

        void Chat_OnChatError(object sender, Exception ex)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                if (!ex.Message.Contains("Auth"))
                {
                    //Save our logged names and reset our controls to the log in screen.
                    App.Chat.SaveRecordedNames();
                    this.Title = "Overwatch";
                    if (App.DebugWindow != null)
                        App.DebugWindow.Title = "Overwatch Debug Console";
                    lobbyControl.Reset();
                    lobbyControl.Visibility = System.Windows.Visibility.Hidden;
                    loginControl.Visibility = System.Windows.Visibility.Visible;
                }

                //Reset our login control and App.Chat to their default state.
                loginControl.IsEnabled = true;
                loginControl.passwordBox.Focus();
                App.Chat.Reset();
                App.Window.Cursor = Cursors.Arrow;
            }));
        }
    }
}
