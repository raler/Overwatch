using agsXMPP.protocol.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Overwatch
{
    [XmlRoot("body")]
    public class SummonerStatus
    {
        [XmlElement("profileIcon")]
        public string ProfileIcon;

        [XmlElement("level")]
        public string Level;

        [XmlElement("wins")]
        public string Wins;

        [XmlElement("leaves")]
        public string Leaves;

        [XmlElement("odinWIns")]
        public string OdinWIns;

        [XmlElement("odinLeaves")]
        public string OdinLeaves;

        [XmlElement("queueType")]
        public string QueueType;

        [XmlElement("rankedLosses")]
        public string RankedLosses;

        [XmlElement("rankedRating")]
        public string RankedRating;

        [XmlElement("tier")]
        public string Tier;

        [XmlElement("rankedSoloRestricted")]
        public string RankedSoloRestricted;

        [XmlElement("rankedLeagueName")]
        public string RankedLeagueName;

        [XmlElement("rankedLeagueDivision")]
        public string RankedLeagueDivision;

        [XmlElement("rankedLeagueTier")]
        public string RankedLeagueTier;

        [XmlElement("rankedLeagueQueue")]
        public string RankedLeagueQueue;

        [XmlElement("rankedWins")]
        public string RankedWins;

        [XmlElement("championMasteryScore")]
        public string ChampionMasteryScore;

        [XmlElement("statusMsg")]
        public string StatusMsg;

        [XmlElement("skinname")]
        public string Champion;

        [XmlElement("gameQueueType")]
        public string CurrentQueueType;

        [XmlElement("gameStatus")]
        public string GameStatus;

        [XmlElement("timeStamp")]
        public double Timestamp;

        public ShowType Show;
    }
}
