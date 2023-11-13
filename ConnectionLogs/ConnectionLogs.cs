using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils; // This is actually used
using Nexd.MySQL;

namespace ConnectionLogs;

public class ConnectionLogs : BasePlugin
{
    public override string ModuleName => "Connection logs";

    public override string ModuleVersion => "0.2.1";

    public override string ModuleAuthor => "WidovV";
    public override string ModuleDescription => "Logs client connections to a database and discord.";

    private MySqlDb Db = null;
    public override void Load(bool hotReload)
    {
        Console.WriteLine(Environment.NewLine + Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.Magenta;
        new Cfg().CheckConfig(ModuleDirectory);
        Console.WriteLine($"[{DateTime.Now}] Loaded {ModuleName} ({ModuleVersion})");
        Console.ResetColor();

        Console.WriteLine(Environment.NewLine + Environment.NewLine);
        Db = new(Cfg.Config.DatabaseHost, Cfg.Config.DatabaseUser, Cfg.Config.DatabasePassword, Cfg.Config.DatabaseName, Cfg.Config.DatabasePort);
        Db.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `Users` (
                                        `Id` int(11) NOT NULL AUTO_INCREMENT,
                                        `SteamId` varchar(18) NOT NULL,
                                        `ClientName` varchar(128) NOT NULL,
                                        `ConnectedAt` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                                        `IpAddress` varchar(16) DEFAULT NULL,
                                        PRIMARY KEY (`Id`),
                                        UNIQUE KEY `SteamId` (`SteamId`)
                                    );");

        RegisterListener<Listeners.OnClientConnect>(Listener_OnClientConnectHandler);
        RegisterListener<Listeners.OnClientDisconnect>(Listener_OnClientDisconnectHandler);
    }

    private void Listener_OnClientConnectHandler(int playerSlot, string name, string ipAddress)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (!IsValid.Client(playerSlot + 1) || player.IsBot)
        {
            return;
        }

        ipAddress = ipAddress.Split(':')[0];

        if (Cfg.Config.StoreInDatabase)
        {
            Queries.InsertNewClient(Db, player, ipAddress);
        }

        if (Cfg.Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(Cfg.Config.DiscordWebhook, true, player, ipAddress);
        }
    }


    public void Listener_OnClientDisconnectHandler(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (!IsValid.Client(playerSlot + 1))
        {
            return;
        }

        if (Cfg.Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(Cfg.Config.DiscordWebhook, false, player);
        }
    }


    [ConsoleCommand("connectedplayers", "get every connected player")]
    public void ConnectedPlayers(CCSPlayerController player, CommandInfo info)
    {
        if (!Cfg.Config.StoreInDatabase)
        {
            return;
        }

        if (!IsValidCommandUsage(player, info.ArgCount))
        {
            return;
        }

        List<User> users = Queries.GetConnectedPlayers(Db);

        if (users.Count == 0)
        {
            player.PrintToChat($"{Cfg.Config.ChatPrefix} No connected players");
            return;
        }

        bool ValidPlayer = player != null;

        foreach (User p in users)
        {
            if (!ValidPlayer)
            {
                Server.PrintToConsole($"{p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
                continue;
            }

            player.PrintToChat($"{Cfg.Config.ChatPrefix} {p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
        }
    }

    private bool IsValidCommandUsage(CCSPlayerController player, int args)
    {
        if (args != 1)
        {
            if (player != null)
            {
                player.PrintToChat($"{Cfg.Config.ChatPrefix} Usage: !connectedplayers");
            }
            else
            {
                Server.PrintToConsole($"{Cfg.Config.ChatPrefix} Usage: !connectedplayers");
            }
            
            return false;
        }
        return true;
    }
}
