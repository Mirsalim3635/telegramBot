using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;

namespace bot1;

public class BotInlineButton
{
    private readonly ILogger<BotInlineButton> _logger;

    public string? Style { get; set; }
    public string? Format { get; set; }
    public string? Background { get; set; }
    public string? Seed { get; set; }
    public string? Color { get; set; }
    public bool IsColorActive { get; set; } = true;

    public BotInlineButton(ILogger<BotInlineButton> logger)
    {
        _logger = logger;
    }

    public async Task SendInlineButtonAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var keyboardEmoji = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("adventurer-neutral", "/adventurer-neutral"), InlineKeyboardButton.WithCallbackData("thumbs", "/thumbs") },
            new[] { InlineKeyboardButton.WithCallbackData("avataaars-neutral", "/avataaars-neutral"), InlineKeyboardButton.WithCallbackData("bottts", "/bottts") },
            new[] { InlineKeyboardButton.WithCallbackData("identicon", "/identicon"), InlineKeyboardButton.WithCallbackData("personas", "/personas") },
            new[] { InlineKeyboardButton.WithCallbackData("adventurer", "/adventurer"), InlineKeyboardButton.WithCallbackData("croodles", "/croodles") },
            new[] { InlineKeyboardButton.WithCallbackData("bottts-neutral", "/bottts-neutral"), InlineKeyboardButton.WithCallbackData("rings", "/rings") }
        });

        var keyboardFormat = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🖼️ PNG", "/png"), InlineKeyboardButton.WithCallbackData("🧾 SVG", "/svg") }
        });

        var keyboardBackground = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("♻️ Transparent", "transparent"), InlineKeyboardButton.WithCallbackData("🎨 Solid", "solid") }
        });

        if (update.Type == UpdateType.Message && update.Message?.Text is not null)
        {
            var messageText = update.Message.Text.Trim();

            if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Request: /start");
                await botClient.SendMessage(
                    chatId: update.Message.Chat.Id,
                    text: "*Hello. Please choose one of styles below 📜:*",
                    replyMarkup: keyboardEmoji,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (!string.IsNullOrEmpty(Style) && !string.IsNullOrEmpty(Format) && !string.IsNullOrEmpty(Background))
            {
                if (!IsColorActive && string.IsNullOrEmpty(Color) && Background == "solid")
                {
                    Color = IsValidColor(messageText);
                    if (string.IsNullOrEmpty(Color))
                    {
                        await botClient.SendMessage(
                            chatId: update.Message.Chat.Id,
                            text: "*❌ Invalid color. Please try again.*",
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                        return;
                    }
                    IsColorActive = true;

                    await botClient.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: "*Please enter seed :*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                if (string.IsNullOrEmpty(Seed))
                {
                    Seed = messageText;
                    _logger.LogInformation("Seed received: {Seed}", Seed);
                    await SendAvatarAsync(botClient, update, cancellationToken);
                }
            }
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data is not null)
        {
            var request = update.CallbackQuery.Data;

            try
            {
                await botClient.EditMessageReplyMarkup(
                    chatId: update.CallbackQuery.Message!.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    replyMarkup: null,
                    cancellationToken: cancellationToken
                );

                await botClient.DeleteMessage(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Callback message edit/delete failed");
            }

            var styles = new[]
            {
                "/adventurer-neutral", "/thumbs", "/avataaars-neutral", "/bottts",
                "/identicon", "/personas", "/adventurer", "/croodles",
                "/bottts-neutral", "/rings"
            };

            if (styles.Contains(request))
            {
                Style = request;
                _logger.LogInformation("Chosen style: {style}", request);

                await botClient.SendMessage(
                    chatId: update.CallbackQuery.Message!.Chat.Id,
                    text: "*Which format do you choose 🤔:*",
                    replyMarkup: keyboardFormat,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (request is "/png" or "/svg")
            {
                Format = request;
                _logger.LogInformation("Chosen format: {format}", request);

                await botClient.SendMessage(
                    chatId: update.CallbackQuery.Message!.Chat.Id,
                    text: "*Which background type do you choose 🤔:*",
                    replyMarkup: keyboardBackground,
                    parseMode: ParseMode.Markdown,
                    cancellationToken: cancellationToken
                );
            }
            else if (request is "transparent" or "solid")
            {
                Background = request;
                _logger.LogInformation("Background: {background}", request);

                if (Background == "solid")
                {
                    IsColorActive = false;
                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Please enter color :*\n *e.g: red *",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
                else if (Background == "transparent")
                {
                    Color = null; // rangni tozalaydi
                    await botClient.SendMessage(
                        chatId: update.CallbackQuery.Message!.Chat.Id,
                        text: "*Please enter seed :*",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
            }

            await botClient.AnswerCallbackQuery(
                callbackQueryId: update.CallbackQuery.Id,
                text: "✅ Selected!",
                showAlert: false,
                cancellationToken: cancellationToken
            );
        }
    }

    private string IsValidColor(string color)
    {
        var colorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "white", "#FFFFFF" },
            { "black", "#000000" },
            { "red", "#FF0000" },
            { "green", "#00FF00" },
            { "blue", "#0000FF" },
            { "yellow", "#FFFF00" },
            { "cyan", "#00FFFF" },
            { "magenta", "#FF00FF" },
            { "gray", "#808080" },
            { "lightgray", "#D3D3D3" },
            { "darkgray", "#A9A9A9" },
            { "orange", "#FFA500" },
            { "purple", "#800080" },
            { "brown", "#A52A2A" },
            { "pink", "#FFC0CB" },
            { "lime", "#32CD32" },
            { "navy", "#000080" },
            { "teal", "#008080" },
            { "olive", "#808000" },
            { "maroon", "#800000" }
        };

        if (colorMap.TryGetValue(color, out var mapped))
            return mapped;

        if (Regex.IsMatch(color, @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$"))
            return color;

        return string.Empty;
    }

    private async Task SendAvatarAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        string baseUrl = $"https://api.dicebear.com/9.x/{Style.TrimStart('/')}/{Format.Replace("/", "")}?seed={Seed}";

        if (Background == "solid" && !string.IsNullOrEmpty(Color))
        {
            baseUrl += $"&backgroundColor={Color.Replace("#", "")}";
        }

        _logger.LogInformation("Dicebear final url: {Url}", baseUrl);

        if (Format == "/svg")
        {
            using var httpClient = new HttpClient();
            var svgBytes = await httpClient.GetByteArrayAsync(baseUrl, cancellationToken);
            using var stream = new MemoryStream(svgBytes);

            await botClient.SendDocument(
                chatId: update.Message!.Chat.Id,
                document: new InputFileStream(stream, "avatar.svg"),
                caption: $"🎨 *Style*: `{Style}`\n📄 *Format*: `{Format}`\n🌱 *Seed*: `{Seed}`",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }
        else if (Format == "/png")
        {
            await botClient.SendPhoto(
                chatId: update.Message!.Chat.Id,
                photo: baseUrl,
                caption: $"🎨 *Style*: `{Style}`\n📄 *Format*: `{Format}`\n🎉 *Seed*: `{Seed}`",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken
            );
        }

        Style = Format = Background = Seed = Color = null;
        IsColorActive = true;

        await botClient.SendMessage(
            chatId: update.Message!.Chat.Id,
            text: "*🎉 If you want to reuse, select button :*\n- /start",
            parseMode: ParseMode.Markdown,
            cancellationToken: cancellationToken
        );
    }
}
