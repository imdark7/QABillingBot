using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace QABillingBot
{
    public partial class QaBillingBot : ServiceBase
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("338156685:AAHEQ5vvvzxisVggLM9U6WdmqXteP9n-s1g");
        private const long QaBillingChatId = -1001118390469;
        private const long SupportChatId = -1001126960536;

        public QaBillingBot()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.StartReceiving();
            Bot.SendTextMessageAsync(QaBillingChatId, "Доброго утречка, котики! :3");
        }

        protected override void OnStop()
        {
            Bot.StopReceiving();
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Text == "/start" || message.Chat.Id == QaBillingChatId) return;
            if (message.Text == "/ping")
            {
                Bot.SendTextMessageAsync(message.Chat.Id, GetRandomPingResponse());
                return;
            }

            var gif = new FileToSend(GetRandomGifUri());

            var chatId = QaBillingChatId;
            var cancellationToken = new CancellationTokenSource(3000).Token;
            var successSend = false;
            try
            {
                Bot.ForwardMessageAsync(chatId, message.Chat.Id, message.MessageId, cancellationToken: cancellationToken)
                    .GetAwaiter()
                    .GetResult();
                successSend = true;
                Bot.SendVideoAsync(chatId, gif, cancellationToken: cancellationToken).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                using (StreamWriter w = File.AppendText("log.txt"))
                {
                    Log(message, w);
                }
            }

            if (successSend)
            {
                Bot.SendTextMessageAsync(message.Chat.Id,
                    @"Дорогой друг! Твое сообщение передано группе тестирования Биллинга, скоро с тобой свяжутся и ответят.",
                    cancellationToken: cancellationToken).GetAwaiter().GetResult();
            }
            else
            {
                Bot.SendTextMessageAsync(message.Chat.Id,
                    @"Дорогой друг! К сожалению, возникли сложности с пересылкой сообщения, попробуй написать кому-нибудь из тестировщиков Биллинга в личку",
                    cancellationToken: cancellationToken).GetAwaiter().GetResult();
            }
            Bot.SendVideoAsync(message.Chat.Id, gif, cancellationToken: cancellationToken).GetAwaiter().GetResult();
        }

        private static void Log(Message message, TextWriter w)
        {
            w.WriteLine($"Username: {message.Chat.Username} ({message.Chat.LastName} {message.Chat.FirstName})");
            w.WriteLine("Chat ID: " + message.Chat.Id);
            w.WriteLine("Chat Type: " + message.Chat.Type + " (0-private, 1-group, 2-channel, 3-supergroup)");
            w.WriteLine(message.Date.ToShortDateString() + " " + message.Date.ToShortTimeString());
            w.WriteLine("Text: " + message.Text);
            w.WriteLine("------------------");
        }

        private static Uri GetRandomGifUri(string tag = "cat")
        {
            var result = new HttpClient()
                .GetAsync($"http://api.giphy.com/v1/gifs/random?api_key=21debe9a0b16487a8d4da781583b56a8&tag={tag}")
                .GetAwaiter().GetResult()
                .Content
                .ReadAsStringAsync()
                .GetAwaiter().GetResult();
            return new Uri(JsonConvert.DeserializeObject<GiphyDotNet.Model.Results.GiphyRandomResult>(result).Data.ImageMp4Url);
        }

        private static string GetRandomPingResponse()
        {
            var rnd = new Random();
            var list = new[]
            {
                "Сам ты пинг",
                "Qu'est-ce que c'es?",
                "Pong",
                "Отлично! Работаем дальше",
                "Вы великолепны!",
                "Format disc C complete",
                "Вы тронули меня в самое сердце <3",
                "Да не сплю я, не сплю",
                "42",
                "А меня все хорошо, а у Вас?"
            };
            return list[rnd.Next(0, 10)];
        }
    }
}
