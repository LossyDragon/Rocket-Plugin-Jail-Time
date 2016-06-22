using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rocket.Unturned.Chat;
using Rocket.API;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using SDG.Unturned;
using Rocket.Unturned;
using Rocket.Core.Logging;
using System.Linq;

namespace ApokPT.RocketPlugins
{

    public class JailTime : RocketPlugin<JailTimeConfiguration>
    {
        private Dictionary<string, Cell> cells = new Dictionary<string, Cell>();
        private Dictionary<string, Sentence> players = new Dictionary<string, Sentence>();

        // Singleton
        public static JailTime Instance;

        //Load
        protected override void Load()
        {
            Instance = this;
            Logger.LogWarning(JailTime.Instance.Translate("jailtime_console_display"));

            if (JailTime.Instance.Configuration.Instance.Enabled)
            {
                UnturnedPlayerEvents.OnPlayerRevive += Events_OnPlayerRevive;
                U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            }

            //This function will load cells from configuration.
            injectConfiCells();
        }

        //Rocket Unload.
        protected override void Unload()
        {
            //This function will unload cells from configuration.
            uninjectConfiCells();

            UnturnedPlayerEvents.OnPlayerRevive -= Events_OnPlayerRevive;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
        }

        //Load cells from configuration file.
        private void injectConfiCells()
        {
            foreach (CellLoc cell in Configuration.Instance.Cells)
            {
                setJail(null, cell.Name.ToLower(), new Vector3(Convert.ToSingle(cell.X), Convert.ToSingle(cell.Y), Convert.ToSingle(cell.Z)));

            }
        }

        //(Rocket unload) Unload players in jail to last location and flush the dictionaries.
        private void uninjectConfiCells()
        {
            //Should Sentence be removed during Unload...
            if (JailTime.Instance.Configuration.Instance.ReleasePlayerOnUnload)
            {
                List<string> keys = new List<string>(players.Keys);

                foreach (string key in keys)
                {
                    removePlayer(null, key);
                }

                //Flush both dictionaries, and local List<>.
                cells.Clear();
                players.Clear();
                keys.Clear();

            }
        }

        //Permissions (Overridden)
        public List<string> Permissions
        {
            get
            {
                return new List<string>() {};
            }
        }

        //Events for On Player connected

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {

            if (player.IsAdmin || player.HasPermission("jail.immune")) return;

            if (players.ContainsKey(player.ToString()))
            {

                if (JailTime.Instance.Configuration.Instance.BanOnReconnect)
                {
                    //removePlayerFromJail(player, players[player.ToString()]);
                    //players.Remove(player.ToString());

                    //Send player over to Player Component (JailTimePlayer) to handle ban on reconnect.

                    //Debug.
                    Logger.Log("Sending " + player.SteamName + " to ban on reconnect.");
                    Logger.Log("Ping: " + player.Ping);

                    JailTimePlayer jailtimeplayer = player.GetComponent<JailTimePlayer>();
                    jailtimeplayer.SetBan(player);
                    players.Remove(player.ToString());

                }
                else
                {
                    if (!(players[player.ToString()].End <= DateTime.Now))
                    {
                        movePlayerToJail_OnPlayerRevive_Connect(player, players[player.ToString()].Cell);
                        UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
                    }
                }
            }
        }

        //Event for On Player Revive (Death or Suicide).
        private void Events_OnPlayerRevive(UnturnedPlayer player, Vector3 position, byte angle)
        {
            if (player.IsAdmin || player.HasPermission("jail.immune")) return;

            if (players.ContainsKey(player.ToString()))
            {
                movePlayerToJail_OnPlayerRevive_Connect(player, players[player.ToString()].Cell);
                UnturnedChat.Say(player, JailTime.Instance.Translate("jailtime_player_back_msg"));
            }
        }

        //This function will handle OnPlayerRevive/Connected to re-strip(if enabled) player and sending back to jail without dupping clothes.
        private void movePlayerToJail_OnPlayerRevive_Connect(UnturnedPlayer player, Cell jail)
        {
            if (Instance.Configuration.Instance.StripInventory)
            {
                InvClear(player);
            }

            player.Teleport(jail.Location, player.Rotation);
        }


        //Fixed Update
        public void FixedUpdate()
        {
            if (this.State == PluginState.Loaded && players != null && players.Count != 0)
            {
                foreach (KeyValuePair<string, Sentence> pl in players)
                {
                    if (pl.Value.End <= DateTime.Now)
                    {
                        try
                        {
                            removePlayer(null, pl.Key);
                            break;
                        }
                        catch { }
                    }


                    try
                    {
                        UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(pl.Key)));

                        if (player != null && Vector3.Distance(player.Position, pl.Value.Cell.Location) > Configuration.Instance.WalkDistance)
                        {
                            if (Configuration.Instance.KillInsteadOfTeleport)
                            {
                                player.Damage(255, player.Position, EDeathCause.PUNCH, ELimb.SKULL, player.CSteamID);
                            }
                            else
                            {
                                player.Teleport(pl.Value.Cell.Location, player.Rotation);
                            }
                        }
                    }
                    catch { }

                }
            }
        }

        //Private Methods 
        //Get cells by name.
        private Cell getCellbyName(string jailName)
        {

            return cells.ContainsKey(jailName) ? cells[jailName] : null;
        }

        //Get random cell.
        private Cell getRandomCell()
        {
            if (cells.Count >= 0)
            {
                List<string> keys = new List<string>(cells.Keys);
                System.Random rand = new System.Random();
                return cells[keys[rand.Next(cells.Count)]];

            }

            return null;
        }

        //Player Methods
        //Adding a player
        internal void addPlayer(IRocketPlayer caller, string playerName, string jailName = "", uint jailTime = 0)
        {

            Cell jail;
            UnturnedPlayer target = UnturnedPlayer.FromName(playerName);

            if (jailTime == 0) jailTime = Configuration.Instance.JailTimeInSeconds;

            if (target == null)
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", jailName));
                return;
            }
            else if (players.ContainsKey(target.ToString()))
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_in_jail", target.CharacterName));
                return;
            }
            else
            {
                if (target.IsAdmin || target.HasPermission("jail.immune") || target.HasPermission("jail"))
                {
                    UnturnedChat.Say(target, JailTime.Instance.Translate("jailtime_player_immune"));
                    return;
                }
                else if (cells.Count == 0)
                {
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_notset", jailName));
                    return;
                }
                else if (jailName == "")
                {
                    jail = getRandomCell();
                }
                else
                {
                    jail = getCellbyName(jailName);
                }

                if (jail == null)
                {
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                    return;
                }

                players.Add(target.ToString(), new Sentence(jail, jailTime, target.Position));
                movePlayerToJail(target, jail);

                UnturnedChat.Say(target, JailTime.Instance.Translate("jailtime_player_arrest_msg", jailTime));
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_arrested", target.CharacterName, jail.Name));
            }
        }

        //Removing a player
        internal void removePlayer(UnturnedPlayer caller, string playerName)
        {
            UnturnedPlayer target;
            try
            {
                target = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(playerName)));
            }
            catch
            {
                target = UnturnedPlayer.FromName(playerName);
            }

            if (target != null && players.ContainsKey(target.ToString()))
            {
                removePlayerFromJail(target, players[target.ToString()]);
                UnturnedChat.Say(target, JailTime.Instance.Translate("jailtime_player_release_msg"));

                if (caller != null)
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_released", target.CharacterName));

                players.Remove(target.ToString());
            }
            else
            {
                if (caller != null)
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_notfound", playerName));

                return;
            }
        }

        //List players
        internal void listPlayers(IRocketPlayer caller)
        {
            if (players.Count == 0)
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_list_clear"));
                return;
            }
            else
            {
                string playersString = "";

                foreach (KeyValuePair<string, Sentence> player in players)
                {
                    try { playersString += UnturnedPlayer.FromName(player.Key).CharacterName + " (" + player.Value.Cell.Name + "), "; }
                    catch { }
                }

                if (playersString != "") playersString = playersString.Remove(playersString.Length - 2) + ".";

                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_player_list", playersString));
                return;
            }
        }

        // Jail Methods
        //Set Jail
        internal void setJail(UnturnedPlayer caller, string jailName, Vector3 location)
        {
            if (caller != null)
            {
                if (cells.ContainsKey(jailName.ToLower()))
                {
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_exists", jailName));
                    return;
                }
                else
                {
                    UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_set", jailName));

                }
                Configuration.Instance.Cells.Add(new CellLoc(jailName, location.x, location.y, location.z));
                Configuration.Save();
            }

            cells.Add(jailName.ToLower(), new Cell(jailName, location));
        }

        //Unset Jail
        internal void unsetJail(IRocketPlayer caller, string jailName)
        {
            if (!cells.ContainsKey(jailName.ToLower()))
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                return;
            }
            else
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_unset", jailName));
                cells.Remove(jailName.ToLower());
                foreach (CellLoc cell in Configuration.Instance.Cells)
                {
                    if (cell.Name.ToLower() == jailName.ToLower())
                    {
                        Configuration.Instance.Cells.Remove(cell);
                        Configuration.Save();
                        return;
                    }
                }
            }
        }

        //TP to a cell
        internal void teleportToCell(UnturnedPlayer caller, string jailName)
        {
            if (!cells.ContainsKey(jailName.ToLower()))
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_notfound", jailName));
                return;
            }
            else
            {
                caller.Teleport(cells[jailName.ToLower()].Location, caller.Rotation);
            }
        }

        //List Jails
        internal void listJails(IRocketPlayer caller)
        {
            if (cells.Count == 0)
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_jail_notset"));
                return;
            }
            else
            {
                string jailsString = "";

                foreach (KeyValuePair<string, Cell> jail in cells)
                {
                    jailsString += jail.Value.Name + ", ";
                }

                if (jailsString != "") jailsString = jailsString.Remove(jailsString.Length - 2) + ".";

                UnturnedChat.Say(caller, Instance.Translate("jailtime_jail_list", jailsString));
                return;
            }
        }

        //Arrest Methods//
        //Move selected player to jail.
        private void movePlayerToJail(UnturnedPlayer player, Cell jail)
        {
            //If StripWeapons is TRUE, Remove player's inventory.
            if (Instance.Configuration.Instance.StripInventory)
            {
                InvClear(player);

                //if GiveClothes is TRUE, jail clothes will be given (to wear).
                if (JailTime.Instance.Configuration.Instance.GiveClothes)
                {
                    player.GiveItem(303, 1);
                    player.GiveItem(304, 1);
                }
            }
            player.Teleport(jail.Location, player.Rotation);
        }

        //Remove player from jail
        private void removePlayerFromJail(UnturnedPlayer player, Sentence sentence)
        {
            player.Teleport(sentence.Location, player.Rotation);
        }

        //Method to attempt inventory clear. | Credit doozzik https://github.com/doozzik/DropManager
        //Inventory removal works, but if a player has a gun in slots 1&2, the model of said item stays visible. 
        private void InvClear(UnturnedPlayer player)
        {
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                byte itemCount = player.Player.inventory.getItemCount(page);

                for (byte index = 0; index < itemCount; index++)
                {
                    try
                    {
                        player.Player.inventory.removeItem(page, index);
                        index--;
                        itemCount--;
                    }
                    catch (Exception e)
                    {
                            Logger.LogWarning(JailTime.Instance.Translate("jailtime_inv_playerInvError") + player.CharacterName);
                            Logger.LogWarning(JailTime.Instance.Translate("jailtime_inv_fullError") + e.Message);
                    }
                }
            }
        }

        // Translations
        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList(){
                    //Some log outputs.
                    {"jailtime_console_display", "JailTime by ApokPT. Updated by Lossy. " },
                    {"jailtime_inv_playerInvError", "JailTime: Cant clear inventory for player. "},
                    {"jailtime_inv_fullError", "JailTime: Full problem description: "},
                    //Jail information
                    {"jailtime_jail_notset","No cells set, please use /jail set [name] first! "},
                    {"jailtime_jail_notfound","No cell named {0} found! "},
                    {"jailtime_jail_set","New cell named {0} created where you stand! "},
                    {"jailtime_jail_exists","Cell named {0} already exists! "},
                    {"jailtime_jail_unset","Cell named {0} deleted! "},
                    {"jailtime_jail_list","Jail Cells: {0} "},
                    //Jail player information.
                    {"jailtime_player_immune","That player cannot be arrested! "},
                    {"jailtime_player_in_jail","Player {0} already in jail! "},
                    {"jailtime_player_arrested","Player {0} was arrested in {1} cell! "},
                    {"jailtime_player_released","Player {0} released from jail! "},
                    {"jailtime_player_list","Players: {0} "},
                    {"jailtime_player_list_clear","Jail cells are getting dusty! "},
                    {"jailtime_player_notfound","No player found named {0}! "},
                    {"jailtime_player_arrest_msg","You have been arrested for {0} seconds! "},
                    {"jailtime_player_release_msg","You have been released! "},
                    {"jailtime_player_back_msg","Get back in your cell! "},
                    //Command Help
                    {"jailtime_help","/jail commands: add, remove, set, unset, list, teleport "},
                    {"jailtime_help_add","use /jail add <player> <time> <cell> - to arrest a player, if no <cell> uses a random one "},
                    {"jailtime_help_remove","use /jail remove <player> - to release a player "},
                    {"jailtime_help_list","use /jail list players or /jail list cells "},
                    {"jailtime_help_set","use /jail set <cell> - to set a new jail cell "},
                    {"jailtime_help_unset","use /jail unset <cell> - to delete a jail cell "},
                    {"jailtime_help_teleport","use /jail teleport <cell> - to teleport to a cell "},
                    //Bans
                    {"jailtime_ban","You have been banned for disconnecting while in Jail! "},
                    {"jailtime_ban_time","You have been banned temporarily for disconnecting while in Jail! "}
                };
            }
        }
    }
}
