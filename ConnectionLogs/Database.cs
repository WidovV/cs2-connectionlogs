using ConnectionLogs;
using MySqlConnector;

namespace ConnectedPlayers;
/*
CREATE TABLE Users (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    SteamId VARCHAR(18) UNIQUE NOT NULL,
    ClientName VARCHAR(128) NOT NULL,
    ConnectedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

*/
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
        Server = CFG.config.DatabaseHost,
        Port = CFG.config.DatabasePort,
        UserID = CFG.config.DatabaseUser,
        Password = CFG.config.DatabasePassword,
        Database = CFG.config.DatabaseName
    };

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connection.ConnectionString);
    }
}