using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
    private string DiscordContent(bool connectType, CCSPlayerController player, string ipAddress)
    {
        string connectTypeString = connectType ? "connected" : "disconnected";

        StringBuilder messageBuilder = new StringBuilder();
        messageBuilder.Append($"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:T> [{player.PlayerName}](<https://steamcommunity.com/profiles/{player.SteamID}>) `{player.SteamID}` {connectTypeString}");

        if (!Cfg.Config.PrintIpToDiscord)
        {
            return messageBuilder.ToString();
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            messageBuilder.Append($" [{ipAddress}](<https://geoiplookup.net/ip/{ipAddress}>)");
        }

        return messageBuilder.ToString();
    }


    /// <summary>
    /// Sends a message to a Discord webhook with information about a player's connection status.
    /// </summary>
    /// <param name="webhook">The Discord webhook URL to send the message to.</param>
    /// <param name="connectType">A boolean indicating whether the player is connecting or disconnecting.</param>
    /// <param name="player">The CCSPlayerController object representing the player.</param>
    public void SendMessage(bool connectType, CCSPlayerController player, string ipAddress = null)
    {
        try
        {
            string msg = DiscordContent(connectType, player, ipAddress);
            Task.Run(() =>
            {
                using (HttpClient? client = new())
                using (StringContent? content = new($"{{\"content\":\"{msg}\"}}", Encoding.UTF8, "application/json"))
                {
                    HttpResponseMessage resp = client.PostAsync(Cfg.Config.DiscordWebhook, content).Result;

                    if (!resp.IsSuccessStatusCode)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[{DateTime.Now}] Failed to send message to Discord: {resp.StatusCode}");
                        Console.ResetColor();
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
