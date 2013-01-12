﻿using System;
using System.Text;
using SteamKit2;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Web;

namespace SteamBot.Trading
{
    /*
     * Trade Status
     * 1 - Trade Completed
     * 2 - 
     * 3 - Trade Cancelled (by them)
     * 4 - Parter Timed out
     * 5 - Failed (?)
     */

    /*
     * Event Actions
     * 0 - Add Item
     * 1 - Remove Item
     * 2 - Ready
     * 3 - Unready
     * 4 - 
     * 5 - 
     * 6 - Currency(?)
     * 7 - Message
     */
    public class Api
    {

        public Web web { get; set; }
        public BotHandler botHandler { get; set; }
        public SteamID otherSID { get; set; }

        private string sessionId;
        private string steamLogin;
        private string baseTradeUri;

        /// <summary>
        /// The position of the event log we are on.
        /// </summary>
        private int logPos { get; set; }

        /// <summary>
        /// The version of the item lists each player has put up.
        /// </summary>
        private int version { get; set; }

        /// <summary>
        /// Becaause whenever we do an action, such as chat, additem, or
        /// removeitem, the server sends back a status.  This is always
        /// called whenever we recieve a status (even in GetStatus).
        /// </summary>
        /// <param name="status">The status the server sent.</param>
        /// <returns></returns>
        public delegate void StatusUpdate(Status status);
        public StatusUpdate StatusUpdater;

        static string SteamTradeUri = "/trade/{0}/";
        static string SteamCommunityDomain = "steamcommunity.com";

        public Api(SteamID OtherSID, BotHandler botHandler)
        {
            this.otherSID = OtherSID;
            this.botHandler = botHandler;
            this.web = new Web(botHandler.web);
            web.Domain = SteamCommunityDomain;
            web.Scheme = "http";
            web.ActAsAjax = true;
            logPos = 0;
            version = 0;

            foreach (System.Net.Cookie cookie in web.Cookies.GetCookies(new Uri("http://steamcommunity.com")))
            {
                Console.WriteLine(cookie);
            }

            sessionId = Uri.UnescapeDataString(botHandler.sessionId);
            steamLogin = botHandler.steamLogin;
            baseTradeUri = String.Format(SteamTradeUri, otherSID.ConvertToUInt64().ToString());
        }

        /// <summary>
        /// Retrieves the status of the server and runs it through StatusUpdater.
        /// </summary>
        /// <returns>The status of the server.</returns>
        public Status GetStatus()
        {
            string result = web.Do(baseTradeUri + "tradestatus", "POST", GetData());
            Status status = JsonConvert.DeserializeObject<Status>(result);
            StatusUpdater(status);
            return status;
        }

        /// <summary>
        /// Send a message to the chat.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>The status of the server.</returns>
        public Status SendMessage(string message)
        {
            NameValueCollection data = GetData();
            data.Add("message", message);
            string result = web.Do(baseTradeUri + "chat", "POST", data);
            Status status = JsonConvert.DeserializeObject<Status>(result);
            StatusUpdater(status);
            return status;
        }

        NameValueCollection GetData()
        {
            NameValueCollection data = new NameValueCollection();
            data.Add("sessionid", sessionId);
            data.Add("logpos", "" + logPos);
            data.Add("version", "" + version);
            return data;
        }

        #region JSON Responses
        public class Status
        {

            [JsonProperty("error")]
            public string Error { get; set; }

            [JsonProperty("newversion")]
            public bool NewVersion { get; set; }

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("trade_status")]
            public long TradeStatus { get; set; }

            [JsonProperty("version")]
            public int Version { get; set; }

            [JsonProperty("logpos")]
            public int Logpos { get; set; }

            [JsonProperty("me")]
            public TradeUser Bot { get; set; }

            [JsonProperty("them")]
            public TradeUser Other { get; set; }

        }

        public class TradeUser 
        {

            [JsonProperty("ready")]
            public int Ready { get; set; }

            [JsonProperty("confirmed")]
            public int Confirmed { get; set; }

            [JsonProperty("sec_since_touch")]
            public int SecondsSinceTouch { get; set; }

        }

        public class TradeEvent
        {

            [JsonProperty("steamid")]
            public string SteamId { get; set; }

            [JsonProperty("action")]
            public int Action { get; set; }

            [JsonProperty("timestamp")]
            public ulong Timestamp { get; set; }

            [JsonProperty("appid")]
            public int AppId { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("contextid")]
            public int ContextId { get; set; }

            [JsonProperty("assetid")]
            public ulong AssetId { get; set; }

        }
        #endregion
    }
}
