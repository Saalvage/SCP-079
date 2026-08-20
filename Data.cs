using System.Collections.Concurrent;
using System.Text.Json;

namespace SCP_079;

public record Data(ConcurrentDictionary<ulong, ServerSettings> ServerSettings) {
    private static readonly string Path = "data.json";
    private static readonly JsonSerializerOptions JsonOptions = new() {
        IncludeFields = true,
    };
    private static readonly Data Instance = LoadOrCreate();
    
    private static Data LoadOrCreate() {
        if (File.Exists(Path)) {
            using var stream = File.OpenRead(Path);
            return JsonSerializer.Deserialize<Data>(stream, JsonOptions) ?? new([]);
        }
        return new([]);
    }

    public static async Task Save() {
        await using var stream = File.Create(Path);
        await JsonSerializer.SerializeAsync(stream, Instance, JsonOptions);
    }

    public static ServerSettings GetOrAddServerSettings(ulong guildId) => Instance.ServerSettings.GetOrAdd(guildId, _ => new());
    public static ServerSettings? TryGetServerSettings(ulong guildId) => Instance.ServerSettings.GetValueOrDefault(guildId);
}
