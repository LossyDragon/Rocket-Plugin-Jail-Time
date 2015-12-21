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

        //Return syntax.
        public string Syntax
        {
            get { return "<add | remove | list | set | unset | t>"; }
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
                        case "t":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_teleport"));
                            break;
                        case "teleport":
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help_teleport"));
                            break;
                        default:
                            UnturnedChat.Say(caller, JailTime.Instance.Translate("jailtime_help"));
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
                            

                            uint tmp;

                            if (uint.TryParse(param[1], out tmp))
                            {
                                // Arrest player in random cell for defined time - /jail add apok 20
                                JailTime.Instance.addPlayer(caller, param[0], "", tmp);
                            }
                            else
                            {
                                //Arrest player in specific cell, thats single-worded, for DEFAULT time - /jail add lossy cell
                                JailTime.Instance.addPlayer(caller, param[0], param[1]);
                            }
                        }
                        else
                        {

                            uint tmp2;

                            if (uint.TryParse(param[1], out tmp2))
                            {
                                // Arrest player in specific cell for defined time - /jail add apok 20 cell 1
                                JailTime.Instance.addPlayer(caller, param[0], string.Join(" ", param.Skip(2).ToArray()), tmp2);
                            }                     
                            else
                            {
                                //Botch job, ;)
                                //Arrest player in specific cell, thats two-worded, for DEFAULT time - /jail add lossy cell 1
                                JailTime.Instance.addPlayer(caller, param[0], string.Join(" ", param.Skip(1).ToArray()));
                            }
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
                            //case(s) 't' and 'teleport' are the same.
                        case "t":
                            JailTime.Instance.teleportToCell(pCaller, string.Join(" ", param.ToArray()));
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
