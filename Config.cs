namespace SCP_079;

public class Config {
    public required string BotToken { get; init; }
    public int SpamMinMessages { get; init; }
    public int SpamMaxTimeSeconds { get; init; }
    public int SpamTimeoutMinutes { get; init; }
    public IReadOnlyDictionary<ulong, ulong> LogChannelsPerServer { get; init; }
}
