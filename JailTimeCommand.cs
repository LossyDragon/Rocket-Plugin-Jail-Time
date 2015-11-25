using Rocket.Unturned.Player;
using System;
using System.Linq;
using Rocket.Unturned.Chat;
using Rocket.API;
using System.Collections.Generic;

namespace ApokPT.RocketPlugins
{
    public class JailTimeCommand : IRocketCommand
    {
        public bool AllowFromConsole
        {
            get { return true; }
        }

        public string Name
        {
            get { return "jail"; }
        }

        public string Help
        {
            get { return "Send players to Jail!"; }
        }

        public string Syntax
        {
            get { return "NULL"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "JailTime.jail" }; }
        }


        public void Execute(IRocketPlayer caller, string[] command)
        {
            string commandString = String.Join(" ", command);

            UnturnedPlayer pCaller = (UnturnedPlayer)caller;

            if (pCaller.IsAdmin && !JailTime.Instance.Configuration.Instance.Enabled) return;

            if (String.IsNullOrEmpty(commandString.Trim()))
            {
                UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help"));
                return;
            }
            else
            {
                string[] oper = commandString.Split(' ');

                if (oper.Length == 1)
                {
                    switch (oper[0])
                    {
                        case "add":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_add"));
                            break;
                        case "remove":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_remove"));
                            break;
                        case "list":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_list"));
                            break;
                        case "set":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_set"));
                            break;
                        case "unset":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_unset"));
                            break;
                        case "teleport":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_teleport"));
                            break;
                        default:
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
