using System.Collections.Concurrent;
using System.Diagnostics;
using DisCatSharp.Entities;

namespace SCP_079;

public class MessageInformation {
    private bool _isSpam;
    private readonly ConcurrentBag<DiscordMessage> _messages = [];
    private long _timestamp;

    public bool ShouldDelete(Config config) =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Interlocked.Read(ref _timestamp)
        >= config.SpamMaxTimeSeconds * 1000;

    public async Task AddMessage(Config config, DiscordMessage message) {
        Debug.Assert(message.Guild is not null);

        var prev = Interlocked.Read(ref _timestamp);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (now - prev >= config.SpamMaxTimeSeconds * 1000) {
            _messages.Clear();
        }

        while (now > prev) {
            prev = Interlocked.CompareExchange(ref _timestamp, now, prev);
        }

        if (_isSpam) {
            await message.DeleteAsync();
            return;
        }

        if (_messages.Count + 1 < config.SpamMinMessages) {
            _messages.Add(message);
            return;
        }

        if (Interlocked.CompareExchange(ref _isSpam, true, false)) {
            await message.DeleteAsync();
            return;
        }

        var timeout = TimeSpan.FromMinutes(config.SpamTimeoutMinutes);
        
        var settings = Data.TryGetServerSettings(message.Guild.Id);
        
        var author = message.Author;
        var timedOut = false;
        try {
            await Task.WhenAll(_messages.Append(message).Select(x => x.DeleteAsync())
                .Append(author.ConvertToMember(message.Channel.Guild!)
                    .ContinueWith(async x => {
                        await x.Result.TimeoutAsync(timeout);
                        // Only set when the timeout didn't fail.
                        timedOut = true;
                    }))
                .Append(author.SendMessageAsync(
                    $"You have been timed out in {message.Guild.Name} for spam."
                    + (!string.IsNullOrEmpty(settings?.AppealRecipient)
                        ? $" If you believe this has been a mistake please contact `{Volatile.Read(ref settings.AppealRecipient)}`."
                        : "")))
            );
        } catch { /* Messages already deleted, or not authorized to time out this user. */ }

        _messages.Clear();
        if (settings == null) { return; }
        var channelId = Volatile.Read(ref settings.LogChannelId);
        if (channelId != 0) {
            var msg = true ? $"Timed out user {author.Mention} until <t:{(DateTimeOffset.UtcNow + timeout).ToUnixTimeMilliseconds() / 1000}:R> for triggering the spam protection."
                : $"Failed to time out user {author.Mention} for triggering the spam protection.";
            msg += message.Content.Length > 0
                ? $" The sent message was:\n```\n{message.Content.Replace('`', '´')}\n```"
                : " The sent message was empty ";
            if (message.Attachments.Count > 0) {
                msg += "with the following attachments:\n" + string.Join("\n", message.Attachments.Select(x => x.Url));
            }
            await message.Channel.Guild.GetChannel(channelId)
                .SendMessageAsync(msg);
        }
    }
}
