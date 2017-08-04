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
    static class Program
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new QaBillingBot()
            };
            ServiceBase.Run(servicesToRun);
        }
        
    }
}
