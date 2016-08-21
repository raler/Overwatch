using agsXMPP.protocol.client;
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

namespace Overwatch
{
    /// <summary>
    /// Interaction logic for FriendsListItem.xaml
    /// </summary>
    public partial class FriendsListItem : UserControl
    {
        public string SummonerName;
        public ShowOrder Show = ShowOrder.Offline;

        public FriendsListItem(string summonerName)
        {
            InitializeComponent();
            SummonerName = summonerName;
            summonerNameLabel.Content = SummonerName;
        }

        public void UpdateStatus(SummonerStatus status)
        {
            if (status == null)
            {
                summonerStatusLabel.Content = "Offline";
                summonerStatusLabel.Foreground = Brushes.White;
                iconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Overwatch;component/Resources/overwatch.ico", UriKind.Absolute));
                Show = ShowOrder.Offline;
                return;
            }

            if(status.ProfileIcon != null && status.ProfileIcon != "")
                iconImage.Source = new BitmapImage(new Uri(App.Chat.ImagePath + "profileicon/" + status.ProfileIcon + ".png"));
            else
                iconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Overwatch;component/Resources/questionmark.png", UriKind.Absolute));

            switch (status.Show)
            {
                case ShowType.chat:
                    summonerStatusLabel.Foreground = Brushes.Green;
                    Show = ShowOrder.OutOfGame;

                    summonerStatusLabel.Content = GameStatusToReadable(status.GameStatus);
                    if(status.GameStatus != null && status.GameStatus == "outOfGame")
                    {
                        if(status.StatusMsg == null || status.StatusMsg == "")
                            summonerStatusLabel.Content = "Online";
                        else
                            summonerStatusLabel.Content = status.StatusMsg;
                    }       
                    break;
                case ShowType.dnd:
                    summonerStatusLabel.Foreground = Brushes.Yellow;
                    Show = ShowOrder.DnD;

                    var stat = GameStatusToReadable(status.GameStatus);
                    string displayStatus = stat;
                    if (displayStatus == "" && status.StatusMsg != null)
                            displayStatus = status.StatusMsg;

                    if (status.GameStatus != null && status.GameStatus == "inGame")
                    {
                        if (status.Champion != null && status.Champion != "")
                            iconImage.Source = new BitmapImage(new Uri(App.Chat.ImagePath + "champion/" + status.Champion + ".png"));
                        else
                            iconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Overwatch;component/Resources/questionmark.png", UriKind.Absolute));

                        //No need to modify our status any further if there is no queue type to show.
                        if (status.CurrentQueueType == null || status.CurrentQueueType == "")
                            break;

                        if (status.CurrentQueueType == "NONE")
                            displayStatus += "(CUSTOM)";
                        else
                            displayStatus += "(" + status.CurrentQueueType + ")";
                    }
                    summonerStatusLabel.Content = displayStatus;
                    break;
                case ShowType.away:
                    summonerStatusLabel.Foreground = Brushes.Red;
                    Show = ShowOrder.Away;

                    if (status.StatusMsg == null || status.StatusMsg == "")
                        summonerStatusLabel.Content = "Away";
                    else
                        summonerStatusLabel.Content = status.StatusMsg;
                    break;
                case ShowType.xa:
                    if (status.StatusMsg != null && status.StatusMsg == "Overwatching")
                    {
                        summonerStatusLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f99e1a"));
                        Show = ShowOrder.Overwatching;

                        summonerStatusLabel.Content = status.StatusMsg;
                        iconImage.Source = new BitmapImage(new Uri("pack://application:,,,/Overwatch;component/Resources/overwatch.ico", UriKind.Absolute));
                    }
                    else
                        goto case ShowType.away;
                    break;
                default:
                    summonerStatusLabel.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#f99e1a"));
                    Show = ShowOrder.Unknown;
                    summonerStatusLabel.Content = "Unknown";
                    break;
            }
        }

        public static string GameStatusToReadable(string input)
        {
            if(input == null)
                return "";
            else
            {
                string gameStatus = input;
                StringBuilder builder = new StringBuilder();
                foreach (char c in gameStatus)
                {
                    if (Char.IsUpper(c) && builder.Length > 0) builder.Append(' ');
                    builder.Append(c);
                }
                gameStatus = builder.ToString();

                if (gameStatus.Length > 1)
                    return gameStatus.First().ToString().ToUpper() + gameStatus.Substring(1);
                else
                    return gameStatus.First().ToString().ToUpper();
            }
        }

        private void recordGamesBox_Checked(object sender, RoutedEventArgs e)
        {
            App.Chat.SaveRecordedNames();
            if (!App.Chat.RecordedNames.Contains(SummonerName))
                App.Chat.RecordedNames.Add(SummonerName);
        }

        private void recordGamesBox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.Chat.SaveRecordedNames();
            if (App.Chat.RecordedNames.Contains(SummonerName))
                App.Chat.RecordedNames.Remove(SummonerName);
        }
    }


    public enum ShowOrder
    {
        Overwatching = 0,
        OutOfGame = 1,
        DnD = 2,
        Away = 3,
        Unknown = 4,
        Offline = 5
    }
}
