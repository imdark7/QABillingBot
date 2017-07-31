using System;
using System.Diagnostics;
using System.Net.Http;
using System.ServiceProcess;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace QABillingBot
{
    public partial class QaBillingBotService : ServiceBase
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("338156685:AAHEQ5vvvzxisVggLM9U6WdmqXteP9n-s1g");
        private const long QaBillingChatId = -1001118390469;
        private const long SupportChatId = 293233922;

        public QaBillingBotService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.StartReceiving();
        }

        protected override void OnStop()
        {
            Bot.StopReceiving();
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage || message.Text == "/start" || message.Chat.Id == QaBillingChatId) return;

            var gif = new FileToSend(GetRandomGifUri());

            await Bot.SendTextMessageAsync(message.Chat.Id, @"Дорогой друг! Твое сообщение передано группе тестирования Биллинга, скоро с тобой свяжутся и ответят.");
            await Bot.SendVideoAsync(SupportChatId, gif);

            await Bot.SendTextMessageAsync(QaBillingChatId, $@"{message.From.FirstName} {message.From.LastName} пишет нам:");
            var forvardedMessage = await Bot.ForwardMessageAsync(QaBillingChatId, message.Chat.Id, message.MessageId);
            await Bot.PinChatMessageAsync(QaBillingChatId, forvardedMessage.MessageId);
            await Bot.SendVideoAsync(QaBillingChatId, gif);
        }

        private static Uri GetRandomGifUri(string tag = "cat")
        {
            var result = new HttpClient()
                .GetAsync($"http://api.giphy.com/v1/gifs/random?api_key=21debe9a0b16487a8d4da781583b56a8&tag={tag}")
                .Result
                .Content
                .ReadAsStringAsync()
                .Result;
            return new Uri(JsonConvert.DeserializeObject<GiphyDotNet.Model.Results.GiphyRandomResult>(result).Data.ImageMp4Url);
        }
    }
}
