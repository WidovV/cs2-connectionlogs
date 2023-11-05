using ConnectionLogs;
using MySqlConnector;
using System.Data;

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
        Server = Cfg.Config.DatabaseHost,
        Port = Cfg.Config.DatabasePort,
        UserID = Cfg.Config.DatabaseUser,
        Password = Cfg.Config.DatabasePassword,
        Database = Cfg.Config.DatabaseName
    };

    public static MySqlConnection? globalConnection;

    public static MySqlConnection GetConnection()
    {
        if (globalConnection == null || globalConnection.State == ConnectionState.Closed)
        {
            globalConnection = new MySqlConnection(connection.ConnectionString);
            globalConnection.Open();
        }

        return globalConnection;
    }
}