using System;
using System.Collections.Generic;
using GameBot.BotVoid;
using GameBot.ConfigModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace GameBot
{
    public class Program
    {
        public static TelegramBotClient Bot { get; private set; }

        private static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .Build();
            
            Initialization(configuration);
            
            var me = Bot.GetMeAsync().Result.FirstName;
            Console.WriteLine(me);
            Bot.OnMessage       += ReadMessage;
            Bot.OnCallbackQuery += ReadButton;
            Bot.StartReceiving();
            Console.ReadKey();
            Bot.StopReceiving();
        }

        /// <summary>
        ///     Обработка событий - новых CallbackQuery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Информация по запросу пользователя</param>
        private static void ReadButton(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var @event = e.CallbackQuery;
                Bot.AnswerCallbackQueryAsync(@event.Id);

                var isConf = @event.From.Id != @event.Message.Chat.Id;

                if (isConf) new ConfVoid().PerformEvent(@event);
                else new PersonalVoid().PerformEvent(@event);
            }
            catch
            {
                Console.WriteLine("fix (Program line 26) try ");
            }
        }

        /// <summary>
        ///     Обработка сообщений - новых Messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Информация по сообщению пользователя</param>
        private static void ReadMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            Console.WriteLine($"{message.From.Username} {message.From.FirstName} {message.Text}");
            var isConf = message.From.Id != message.Chat.Id;
            if (message.Text != null)
                if (isConf) new ConfVoid().PerformMessage(message);
                else new PersonalVoid().PerformMessage(message);
        }

        /// <summary>
        ///     Инициализация объектов программы
        /// </summary>
        private static void Initialization(IConfiguration configuration)
        {
            for (var i = 0; i < GameBotOnline.Turn.Length; i++)
                GameBotOnline.Turn[i] = new List<ObjectGame>();
            for (var i = 0; i < PersonalVoid.TurnWaitPlayer.Length; i++)
                PersonalVoid.TurnWaitPlayer[i] = new List<ObjectGame>();
            //

            var token = configuration["TelegramToken"];
            var botInfo = JsonConvert.DeserializeObject<BotInfo>(token);
            Bot = new TelegramBotClient(botInfo.Token);
        }

    }
}