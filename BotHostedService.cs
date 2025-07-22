using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace bot1;

public class BotHostedService(ILogger<BotHostedService> logger,
ITelegramBotClient botClient,
IUpdateHandler updateHandler) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var me = await botClient.GetMe(cancellationToken);
        logger.LogInformation("{bot} started.", me.FirstName ?? me.Username);
        botClient.StartReceiving(
            updateHandler,
            new ReceiverOptions
            {
                DropPendingUpdates = true,
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery } // ✅ MUHIM
            },
            cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{service} exiting ...", nameof(BotHostedService));
        return Task.CompletedTask;
    }
}
