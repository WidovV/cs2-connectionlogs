using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using System.Text.Json;


namespace ConnectionLogs;

internal class CFG
{
    public static Config config = new();

    /// <summary>
    /// Checks the configuration file for the module and creates it if it does not exist.
    /// </summary>
    /// <param name="moduleDirectory">The directory where the module is located.</param>
    public void CheckConfig(string moduleDirectory)
    {
        string path = Path.Join(moduleDirectory, "config.json");

        if (!File.Exists(path))
        {
            CreateAndWriteFile(path);
        }

        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs))
        {
            // Deserialize the JSON from the file and load the configuration.
            config = JsonSerializer.Deserialize<Config>(sr.ReadToEnd());
        }
        config.ChatPrefix = ModifiedChatPrefix(config.ChatPrefix);
    }

    /// <summary>
    /// Creates a new file at the specified path and writes the default configuration settings to it.
    /// </summary>
    /// <param name="path">The path where the file should be created.</param>
    private static void CreateAndWriteFile(string path)
    {

        using (FileStream fs = File.Create(path))
        {
            // File is created, and fs will automatically be disposed when the using block exits.
        }

        Console.WriteLine($"File created: {File.Exists(path)}");

        config = new Config
        {
            ChatPrefix = "[ConnectionLogs]",
            SendMessageToDiscord = false,
            DiscordWebhook = "https://discord.com/api/webhooks/",
            StoreInDatabase = false,
            DatabaseHost = "localhost",
            DatabasePort = 3306,
            DatabaseUser = "root",
            DatabasePassword = "password",
            DatabaseName = "database"
        };

        // Serialize the config object to JSON and write it to the file.
        string jsonConfig = JsonSerializer.Serialize(config, new JsonSerializerOptions()
        {
            WriteIndented = true
        });
        File.WriteAllText(path, jsonConfig);
    }

    // Essential method for replacing chat colors from the config file, the method can be used for other things as well.
    private string ModifiedChatPrefix(string msg)
    {
        if (msg.Contains("{"))
        {
            string modifiedValue = msg;
            foreach (FieldInfo field in typeof(ChatColors).GetFields())
            {
                string pattern = $"{{{field.Name}}}";
                if (msg.Contains(pattern))
                {
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null).ToString());
                }
            }
            return modifiedValue;
        }

        return string.IsNullOrEmpty(msg) ? "[ConnectionLog]" : msg;
    }
}

internal class Config
{
    public string? ChatPrefix { get; set; }
    public bool SendMessageToDiscord { get; set; }
    public string? DiscordWebhook { get; set; }
    public bool StoreInDatabase { get; set; }
    public string? DatabaseHost { get; set; }
    public uint DatabasePort { get; set; }
    public string? DatabaseUser { get; set; }
    public string? DatabasePassword { get; set; }
    public string? DatabaseName { get; set; }
}
