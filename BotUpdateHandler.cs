using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace bot1;

public class BotUpdateHandler(ILogger<BotUpdateHandler> logger, BotInlineButton inlineButton) : IUpdateHandler
{
    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error: {message}", exception.Message);
        return Task.CompletedTask;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        logger.LogInformation("new update: {updateType}", update.Type);
        await inlineButton.SendInlineButtonAsync(botClient, update, cancellationToken);
    }
}
