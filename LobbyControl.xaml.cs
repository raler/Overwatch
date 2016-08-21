using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    /// Interaction logic for LobbyControl.xaml
    /// </summary>
    public partial class LobbyControl : UserControl
    {
        public  ObservableCollection<FriendsListItem> FriendListItems;

        public LobbyControl()
        {
            InitializeComponent();
            FriendListItems = new ObservableCollection<FriendsListItem>();
            userList.ItemsSource = FriendListItems;

            App.Chat.OnFriendsListReceived += Chat_OnFriendsListReceived;
            App.Chat.OnUserGameFinished += Chat_OnUserGameFinished;
        }

        void Chat_OnFriendsListReceived(object sender, Dictionary<agsXMPP.Jid, SummonerStatus> currentStatuses)
        {
            App.Chat.OnUserStatusChanged += Chat_OnUserStatusChanged;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                foreach (var i in App.Chat.SummonerNames.Keys)
                {
                    FriendsListItem user = new FriendsListItem(App.Chat.SummonerNames[i]);
                    FriendListItems.Add(user);

                    if (App.Chat.RecordedNames.Contains(App.Chat.SummonerNames[i]))
                        user.recordGamesBox.IsChecked = true;

                    if (currentStatuses.Where(x => x.Key.User == i).Count() > 0)
                    {
                        var status = currentStatuses.Reverse().Where(x => x.Key.User == i).First();
                        user.UpdateStatus(status.Value);
                    }
                }
                SortFriendsList();
            }));
        }

        void Chat_OnUserStatusChanged(object sender, string summonerName, SummonerStatus currentStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                foreach (var i in FriendListItems)
                    if (i.SummonerName == summonerName)
                    {
                        i.UpdateStatus(currentStatus);
                        SortFriendsList();
                        break;
                    }
            }));
        }

        void SortFriendsList()
        {
            FriendListItems = new ObservableCollection<FriendsListItem>(FriendListItems.OrderBy((x) => x.Show).ThenBy((x) => x.SummonerName));
            userList.ItemsSource = FriendListItems;
            FilterFriendsList();
        }

        void FilterFriendsList()
        {
            foreach (var i in FriendListItems)
            {
                if (!i.SummonerName.ToLower().Contains(filterBox.Text.ToLower()))
                    i.Visibility = System.Windows.Visibility.Collapsed;
                else
                    i.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void Reset()
        {
            FriendListItems = new ObservableCollection<FriendsListItem>();
            userList.ItemsSource = FriendListItems;
        }

        void Chat_OnUserGameFinished(object sender, string summonerName, SummonerStatus previousStatus)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(async () =>
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(previousStatus.Timestamp).ToLocalTime();
                var currentDateTime = System.DateTime.Now;
                var timeInGame = currentDateTime.Subtract(dtDateTime);

                if (logAllUsersBox.IsChecked == true || App.Chat.RecordedNames.Contains(summonerName))
                {
                    finishedGamesLog.Items.Add(new Label() { Foreground = Brushes.White, Content = "[" + currentDateTime.ToLongTimeString() + "]" + summonerName + " has finished a game." + "  Queue Type: " + previousStatus.CurrentQueueType + "  Champion: " + previousStatus.Champion + "  Game Length: " + timeInGame.TotalMinutes.ToString().Split('.')[0] + ":" + timeInGame.Seconds.ToString("00") });

                    await App.Chat.OutputFinishedGame(summonerName, previousStatus);
                }
            }));
        }

        private void filterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterFriendsList();
        }
    }
}
