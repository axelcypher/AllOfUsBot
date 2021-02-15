﻿using Impostor.Api.Events;
using Impostor.Api.Net.Inner.Objects;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using XtraCube.Plugins.AllofUs;
using System.Collections.Generic;
using System;

namespace XtraCube.Plugins.AllOfUs.Handlers
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<AllOfUsPlugin> _logger;

        public async ValueTask<int> SendMessage(IInnerPlayerControl player, string message)
        {
            string name = player.PlayerInfo.PlayerName;
            byte color = player.PlayerInfo.ColorId;
            await player.SetNameAsync("[FF0000FF]Amongo");
            await player.SetColorAsync((byte)0);
            await player.SendChatAsync(message);
            await player.SetNameAsync(name);
            await player.SetColorAsync(color);
            return 0;
        }

        public GameEventListener(ILogger<AllOfUsPlugin> logger)
        {
            _logger = logger;
        }

        [EventListener]
        public async void OnLobbyCreate(IPlayerSpawnedEvent e)
        {
            if (e.ClientPlayer.IsHost)
            {
                await Task.Delay(1000);
                await SendMessage(e.PlayerControl, "Welcome to the server!");
                await SendMessage(e.PlayerControl, "Type /help for a list of all the commands!");
            }

        }

        [EventListener]
        public async void OnPlayerChat(IPlayerChatEvent e)
        {
            string[] args = e.Message.Trim().ToLower().Split(" ");
            string name = e.PlayerControl.PlayerInfo.PlayerName;
            char[] alpha = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            byte color = e.PlayerControl.PlayerInfo.ColorId;
            Dictionary<string, int> Colors = new Dictionary<string, int>();
            Colors.Add("red", 0);
            Colors.Add("blue", 1);
            Colors.Add("green", 2);
            Colors.Add("pink", 3);
            Colors.Add("orange", 4);
            Colors.Add("yellow", 5);
            Colors.Add("black", 6);
            Colors.Add("white", 7);
            Colors.Add("purple", 8);
            Colors.Add("brown", 9);
            Colors.Add("cyan", 10);
            Colors.Add("lime", 11);
            if (e.Game.GameState == GameStates.NotStarted)
            {
                switch (args[0])
                {
                    case "/help":
                        if (e.ClientPlayer.IsHost)
                        {
                            await SendMessage(e.PlayerControl, "Commands: /map, /name, /color, /playerlimit, /implimit, /help");
                            await SendMessage(e.PlayerControl, "Type /<command name> to learn how to use it");
                        }
                        else
                        {
                            await SendMessage(e.PlayerControl, "Commands: /name, /color, /help");
                            await SendMessage(e.PlayerControl, "Type /<command name> to learn how to use it");
                        }
                        break;
                    case "/color":
                        if (args.Length > 1)
                        {
                            if (Colors.ContainsKey(args[1]))
                            {
                                await SendMessage(e.PlayerControl, "Color changed successfuly!");
                                await e.PlayerControl.SetColorAsync((byte)Colors[args[1]]);
                                break;
                            }
                            else
                            {
                                await SendMessage(e.PlayerControl, "[FF0000FF]Invalid color!");
                                await SendMessage(e.PlayerControl, "Available colors: Red, Blue, Green, Pink, Orange, Yellow, Black, White, Purple, Brown, Cyan, Lime");
                                break;
                            }
                            }
                        else if (args.Length == 1)
                        {
                            await SendMessage(e.PlayerControl, "/color {color}\n Change your color!");
                            await SendMessage(e.PlayerControl, "Available colors: Red, Blue, Green, Pink, Orange, Yellow, Black, White, Purple, Brown, Cyan, Lime");
                        }
                        break;
                    case "/name":
                        if (args.Length > 1)
                        {
                            if (args[1].Length < 11 && args[1].Length > 3)
                            {
                                bool nameUsed = false;
                                foreach (var player in e.Game.Players)
                                {
                                    string playerName = player.Character.PlayerInfo.PlayerName;
                                    if (playerName == args[1])
                                    {
                                        nameUsed = true;
                                        await SendMessage(e.PlayerControl, "Name already taken!");
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                if (!nameUsed)
                                {
                                    char[] requestedName = args[1].ToCharArray();
                                    bool nameInvalid = false;
                                    foreach (char letter in requestedName)
                                    {
                                        bool invalidCharExists = !(Array.Exists(alpha, character => character == letter));
                                        if (invalidCharExists)
                                        {
                                            nameInvalid = true;
                                        }
                                    }
                                    if (!nameInvalid)
                                    {
                                        await SendMessage(e.PlayerControl, "Name changed successfully!");
                                        await e.PlayerControl.SetNameAsync($"{args[1]}");
                                    }
                                    else if (nameInvalid)
                                    {
                                        await SendMessage(e.PlayerControl, "Name contains characters that are not in the standard alphabet.");
                                    }
                                }
                            }
                            else
                            {
                                await SendMessage(e.PlayerControl, "Name is too long or too short! Please try a different name!");
                            }
                        }
                        else if (args.Length == 1)
                        {
                            await SendMessage(e.PlayerControl, "/name {name}\n Change your name!");
                        }
                        break;

                    case "/implimit":
                        if (e.ClientPlayer.IsHost)
                        {
                            if (args.Length > 1)
                            {
                                bool tryLimit = int.TryParse(args[1], out int limit);
                                if (tryLimit)
                                {
                                    if (limit < 127 && limit > 0)
                                    {
                                        if ((e.Game.Options.MaxPlayers / limit) > 2f)
                                        {
                                            e.Game.Options.NumImpostors = (byte)limit;
                                            await SendMessage(e.PlayerControl, $"Impostor limit has been set to {args[1]}!");
                                            await e.Game.SyncSettingsAsync();
                                        }
                                        else
                                        {
                                            await SendMessage(e.PlayerControl, "Impostor limit is too high! Please make it lower!");
                                        }
                                    }
                                    else
                                    {
                                        await SendMessage(e.PlayerControl, "[FF0000FF]Error: Impostor limit can only be greater or equal to than 1 and less than 64!");
                                    }
                                }
                                else
                                {
                                    await SendMessage(e.PlayerControl, "[FF0000FF]Error: Please enter a number! If you did enter a number, then there was an error!");
                                }
                            }
                            if (args.Length == 1)
                            {
                                await SendMessage(e.PlayerControl, "/implimit {amount}\nSet the maximum impostor count. Max is 63, minimum is 1");
                            }
                        }
                        else
                        {
                            await SendMessage(e.PlayerControl, "[FF0000FF] You can't use that command!");
                        }
                        break;
                    case "/playerlimit":
                        if (e.ClientPlayer.IsHost)
                        {
                            if (args.Length > 1)
                            {
                                bool tryLimit = int.TryParse(args[1], out int limit);
                                if (tryLimit)
                                {
                                    if (limit < 127 && limit > 4)
                                    {
                                        e.Game.Options.MaxPlayers = (byte)limit;
                                        await SendMessage(e.PlayerControl, $"Player limit has been set to {args[1]}!\nNote: The counter will not change until someone joins/leaves!");
                                        await e.Game.SyncSettingsAsync();
                                    }
                                    else
                                    {
                                        await SendMessage(e.PlayerControl, "[FF0000FF]Error: Player limit can only be greater than 3 and less than 128!");
                                        await e.Game.SyncSettingsAsync();
                                    }
                                }
                                else
                                {
                                    await SendMessage(e.PlayerControl, "[FF0000FF]Error: Please enter a number! If you did enter a number, then there was an error!");
                                }
                            }
                            if (args.Length == 1)
                            {
                                await SendMessage(e.PlayerControl, "/playerlimit {amount}\nSet the maximum player limit. Max is 127, minimum is 4");
                                await SendMessage(e.PlayerControl, "Note: The counter will not change until someone joins/leaves!");
                            }
                        }
                        else
                        {
                            await SendMessage(e.PlayerControl, "[FF0000FF] You can't use that command!");
                        }
                        break;

                    case "/map":
                        if (e.ClientPlayer.IsHost)
                        {
                            if (args.Length > 1) 
                            {
                                string mapname = args[1];
                                switch (mapname)
                                {
                                    case "skeld":
                                        e.Game.Options.Map = (MapTypes)(byte)MapTypes.Skeld;
                                        await e.Game.SyncSettingsAsync();
                                        await SendMessage(e.PlayerControl, "Map has been set to The Skeld!");
                                        break;

                                    case "mira":
                                        e.Game.Options.Map = (MapTypes)(byte)MapTypes.MiraHQ;
                                        await e.Game.SyncSettingsAsync();
                                        await SendMessage(e.PlayerControl, "Map has been set to MiraHQ!");
                                        break;
                                    case "mirahq":
                                        e.Game.Options.Map = (MapTypes)(byte)MapTypes.MiraHQ;
                                        await e.Game.SyncSettingsAsync();
                                        await SendMessage(e.PlayerControl, "Map has been set to MiraHQ!");
                                        break;
                                    case "polus":
                                        e.Game.Options.Map = (MapTypes)(byte)MapTypes.Polus;
                                        await e.Game.SyncSettingsAsync();
                                        await SendMessage(e.PlayerControl, "Map has been set to Polus!");
                                        break;
                                    default:
                                        await SendMessage(e.PlayerControl, "[FF0000FF]Error: That is not a map!\nAvailable Maps: Skeld, Mira/MiraHQ, Polus");
                                        break;
                                }
                            }
                            if (args.Length == 1)
                            {
                                await SendMessage(e.PlayerControl, "/map {map}\nSet the map without making a new lobby! Maps: Skeld, Mira/MiraHQ, Polus");
                            }
                        }
                        else await SendMessage(e.PlayerControl, "[FF0000FF] You can't use that command!");
                        break;
                    case "/about":
                        await SendMessage(e.PlayerControl, "All of Us is a 100 Player Mod for Among Us developed by XtraCube and Pure, and is based on a mod by andry08.");
                        await SendMessage(e.PlayerControl, "All Of Us Bot is a server plugin for the custom Among Us server, Impostor. It can't be used to play 100 player games without the client mod!");
                        await SendMessage(e.PlayerControl, "This server is using the public version of the mod with stripped down features!");
                        break;
                }

            }
        }
    }

}
