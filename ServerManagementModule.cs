using System.Diagnostics;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

namespace SCP_079;

public class ServerManagementModule : ApplicationCommandsModule {
    [SlashCommand("set-log-channel", "Sets the log channel for the server.",
        allowedContexts: [InteractionContextType.Guild], defaultMemberPermissions: (long)Permissions.ModerateMembers)]
    public async Task SetLogChannel(InteractionContext context,
        [Option("channel", "The text channel in which moderation actions by the bot will be logged"),
         ChannelTypes(ChannelType.Text)] DiscordChannel channel) {
        Debug.Assert(context.Guild is not null);

        var settings = Data.GetOrAddServerSettings(context.Guild.Id);
        settings.LogChannelId = channel.Id;
        await Data.Save();
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {
            Content = $"Successfully set log channel to {channel.Mention}.",
        });
    }
    
    [SlashCommand("set-appeal-recipient", "Sets the log channel for the server.",
        allowedContexts: [InteractionContextType.Guild], defaultMemberPermissions: (long)Permissions.ModerateMembers)]
    public async Task SetLogChannel(InteractionContext context,
        [Option("recipient", "Discord username of the recipient for appeals for moderation actions")]
        string recipientName) {
        Debug.Assert(context.Guild is not null);

        var settings = Data.GetOrAddServerSettings(context.Guild.Id);
        settings.AppealRecipient = recipientName;
        await Data.Save();
        
        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {
            Content = $"Successfully set appeal recipient to `{recipientName}`.",
        });
    }
}
