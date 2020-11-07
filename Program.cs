using System;
using System.Collections.Generic;
using System.IO;
using GameBot.BotVoid;
using GameBot.ConfigModel;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace GameBot
{
    internal class Program
    {
        /// <summary>
        ///     Объект, через который выполняются все запросы (экземпляр бота)
        /// </summary>
        public static TelegramBotClient Bot { get; set; }

        private static void Main()
        {
            Initialization();
            var me = Bot.GetMeAsync().Result.FirstName;
            Console.WriteLine(me);
            Bot.OnMessage       += new Program().ReadMessage;
            Bot.OnCallbackQuery += new Program().ReadButton;
            Bot.StartReceiving();
            Console.ReadKey();
            Bot.StopReceiving();
        }

        /// <summary>
        ///     Обработка событий - новых CallbackQuery
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Информация по запросу пользователя</param>
        private void ReadButton(object sender, CallbackQueryEventArgs e)
        {
            try
            {
                var @event = e.CallbackQuery;
                Bot.AnswerCallbackQueryAsync(@event.Id);

                var isConf = @event.From.Id != @event.Message.Chat.Id;

                if (isConf) new ConfVoid().PerformEvent(@event);
                else new PersonalVoid().PerfomEvent(@event);
            }
            catch
            {
                Console.WriteLine("TO DO : fix (Program line 26) try ");
            }
        }

        /// <summary>
        ///     Обработка сообщений - новых Messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Информация по сообщению пользователя</param>
        private void ReadMessage(object sender, MessageEventArgs e)
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
        private static void Initialization()
        {
            for (var i = 0; i < GameBotOnline.Turn.Length; i++)
                GameBotOnline.Turn[i] = new List<ObjectGame>();
            for (var i = 0; i < PersonalVoid.TurnWaitPlayer.Length; i++)
                PersonalVoid.TurnWaitPlayer[i] = new List<ObjectGame>();
            //

            var json                = File.ReadAllText(@"token.json");
            var botInfo = JsonConvert.DeserializeObject<BotInfo>(json);
            Bot = new TelegramBotClient(botInfo.Token);
        }

        private class BotInfo
        {
             public  string Token = null;
        }
    }
}