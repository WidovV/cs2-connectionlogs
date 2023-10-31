using CounterStrikeSharp.API.Core;

namespace ConnectionLogs;

internal class DiscordClass
{
    /// <summary>
    /// Generates a string containing information about a player's connection status and timestamp.
    /// </summary>
    /// <param name="connectType">A boolean indicating whether the player has connected or disconnected.</param>
    /// <param name="player">The player whose connection status is being logged.</param>
    /// <returns>A string containing the player's name, Steam ID, connection status, and timestamp.</returns>
    private string DiscordContent(bool connectType, CCSPlayerController player)
    {
        string connectTypeString = connectType ? "connected" : "disconnected";
        // Get the current timestamp as a unix timestamp
        return $"<t:{DateTimeOffset.Now.ToUnixTimeSeconds()}:T> {player.PlayerName} ({player.SteamID}) {connectTypeString}";
    }

    /// <summary>
    /// Sends a message to a Discord webhook with information about a player's connection status.
    /// </summary>
    /// <param name="webhook">The Discord webhook URL to send the message to.</param>
    /// <param name="connectType">A boolean indicating whether the player is connecting or disconnecting.</param>
    /// <param name="player">The CCSPlayerController object representing the player.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SendMessage(string webhook, bool connectType, CCSPlayerController player)
    {
        try
        {
            string msg = DiscordContent(connectType, player);
            using var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("content", msg)
            });

            // Discard
            await client.PostAsync(webhook, content);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Failed to send message to discord.\n{ex.Message}");
            Console.ResetColor();
        }
    }
}