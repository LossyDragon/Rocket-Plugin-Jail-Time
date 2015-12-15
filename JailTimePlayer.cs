using Rocket.Core.Logging; //Used when Logger.Log is uncommented.
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;

//This class is to Ban the player on reconnect. Should allow the player to connect then to be banned instead of getting stuck on 'Loading'.
namespace ApokPT.RocketPlugins
{
    public class JailTimePlayer : UnturnedPlayerComponent
    {
        private bool banPlayer;
        private DateTime startTime;
        private UnturnedPlayer playerData;
        private float ping;

        protected override void Load()
        {
            banPlayer = false;
        }

        internal void SetBan(UnturnedPlayer player)
        {
            startTime = DateTime.Now;
            banPlayer = true;
            playerData = player;
        }

        public void FixedUpdate()
        { 

            //When player connects, run through iteration.
            if (banPlayer)
            {
                //Keep trying to ban player once ping had reached threshold.
                try
                {
                    ping = playerData.Ping;

                    if (ping == 0)
                    { ping = 1; }

                    //Debug.
                    //Logger.Log("Player's ping: " + ping);
                    //Logger.Log("Time Now: " + startTime);
                    //Logger.Log("Total Seconds: " +(DateTime.Now - startTime).TotalSeconds);
                    //Logger.Log("Config Set Grace: " + JailTime.Instance.Configuration.Instance.GracePeriod);
                    //Logger.Log("Grace: " + (JailTime.Instance.Configuration.Instance.GracePeriod + (ping * 10)));

                    if ((DateTime.Now - startTime).TotalSeconds >= JailTime.Instance.Configuration.Instance.GracePeriod + (ping * 10))
                    {
                        //Debug
                        //Logger.Log("Attempt to ban " + playerData.SteamName);

                        //Temp ban.
                        if (JailTime.Instance.Configuration.Instance.BanOnReconnectTime > 0)
                        {
                            SteamBlacklist.ban(playerData.CSteamID, (CSteamID)0UL , JailTime.Instance.Translate("jailtime_ban_time"), JailTime.Instance.Configuration.Instance.BanOnReconnectTime);
                            SteamBlacklist.save();

                            //Debug
                            //Logger.Log("Banned player " + playerData.SteamName + " temporarily. Ban-On-Reconnect.");
                        }

                        //Perma ban. 
                        else
                        {
                            SteamBlacklist.ban(playerData.CSteamID, (CSteamID)0L, JailTime.Instance.Translate("jailtime_ban"), 1000000000);
                            SteamBlacklist.save();

                            //Debug
                            //Logger.Log("Banned player " + playerData.SteamName + " permanently. Ban-On-Reconnect.");
                        }

                        banPlayer = false;
                    }

                }
                catch
                {
                    //Nothing to catch, run on next interval
                }
            }
        }
    }
}
