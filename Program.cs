using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Polling;
using bot1;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddSingleton<BotInlineButton>();
builder.Services.AddSingleton<IUpdateHandler, BotUpdateHandler>();
builder.Services.AddSingleton<ITelegramBotClient, TelegramBotClient>(x =>
    new TelegramBotClient(builder.Configuration["Bot:Token"]
        ?? throw new ArgumentException("Telegram Bot Token is not configured.")));
builder.Services.AddHostedService<BotHostedService>();

var app = builder.Build();
app.Run();
