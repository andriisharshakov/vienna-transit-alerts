using CodeHollow.FeedReader;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

class Program
{
    private static IConfiguration _configuration;

    static async Task Main(string[] args)
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory))
            .AddJsonFile("appsettings.json")
            .Build();

        var wienerLinienSection = _configuration.GetSection(key: "appSettings").GetSection(key: "wienerLinien");
        var feedUrl = wienerLinienSection.GetSection(key: "feedUrl").Value;

        if (string.IsNullOrWhiteSpace(feedUrl))
        {
            return;
        }

        var telegramSection = _configuration.GetSection(key: "appSettings").GetSection(key: "telegram");
        var telegramBotToken = telegramSection.GetSection(key: "botToken").Value;
        var telegramChannelUsername = telegramSection.GetSection(key: "channelUsername").Value;

        if (string.IsNullOrWhiteSpace(telegramBotToken))
        {
            return;
        }
        
        if (string.IsNullOrWhiteSpace(telegramChannelUsername))
        {
            return;
        }

        var botClient = new TelegramBotClient(telegramBotToken);
        while (true)
        {
            var feed = await FeedReader.ReadAsync(feedUrl);
            var ubahnDisruptions = feed.Items.Where(item => item.Title.Contains("U-Bahn"));

            foreach (var disruption in ubahnDisruptions)
            {
                var message = $"🚇 *U-Bahn Disruption*\n\n*{disruption.Title}*\n\n{disruption.Description}";

                Console.WriteLine(message);
                await botClient.SendTextMessageAsync(telegramChannelUsername, message, (int) ParseMode.Markdown);
            }

            await Task.Delay(TimeSpan.FromMinutes(5)); // Check every 5 minutes
        }
    }
}
