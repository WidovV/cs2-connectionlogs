using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using System.Text.Json;
using CounterStrikeSharp.API.Core;


namespace ConnectionLogs;

internal class Cfg
{
    // Essential method for replacing chat colors from the standardConfig file, the method can be used for other things as well.
    public static string ModifyColorValue(string msg)
    {
        if (!msg.Contains('{'))
        {
            return string.IsNullOrEmpty(msg) ? "[ConnectionLogs]" : msg;
        }

        string modifiedValue = msg;

        foreach (FieldInfo field in typeof(ChatColors).GetFields())
        {
            string pattern = $"{{{field.Name}}}";
            if (msg.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null).ToString(), StringComparison.OrdinalIgnoreCase);
            }
        }
        return modifiedValue;
    }
}

public class StandardConfig : BasePluginConfig
{
    public string? ChatPrefix { get; set; } = "[ConnectionLogs]";
    public bool SendMessageToDiscord { get; set; } = false;
    public bool PrintIpToDiscord { get; set; } = true;
    public string? DiscordWebhook { get; set; } = "https://discord.com/api/webhooks/";
    public bool StoreInDatabase { get; set; } = false;
    public string? DatabaseHost { get; set; } = "localhost";
    public int DatabasePort { get; set; } = 3306;
    public string? DatabaseUser { get; set; } = "root";
    public string? DatabasePassword { get; set; } = "password";
    public string? DatabaseName { get; set; } = "database";
}