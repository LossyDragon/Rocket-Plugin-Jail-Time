using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;

//This class is to Ban the player on reconnect. Should allow the player to connect then to be banned instead of getting stuck on 'Loading'.
namespace ApokPT.RocketPlugins
{
    public class JailTimePlayer
    {
        private bool banPlayer;
        private DateTime startTime;
        private UnturnedPlayer playerData;
        private float ping;

        internal void SetBan(UnturnedPlayer player)
        {
            startTime = DateTime.Now;
            banPlayer = true;
            playerData = player;

            //TODO Debug.
            Logger.Log(playerData.SteamName + "in SetBan class.");

            if (banPlayer)
            {
                try
                {
                    ping = playerData.Ping;

                    if (ping == 0)
                        ping = 1;

                    //TODO Debug.
                    Logger.Log("Player's ping: " + ping);
                    Logger.Log("Time Now: " + startTime);
                    Logger.Log("Total Seconds: " +(DateTime.Now - startTime).TotalSeconds);
                    Logger.Log("Config Set Grace: " + JailTime.Instance.Configuration.Instance.GracePeriod);
                    Logger.Log("Grace: " + (JailTime.Instance.Configuration.Instance.GracePeriod + (ping * 10)));


                    //TODO try and fix delay.
                    if ((DateTime.Now - startTime).TotalSeconds >= JailTime.Instance.Configuration.Instance.GracePeriod + (ping * 10))
                    {
                        Logger.Log("Attempt to ban " + playerData.SteamName);
                        if (JailTime.Instance.Configuration.Instance.BanOnReconnectTime > 0)
                            {
                                SteamBlacklist.ban(playerData.CSteamID, (CSteamID)0UL , JailTime.Instance.Translate("jailtime_ban_time"), JailTime.Instance.Configuration.Instance.BanOnReconnectTime);
                                SteamBlacklist.save();

                                Logger.Log("Banned player " + playerData.SteamName + " temporarily. Ban-On-Reconnect.");
                            }
                            else
                            {
                                SteamBlacklist.ban(playerData.CSteamID, (CSteamID)0L, JailTime.Instance.Translate("jailtime_ban"), 1000000000);
                                SteamBlacklist.save();

                                Logger.Log("Banned player " + playerData.SteamName + " permanently. Ban-On-Reconnect.");
                            }
                        }

                        banPlayer = false;

                    }
                catch(Exception e)
                {
                    Logger.LogWarning("JailTime BAN error: " + e.Message);
                }
            }
        }
    }
}
