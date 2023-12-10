using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Net;
using System.Text;

namespace ConnectionLogs;

internal class DiscordClass
{
    /// <summary>
    /// Generates a string containing information about a player's connection status and timestamp.
    /// </summary>
    /// <param name="connectType">A boolean indicating whether the player has connected or disconnected.</param>
    /// <param name="player">The player whose connection status is being logged.</param>
    /// <returns>A string containing the player's name, Steam ID, connection status, and timestamp.</returns>
    private string DiscordContent(bool connectType, CCSPlayerController player, string serverName)
    {
        string connectTypeString = connectType ? "connected to" : "disconnected from";

        StringBuilder messageBuilder = new();
        messageBuilder.Append($"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:T> [{player.PlayerName}](<https://steamcommunity.com/profiles/{player.SteamID}>)(`{player.SteamID}`)");

        if (!Cfg.Config.PrintIpToDiscord)
        {
            messageBuilder.Append($" {connectTypeString} {serverName}");
            return messageBuilder.ToString();
        }

        messageBuilder.Append($" [{player.IpAddress.Split(':')[0]}](<https://geoiplookup.net/ip/{player.IpAddress.Split(':')[0]}>)");
        messageBuilder.Append($" {connectTypeString} {serverName}");

        return messageBuilder.ToString();
    }


    /// <summary>
    /// Sends a message to a Discord webhook with information about a player's connection status.
    /// </summary>
    /// <param name="webhook">The Discord webhook URL to send the message to.</param>
    /// <param name="connectType">A boolean indicating whether the player is connecting or disconnecting.</param>
    /// <param name="player">The CCSPlayerController object representing the player.</param>
    public void SendMessage(bool connectType, CCSPlayerController player, string serverName)
    {
        try
        {
            string msg = DiscordContent(connectType, player, serverName);
            Task.Run(() =>
            {
                using HttpClient? client = new();
                using StringContent? content = new($"{{\"content\":\"{msg}\"}}", Encoding.UTF8, "application/json");
                using HttpResponseMessage resp = client.PostAsync(Cfg.Config.DiscordWebhook, content).Result;

                if (!resp.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[{DateTime.Now}] Failed to send message to Discord: {resp.StatusCode}");
                    Console.ResetColor();
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
