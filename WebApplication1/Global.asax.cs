using System;
using System.Diagnostics;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using BillingBot;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WebApplication1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("338156685:AAHEQ5vvvzxisVggLM9U6WdmqXteP9n-s1g");
        private const string QaBillingChatId = "-1001118390469";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            var me = Bot.GetMeAsync().Result;

            Console.Title = me.Username;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }
        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Debugger.Break();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.TextMessage || message.Text == "/start") return;

            var fd = new Random();
            var number = fd.Next(500, 1500);
            var usage = @"Дорогой друг! Твое сообщение передано группе тестирования, скоро с тобой свяжутся и ответят.";
            var gif = new FileToSend(NewGifUri());

            await Bot.SendTextMessageAsync(message.Chat.Id, usage);
            await Bot.SendVideoAsync(message.Chat.Id, gif);

            //await Bot.SendTextMessageAsync(QaBillingChatId, $@"{message.From.FirstName} {message.From.LastName} пишет нам:");
            //var forvardedMessage = await Bot.ForwardMessageAsync(QaBillingChatId, message.Chat.Id, message.MessageId);
            //await Bot.PinChatMessageAsync(QaBillingChatId, forvardedMessage.MessageId);
            //await Bot.SendVideoAsync(QaBillingChatId, gif);


        }

        private static Uri NewGifUri()
        {
            var httpClient = new HttpClient();
            var result = httpClient.GetAsync("http://api.giphy.com/v1/gifs/random?api_key=21debe9a0b16487a8d4da781583b56a8&tag=cat").Result.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<GiphyDotNet.Model.Results.GiphyRandomResult>(result);
            var url = data.Data.ImageMp4Url;
            return new Uri(url);
        }
    }
}
