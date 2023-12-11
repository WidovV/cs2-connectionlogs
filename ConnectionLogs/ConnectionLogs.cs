using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using Nexd.MySQL;

namespace ConnectionLogs;

public class ConnectionLogs : BasePlugin, IPluginConfig<StandardConfig>
{
    public override string ModuleName => "Connection logs";

    public override string ModuleVersion => "0.3";

    public override string ModuleAuthor => "WidovV";
    public override string ModuleDescription => "Logs client connections to a database and discord.";

    private MySqlDb? _db;
    private string? _serverName;

    public required StandardConfig Config { get; set; }

    public override void Load(bool hotReload)
    {
        _db = new(Config.DatabaseHost ?? string.Empty, Config.DatabaseUser ?? string.Empty, Config.DatabasePassword ?? string.Empty, Config.DatabaseName ?? string.Empty, Config.DatabasePort);
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
    }

    private void Listener_OnMapStartHandler(string mapName) => _serverName = ConVar.Find(("hostname")).StringValue ?? "Server";

    private void Listener_OnClientPutInServerHandler(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player.IsBot || !IsValid.Client(playerSlot + 1))
        {
            return;
        }

        if (Config.StoreInDatabase)
        {
            Queries.InsertNewClient(_db, player, player.IpAddress?.Split(':')[0] ?? string.Empty);
        }

        if (Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(Config, true, player, _serverName);
        }
    }


    public void Listener_OnClientDisconnectHandler(int playerSlot)
    {
        CCSPlayerController player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player.IsBot || !IsValid.Client(playerSlot + 1))
        {
            return;
        }

        if (Config.SendMessageToDiscord)
        {
            new DiscordClass().SendMessage(Config, false, player, _serverName);
        }
    }

    
    [ConsoleCommand("css_connectedplayers", "get every connected player")]
    [CommandHelper(usage: "css_connectedplayers", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissionsOr("@css/changemap", "@css/rcon")]
    public void ConnectedPlayers(CCSPlayerController player, CommandInfo info)
    {
        if (!Config.StoreInDatabase)
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
            player.PrintToChat($"{Config.ChatPrefix} No connected players");
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

            player?.PrintToChat($"{Config.ChatPrefix} {p.ClientName} ({p.SteamId}) last joined: {p.ConnectedAt}");
        }
    }

    public void OnConfigParsed(StandardConfig standardConfig)
    {
        foreach (PropertyInfo property in typeof(StandardConfig).GetProperties())
        {
            if (property.GetValue(standardConfig) == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[{DateTime.Now}] {property.Name} is null, please fix this in the standardConfig file.");
                Console.ResetColor();
                continue;
            }

            // Check if the property is a string
            if (property.PropertyType == typeof(string))
            {
                // Check if the property is empty
                if (string.IsNullOrEmpty(property.GetValue(standardConfig).ToString()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] {property.Name} is empty, please fix this in the standardConfig file.");
                    Console.ResetColor();
                    continue;
                }
                
                property.SetValue(standardConfig, Cfg.ModifyColorValue(property.GetValue(standardConfig).ToString()));
            }
        }

        Config = standardConfig;
    }
}
