using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IrcDotNet;
using Stats.Common;

namespace StatsBot {

    class StatsBot : BasicIrcBot {

        public StatsBot() : base() {
            ChannelsToRejoin = new List<string>();

            //Defaults
            AutoReconnect = true;
            AutoRejoin = true;
            ReconnectAttempts = 0;
        }

        private bool AutoRejoin { get; set; }
        private bool AutoReconnect { get; set; }

        private int ReconnectAttempts { get; set; }

        //Rejoin these channels after disconnect
        private List<string> ChannelsToRejoin { get; set; }

        public override IrcRegistrationInfo RegistrationInfo {
            get {
                return new IrcUserRegistrationInfo() {
#if DEBUG
                    NickName = "buttes-TEST",
                    UserName = "StatsBot-TEST-BUILD",
                    RealName = "Statistics bot TEST BUILD"
#else

                    NickName = "buttes",
                    UserName = "StatsBot",
                    RealName = "Statistics bot"
#endif
                    
                };
            }
        }


        protected override void OnClientConnect(IrcClient client) {
            ReconnectAttempts = 0;
        }

        protected override void OnClientDisconnect(IrcClient client) {
            Thread.Sleep(100);

            if(ReconnectAttempts < 5 && !client.IsConnected) {
                Connect(Network, RegistrationInfo);
                ReconnectAttempts++;
            }
        }

        protected override void OnClientRegistered(IrcClient client) {
            Console.WriteLine("Registered");
            if (ChannelsToRejoin.Count != 0) {
                ChannelsToRejoin.ForEach(c => client.Channels.Join(c));
            }
        }

        protected override void OnLocalUserJoinedChannel(IrcLocalUser localUser, IrcChannelEventArgs e) {
            ChannelsToRejoin.Add(e.Channel.Name);

        }

        protected override void OnLocalUserLeftChannel(IrcLocalUser localUser, IrcChannelEventArgs e) {
            if(AutoRejoin) {
                localUser.Client.Channels.Join(e.Channel.Name);
            }
        }

        protected override void OnLocalUserNoticeReceived(IrcLocalUser localUser, IrcMessageEventArgs e) {

        }

        protected override void OnLocalUserMessageReceived(IrcLocalUser localUser, IrcMessageEventArgs e) {

        }

        protected override void OnLocalUserQuit(IrcLocalUser localUser, IrcCommentEventArgs e) {
            Console.WriteLine("Quit: {0}",e.Comment);


        }

        protected override void OnChannelUserJoined(IrcChannel channel, IrcChannelUserEventArgs e) {
            
        }

        protected override void OnChannelUserLeft(IrcChannel channel, IrcChannelUserEventArgs e) {
            
        }

        protected override void OnChannelNoticeReceived(IrcChannel channel, IrcMessageEventArgs e) {

        }

        protected override void OnChannelMessageReceived(IrcChannel channel, IrcMessageEventArgs e) {
            using (var logger = new StatsLogger()) {
                logger.LogMessage(e.Text, e.Source.Name, DateTime.UtcNow, channel.Name);
            }
        }

        protected override void OnChannelTopicChanged(IrcChannel channel, EventArgs e) {
            using (var logger = new StatsLogger()) {
                logger.LogTopicChange(channel.Topic, channel.Name, DateTime.UtcNow);
            }
        }


        protected override void InitializeCommandProcessors() {
            base.InitializeCommandProcessors();

            this.CommandProcessors.Add("autorejoin",ProcessCommandAutoRejoin);
            this.CommandProcessors.Add("autorj", ProcessCommandAutoRejoin);
            this.CommandProcessors.Add("autoreconnect", ProcessCommandAutoReconnect);
            this.CommandProcessors.Add("autorc", ProcessCommandAutoReconnect);
        }

        private void ProcessCommandAutoRejoin(string command, IList<string> parameters) {
            if(parameters.Count < 1) {
                throw new ArgumentException(Properties.Resources.MessageNotEnoughArgs);
            }

            switch(parameters[0]) {
                case "on":
                case "true":
                    AutoRejoin = true;
                    break;
                case "off":
                case "false":
                    AutoRejoin = false;
                    break;

            }
        }

        private void ProcessCommandAutoReconnect(string command, IList<string> parameters) {
            if(parameters.Count < 1) {
                throw new ArgumentException(Properties.Resources.MessageNotEnoughArgs);
            }

            switch(parameters[0]) {
                case "on":
                case "true":
                    AutoReconnect = true;
                    break;
                case "off":
                case "false":
                    AutoRejoin = false;
                    break;
            }
        }



        
    }
}
