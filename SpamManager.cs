using System.Collections.Concurrent;
using DisCatSharp;
using DisCatSharp.Entities;

namespace SCP_079;

public class SpamManager {
    private readonly ConcurrentDictionary<int, MessageInformation> _dic = [];

    private readonly Config _config;

    public SpamManager(DiscordClient client, Config config) {
        _config = config;
        client.MessageCreated += async (_, msg) => {
            if (msg.Guild is null) { return; }
    
            await _dic.GetOrAdd(HashMessage(msg.Message), _ => new())
                .AddMessage(_config, msg.Message);
    
            int HashMessage(DiscordMessage message) {
                var hash = new HashCode();
                hash.Add(message.Author.Id);
                hash.Add(message.Content);
                foreach (var attachment in message.Attachments) {
                    hash.Add(attachment.Filename);
                }
                return hash.ToHashCode();
            }
        };
    }
    
    public async Task StartCleanup() {
        while (true) {
            foreach (var (hash, msg) in _dic) {
                if (msg.ShouldDelete(_config)) {
                    _dic.Remove(hash, out _);
                }
            }

            await Task.Delay(1000);
        }
    }
}
