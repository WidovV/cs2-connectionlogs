using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Nexd.MySQL;

namespace ConnectionLogs;

public class ConnectionLogs : BasePlugin
{
    public override string ModuleName => "Connection logs";

    public override string ModuleVersion => "0.3";

    public override string ModuleAuthor => "WidovV";
    public override string ModuleDescription => "Logs client connections to a database and discord.";

    private MySqlDb? _db;
    private string? _serverName;
    public override void Load(bool hotReload)
    {
        new Cfg().CheckConfig(ModuleDirectory);
        Console.WriteLine(Environment.NewLine + Environment.NewLine);
        _db = new(Cfg.Config.DatabaseHost ?? string.Empty, Cfg.Config.DatabaseUser ?? string.Empty, Cfg.Config.DatabasePassword ?? string.Empty, Cfg.Config.DatabaseName ?? string.Empty, Cfg.Config.DatabasePort);
        _db.ExecuteNonQueryAsync(@"CREATE TABLE IF NOT EXISTS `Users` (
                                        `Id` int(11) NOT NULL AUTO_INCREMENT,
                                        `SteamId` varchar(18) NOT NULL,
                                        `ClientName` varchar(128) NOT NULL,
                                        `ConnectedAt` timestamp NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
                                        `IpAddress` varchar(16) DEFAULT NULL,
                                        PRIMARY KEY (`Id`),
                                        UNIQUE KEY `SteamId` (`SteamId`)
                                    );");

        RegisterListener<Listeners.OnClientDisconnect>(Listener_OnClientDisconnectHandler);
        RegisterListener<Listeners.OnClientPutInServer>(Listener_OnClientPutInServerHandler);
        RegisterListener<Listeners.OnMapStart>(Listener_OnMapStartHandler);

        Console.WriteLine(Environment.NewLine + Environment.NewLine);
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[{DateTime.Now}] Loaded {ModuleName} ({ModuleVersion}) by {ModuleAuthor}\n{ModuleDescription}");
        Console.ResetColor();
    }

    private void Listener_OnMapStartHandler(string mapName) => _serverName = ConVar.Find(("hostname")).StringValue;

    private void Listener_OnClientPutInServerHandler(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player.IsBot || !IsValid.Client(playerSlot + 1))
        {
            return;
        }

        if (Cfg.Config.StoreInDatabase)
        {
            Queries.InsertNewClient(_db, player, player.IpAddress?.Split(':')[0] ?? string.Empty);
        }

        if (Cfg.Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage( true, player, _serverName);
        }
    }


    public void Listener_OnClientDisconnectHandler(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player.IsBot || !IsValid.Client(playerSlot + 1))
        {
            return;
        }

        if (Cfg.Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage( false, player, _serverName);
        }
    }

    
    [ConsoleCommand("css_connectedplayers", "get every connected player")]
    [CommandHelper(usage: "css_connectedplayers", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void ConnectedPlayers(CCSPlayerController player, CommandInfo info)
    {
        if (!Cfg.Config.StoreInDatabase)
        {
            return;
        }

        if (info.ArgCount != 1)
        {
            return;
        }

        List<User> users = Queries.GetConnectedPlayers(_db);

        if (users.Count == 0)
        {
            player.PrintToChat($"{Cfg.Config.ChatPrefix} No connected players");
            return;
        }

        bool validPlayer = player != null;

        foreach (User p in users)
        {
            if (!validPlayer)
            {
                Server.PrintToConsole($"{p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
                continue;
            }

            player?.PrintToChat($"{Cfg.Config.ChatPrefix} {p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
        }
    }
}
