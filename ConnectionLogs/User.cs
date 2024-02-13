namespace ConnectionLogs;

internal class User
{
    public int Id { get; set; }
    public ulong SteamId { get; set; }
    public string ClientName { get; set; }
    public DateTime ConnectedAt { get; set; }
    public DateTime LastSeen { get; set; }
}
