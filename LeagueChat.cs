using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Net.Http;

using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;

namespace Overwatch
{
    public class LeagueChat
    {
        public Dictionary<string, string> SummonerNames;
        public Dictionary<Jid, SummonerStatus> SummonerStatuses;
        public List<string> RecordedNames = new List<string>();

        public string Username = "";
        public string ImagePath = "";

        private XmppClientConnection conn;

        private XmlSerializer serializer = new XmlSerializer(typeof(SummonerStatus));

        private RealmJSON regionVersions;


        private const string realmJsonUrl = "http://ddragon.leagueoflegends.com/realms/na.json";
        private const string xmppConnectServer = "chat.na2.lol.riotgames.com";

        public delegate void FriendsListReceivedHandler(object sender, Dictionary<Jid, SummonerStatus> summonerStatuses);
        public event FriendsListReceivedHandler OnFriendsListReceived;
        private void FriendsListReceived(Dictionary<Jid, SummonerStatus> summonerStatuses)
        {
            if (OnFriendsListReceived != null)
                OnFriendsListReceived(this, summonerStatuses);
        }

        public delegate void UserStatusChangedHandler(object sender, string summonerName, SummonerStatus currentStatus);
        public event UserStatusChangedHandler OnUserStatusChanged;
        private void UserStatusChanged(string summonerName, SummonerStatus status)
        {
            if (OnUserStatusChanged != null)
                OnUserStatusChanged(this, summonerName, status);
        }

        public delegate void UserFinishedGameHandler(object sender, string summonerName, SummonerStatus previousStatus);
        public event UserFinishedGameHandler OnUserGameFinished;
        private void GameFinished(string summonerName, SummonerStatus previousStatus)
        {
            if (OnUserGameFinished != null)
                OnUserGameFinished(this, summonerName, previousStatus);
        }

        public delegate void ChatErrorHandler(object sender, Exception ex);
        public event ChatErrorHandler OnChatError;
        private void ChatError(Exception ex)
        {
            if (OnChatError != null)
                OnChatError(this, ex);
        }

        public delegate void CurrentVersionReceivedHandler(object sender);
        public event CurrentVersionReceivedHandler OnCurrentVersionReceived;
        private void CurrentVersionReceived()
        {
            if (OnCurrentVersionReceived != null)
                OnCurrentVersionReceived(this);
        }

        public async void Initialize()
        {
            Reset();
            
            var serializer = new JavaScriptSerializer();
            var realmJson = await GetStringFromWeb(realmJsonUrl);
            var realmObj = (RealmJSON)serializer.Deserialize(realmJson, typeof(RealmJSON));
            regionVersions = realmObj;
            ImagePath = "http://ddragon.leagueoflegends.com/cdn/" + realmObj.v + "/img/";
            CurrentVersionReceived();
        }

        public void Reset()
        {
            SummonerNames = new Dictionary<string, string>();
            SummonerStatuses = new Dictionary<Jid, SummonerStatus>();
            Username = "";

            conn = new XmppClientConnection("pvp.net", 5223);
            conn.AutoPresence = false;
            conn.ConnectServer = xmppConnectServer;
            conn.Resource = "Overwatch";
            conn.Status = "<body><profileIcon>0</profileIcon><statusMsg>Overwatching</statusMsg></body>";
            conn.UseSSL = true;
            conn.Show = ShowType.xa;
            conn.OnXmppConnectionStateChanged += conn_OnXmppConnectionStateChanged;
            conn.OnLogin += conn_OnLogin;
            conn.OnAuthError += conn_OnAuthError;
            conn.OnSocketError += conn_OnSocketError;

            conn.OnRosterStart += conn_OnRosterStart;
            conn.OnRosterItem += conn_OnRosterItem;
            conn.OnRosterEnd += conn_OnRosterEnd;

            conn.OnPresence += conn_OnPresence;

            conn.OnMessage += conn_OnMessage;
        }
       
        public void LoadRecordedNames()
        {
            RecordedNames.Clear();

            var loggedUsers = Properties.Settings.Default.LoggedUsers;
            if (loggedUsers == null)
                return;

            foreach (var u in loggedUsers)
            {
                if (u.Split(':')[0] == Username.ToLower())
                {
                    var namesForUser = u.Split(':')[1].Split(',');
                    RecordedNames = new List<string>(namesForUser);
                    break;
                }
            }
        }

        public void SaveRecordedNames()
        {
            var lobbyControl = App.Window.lobbyControl;
            if (lobbyControl.FriendListItems.Count == 0 || Username == "")
                return;

            var namesArray = new List<String>();
            foreach (var i in lobbyControl.FriendListItems)
            {
                if(i.recordGamesBox.IsChecked == true)
                    namesArray.Add(i.SummonerName);
            }

            var loggedUsers = Properties.Settings.Default.LoggedUsers;

            //We have no users logged on any accounts.
            //Check to see if we need to add this account.
            if(loggedUsers == null)
            {
                if (namesArray.Count > 0)
                {
                    Properties.Settings.Default.LoggedUsers = new System.Collections.Specialized.StringCollection();
                    Properties.Settings.Default.LoggedUsers.Add(Username.ToLower() + ":" + string.Join(",", namesArray));
                    Properties.Settings.Default.Save();
                }
                return;
            }


            //We had previous users with saves names.
            //Check to see if one of them was this account and update if needed.
            for(int i = 0; i < loggedUsers.Count; i++)
            {
                if (loggedUsers[i].Split(':')[0] == Username.ToLower())
                {
                    if(namesArray.Count == 0)
                    {
                        if (loggedUsers.Count == 1)
                        {
                            Properties.Settings.Default.LoggedUsers = null;
                        }
                        else
                        {
                            loggedUsers.Remove(loggedUsers[i]);
                            Properties.Settings.Default.LoggedUsers = loggedUsers;
                        }
                    }
                    else
                    {
                        loggedUsers[i] = Username.ToLower() + ":" + string.Join(",", namesArray);
                        Properties.Settings.Default.LoggedUsers = loggedUsers;
                    }
                    Properties.Settings.Default.Save();
                    return;
                }
            }

            //If our account was not in the array already, add the logged names if needed.
            if (namesArray.Count > 0)
            {
                loggedUsers.Add(Username.ToLower() + ":" + string.Join(",", namesArray));
                Properties.Settings.Default.LoggedUsers = loggedUsers;
                Properties.Settings.Default.Save();
            }
        }

        public async Task OutputFinishedGame(string summonerName, SummonerStatus status)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(status.Timestamp).ToLocalTime();
            var currentDateTime = System.DateTime.Now;
            var timeInGame = currentDateTime.Subtract(dtDateTime);

            using (StreamWriter writer = File.AppendText("overwatch-logs\\" + Username.ToLower() + "\\" + currentDateTime.Year + "-" + currentDateTime.Month + "-" + currentDateTime.Day + "_Overwatch.log"))
                await writer.WriteLineAsync("[" + currentDateTime.ToLongTimeString() + "]Summoner Name: " + summonerName + " - Queue Type: " + status.CurrentQueueType + " - Champion: " + status.Champion + " - Game Length: " + timeInGame.TotalMinutes.ToString().Split('.')[0] + ":" + timeInGame.Seconds.ToString("00"));
        }

        public void Start(string username, string password)
        {
            Username = username;

            if (!Directory.Exists("overwatch-logs"))
                Directory.CreateDirectory("overwatch-logs");

            if (!Directory.Exists("overwatch-logs\\" + Username.ToLower()))
                Directory.CreateDirectory("overwatch-logs\\" + Username.ToLower());

            LoadRecordedNames();
            conn.Open(username, "AIR_" + password);
        }

        void conn_OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            if (state == XmppConnectionState.Disconnected)
                OnChatError(this, new Exception("You have been disconnected.(Logged in from another location)"));

            Console.WriteLine(state.ToString());
        }

        void conn_OnSocketError(object sender, Exception ex)
        {
            ChatError(new Exception("You have been disconnected.(Connection to server lost)"));
            Console.WriteLine("You have been disconnected.(Connection to server lost)");
        }

        void conn_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            ChatError(new Exception("Authentication Error(Wrong username or password)"));
            Console.WriteLine("Authentication Error(Wrong username or password)");
        }

        void conn_OnLogin(object sender)
        {
            Console.WriteLine("LoggedIn");
        }


        void conn_OnRosterStart(object sender)
        {
            Console.WriteLine("ReceivingRoster");
        }

        void conn_OnRosterItem(object sender, RosterItem item)
        {
            SummonerNames.Add(item.Jid.User, item.Name);
            Console.WriteLine(item.ToString().Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'"));
        }

        void conn_OnRosterEnd(object sender)
        {
            Console.WriteLine("RosterReceived.");
            App.DebugWindow.ScrollToBottom();
            //Console.WriteLine("Getting current user statuses.");
            FriendsListReceived(new Dictionary<Jid, SummonerStatus>(SummonerStatuses));
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                App.Window.Cursor = Cursors.Arrow;
            }));
            conn.SendMyPresence();
        }

        void conn_OnMessage(object sender, Message msg)
        {
            if (msg.Type == MessageType.groupchat)
            {
                Console.WriteLine(msg.ToString().Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'"));
            }
        }

        void conn_OnPresence(object sender, Presence pres)
        {
            Console.WriteLine(pres.ToString().Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'"));

            //We don't care what our account is doing on any connection, including this one.
            if (pres.From.User == conn.MyJID.User)
            {

                //We want our status to be overwatching unless we are .dnd(In game, In queue, Champ select)
                //if (pres.Show != ShowType.dnd && conn.MyJID.Resource != pres.From.Resource)
                //    conn.SendMyPresence();
                return;
            }

            //WE don't care about any other servers except pvp.net. Other servers are conference.pvp.net sec.pvp.net
            //These are used for MUCs from what I can tell.
            if (pres.From.Server.Contains("pvp.net") && pres.From.Server != "pvp.net")
                return;

            //The user logged off. Let's remove him from the presence list.
            if (pres.Type == PresenceType.unavailable)
            {
                if(SummonerStatuses.ContainsKey(pres.From))
                    SummonerStatuses.Remove(pres.From);

                if (SummonerStatuses.Where(x => x.Key.User == pres.From.User).Count() == 0)
                    UserStatusChanged(SummonerNames[pres.From.User], null);
                else
                    UserStatusChanged(SummonerNames[pres.From.User], SummonerStatuses.Where(x => x.Key.User == pres.From.User).First().Value);

                return;
            }

            //If we already have a value for this user, store it temporarily so we can get the previous status.
            SummonerStatus previousStatus = null;
            if (SummonerStatuses.ContainsKey(pres.From))
                previousStatus = SummonerStatuses[pres.From];

            //Deserialize and update our status in the dictionary.
            SummonerStatus currentStatus = null;

            if (pres.Status == null)
                pres.Status = "";

            using (TextReader reader = new StringReader(pres.Status))
            {
                try
                {
                    currentStatus = (SummonerStatus)serializer.Deserialize(reader);
                }
                catch
                {
                    currentStatus = new SummonerStatus() { StatusMsg = pres.Status };
                }
            }
                

            currentStatus.Show = pres.Show;

            SummonerStatuses.Remove(pres.From);
            SummonerStatuses.Add(pres.From, currentStatus);

            if (SummonerNames.ContainsKey(pres.From.User) && currentStatus != null)
                UserStatusChanged(SummonerNames[pres.From.User], currentStatus);

            //If we managed to get here AND the previous status is not null, this means the status has changed.
            if (previousStatus != null && currentStatus != null)
            {
                if (previousStatus.GameStatus == "inGame" && currentStatus.GameStatus != "inGame")
                {
                    if(SummonerNames.ContainsKey(pres.From.User))
                        GameFinished(SummonerNames[pres.From.User], previousStatus);
                }
            }
        }


        private async Task<string> GetStringFromWeb(string url)
        {
            using (var client = new HttpClient())
            {
                var s = await client.GetStringAsync(new System.Uri(url));
                return s;
            }
        }
    }




    public class RealmJSON
    {
        public Subversion n { get; set; }
        public string v { get; set; }
        public string l { get; set; }
        public string cdn { get; set; }
    }

    public class Subversion
    {
        public string item { get; set; }
        public string rune { get; set; }
        public string mastery { get; set; }
        public string summoner { get; set; }
        public string champion { get; set; }
        public string profileicon { get; set; }
        public string language { get; set; }
    }
}
