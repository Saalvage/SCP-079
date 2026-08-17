using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SCP_079;

var configRoot = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton(configRoot)
    .AddOptions()
    .Configure<Config>(configRoot.GetSection("SCP-079"),
        binder => binder.BindNonPublicProperties = true)
    .BuildServiceProvider();

var config = services.GetRequiredService<IOptions<Config>>().Value;

var client = new DiscordClient(new() {
    Token = config.BotToken,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContent,
    ServiceProvider = services,
});

var spamManager = new SpamManager(client, config);


await client.ConnectAsync();
// TODO: Emoji doesn't seem to work.
await client.UpdateStatusAsync(new("", ActivityType.Custom) { Name = "PARSING", Emoji = new("💾") });

await spamManager.StartCleanup();
