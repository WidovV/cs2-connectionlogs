using ConnectionLogs;
using MySqlConnector;

namespace ConnectedPlayers;

internal class Database
{
    /// <summary>
    /// Represents a class for managing the database connection string.
    /// </summary>
    /// <summary>
    /// Represents a static class that contains the database connection string builder.
    /// </summary>
    private static readonly MySqlConnectionStringBuilder connection = new()
    {
        Server = Cfg.config.DatabaseHost,
        Port = Cfg.config.DatabasePort,
        UserID = Cfg.config.DatabaseUser,
        Password = Cfg.config.DatabasePassword,
        Database = Cfg.config.DatabaseName
    };

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connection.ConnectionString);
    }
}