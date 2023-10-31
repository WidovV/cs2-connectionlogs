using ConnectedPlayers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils; // This is actually used
using System.Reflection;

namespace ConnectionLogs;

public class ConnectionLogs : BasePlugin
{
    public override string ModuleName => "Connection logs";

    public override string ModuleVersion => "0.1";
    public override void Load(bool hotReload)
    {
        Console.WriteLine(Environment.NewLine + Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.Magenta;
        new CFG().CheckConfig(ModuleDirectory);
        Console.WriteLine($"[{DateTime.Now}] Loaded {ModuleName} ({ModuleVersion})");

        foreach (PropertyInfo prop in CFG.config.GetType().GetProperties())
        {
            Console.WriteLine($"{prop.Name}: {prop.GetValue(CFG.config)}");
        }
        Console.ResetColor();

        Console.WriteLine(Environment.NewLine + Environment.NewLine);
    }

    // Make a method that prints to discord and does not use async

    [GameEventHandler]
    public HookResult OnClientConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (IsClient.Bot(@event.Userid))
        {
            return HookResult.Continue;
        }

        if (CFG.config.StoreInDatabase)
        {
            Queries.InsertUser(@event.Userid.SteamID.ToString(), @event.Userid.PlayerName);
        }

        if (CFG.config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(CFG.config.DiscordWebhook, true, @event.Userid);
        }

        return HookResult.Continue;
    }

    [GameEventHandler(HookMode.Pre)]
    public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (IsClient.Bot(@event.Userid))
        {
            return HookResult.Continue;
        }

        if (CFG.config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(CFG.config.DiscordWebhook, false, @event.Userid);
        }

        return HookResult.Continue;
    }



    [ConsoleCommand("connectedplayers", "get every connected player")]
    public void ConnectedPlayers(CCSPlayerController player, CommandInfo info)
    {
        if (!CFG.config.StoreInDatabase)
        {
            player.PrintToChat($"{CFG.config.ChatPrefix} This command is disabled");
            return;
        }

        if (!IsValidCommandUsage(player, info.ArgCount))
        {
            return;
        }

        List<User> users = Queries.GetConnectedPlayers();

        if (player == null)
        {
            foreach (var p in users)
            {
                Server.PrintToConsole($"{p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
            }
            return;
        }

        foreach (var p in users)
        {
            player.PrintToChat($"{CFG.config.ChatPrefix} {p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
        }
    }

    private bool IsValidCommandUsage(CCSPlayerController player, int args)
    {
        if (args != 1)
        {
            if (player != null)
            {
                player.PrintToChat($"{CFG.config.ChatPrefix} Usage: !connectedplayers");
            }
            else
            {
                Server.PrintToConsole($"{CFG.config.ChatPrefix} Usage: !connectedplayers");
            }

            return false;
        }

        return true;
    }
}
