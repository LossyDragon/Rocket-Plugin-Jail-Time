//Original Author: ApokPT - https://github.com/ApokPT
using Rocket.Unturned.Player;
using System;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using System.Collections.Generic;
using Rocket.Core.Logging;

namespace ApokPT.RocketPlugins
{
    public class JailTimeCommand : IRocketCommand
    {
        //Allowed from Rocket console.
        public bool AllowFromConsole
        {
            get { return false; }
        }

        //Plugin name.
        public string Name
        {
            get { return "jail"; }
        }

        //Return help (Overriden).
        public string Help
        {
            get { return "Send players to Jail!"; }
        }

        //Return syntax (Overriden).
        public string Syntax
        {
            get { return "NULL"; }
        }

        //Aliases
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        //Permissions
        public List<string> Permissions
        {
            get { return new List<string>() { "jail" }; }
        }

        //Start.
        public void Execute(IRocketPlayer caller, string[] command)
        {
            string commandString = String.Join(" ", command);

            UnturnedPlayer pCaller = (UnturnedPlayer)caller;

            if (pCaller.IsAdmin && !JailTime.Instance.Configuration.Instance.Enabled) return;

            //If called from Console
            if (caller == null)
            {
                Logger.LogWarning("This command cannot be called from the console.");
                return;
            }

            //If command is empty, ?, or help.
            //Added Logger to help debug commands.
            if (String.IsNullOrEmpty(commandString.Trim()) || command[0].Trim() == "?" || command[0].Trim().ToLower() == "help")
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help"));
                Logger.LogWarning("Issued jailtime_help_add to Steam user: " + pCaller.SteamName + ".");
                return;
            }
            else
            {
                string[] oper = commandString.Split(' ');

                //Added Loggers to help debug commands. 
                if (oper.Length == 1)
                {
                    switch (oper[0])
                    {
                        case "add":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_add"));
                            Logger.LogWarning("Issued jailtime_help_add to Steam user: " + pCaller.SteamName + ".");
                            break;
                        case "remove":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_remove"));
                            Logger.LogWarning("Issued jailtime_help_remove to Steam user: " + pCaller.SteamName + ".");
                            break;
                        case "list":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_list"));
                            Logger.LogWarning("Issued jailtime_help_list to Steam user: " + pCaller.SteamName + ".");
                            break;
                        case "set":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_set"));
                            Logger.LogWarning("Issued jailtime_help_set to Steam user: " + pCaller.SteamName + ".");
                            break;
                        case "unset":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_unset"));
                            Logger.LogWarning("Issued jailtime_help_unset to Steam user: " + pCaller.SteamName + ".");
                            break;
                        case "teleport":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_teleport"));
                            Logger.LogWarning("Issued jailtime_help_teleport to Steam user: " + pCaller.SteamName + ".");
                            break;
                        default:
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help"));
                            Logger.LogWarning("Issued jailtime_help to Steam user: " + pCaller.SteamName + ".");
                            break;
                    }
                    return;
                }
                else
                {

                    string[] param = string.Join(" ", oper.Skip(1).ToArray()).Split(' ');

                    switch (oper[0])
                    {
                        case "add":
                            if (param.Length == 1)
                            {
                                // Arrest player in random cell for default time - /jail add apok
                                JailTime.Instance.addPlayer(caller, string.Join(" ", param.ToArray()));
                            }
                            else if (param.Length == 2)
                            {
                                // Arrest player in random cell for defined time - /jail add apok 20
                                JailTime.Instance.addPlayer(caller, param[0], "", Convert.ToUInt32(param[1]));
                            }
                            else
                            {
                                // Arrest player in specific cell for defined time - /jail add apok 20 cell 1
                                JailTime.Instance.addPlayer(caller, param[0], string.Join(" ", param.Skip(2).ToArray()), Convert.ToUInt32(param[1]));
                            }
                            break;
                        case "remove":
                            JailTime.Instance.removePlayer(pCaller, string.Join(" ", param.ToArray()));
                            break;
                        case "list":
                            switch (param[0])
                            {
                                case "players":
                                    JailTime.Instance.listPlayers(caller);
                                    break;
                                case "cells":
                                    JailTime.Instance.listJails(caller);
                                    break;
                            }
                            break;
                        case "set":
                            JailTime.Instance.setJail(pCaller, string.Join(" ", param.ToArray()), pCaller.Position);
                            break;
                        case "unset":
                            JailTime.Instance.unsetJail(caller, string.Join(" ", param.ToArray()));
                            break;
                        case "teleport":
                            JailTime.Instance.teleportToCell(pCaller, string.Join(" ", param.ToArray()));
                            break;
                        default:
                            break;
                    }

                }
            }
        }
    }
}
