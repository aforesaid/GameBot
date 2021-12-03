using System.Collections.Generic;
using System.Threading.Tasks;
using GameBot.ConfigModel;
using GameBot.GameModel;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GameBot.BotVoid
{
    /// <summary>
    ///     Реализация функций бота в личных сообщениях
    /// </summary>
    public class PersonalVoid:IDefaultVoid
    {
        public delegate Task VoidGame(Message mes, ObjectGame ask);


        public static string Games =
            "Чтобы выбрать игру - пишите команду , указанную после названия игры.\n\n" +
            "1. Быки и Коровы - быкануть.\n" +
            "2. Крестики , нолики - занулить.\n" + "3. Дурак подкидной - дурак.\n\n" +
            "А для того , чтобы общаться в анонимном чате - " + @"пишите ""общаться"" .";

        public static VoidGame[] GameDo =
        {
            new Anonymchat().SendMessage,
            new BullAndCows().InGame,
            null,
            new Durak().Motion
        };

        /// <summary>
        ///     Информация о пользователях,которые находятся в очереди на игру
        /// </summary>
        public static List<ObjectGame>[] TurnWaitPlayer { get; set; } = new List<ObjectGame>[100];

        /// <summary>
        ///     Обработка сообщений пользователей
        /// </summary>
        /// <param name="message">Сообщение пользователя</param>
        public void PerformMessage(Message message)
        {
            var ask = new GameBotOnline().CheckGame(message.Chat.Id.ToString());

            if (ask != null)
            {
                if (message.Text.ToLower() == "!стоп")
                    GameBotOnline.Quits[ask.NumberGame](message, ask);
                else if (ask.NumberGame != 2)
                    GameDo[ask.NumberGame](message, ask).Wait();
            }
            else if (!CheckingInTurnGamePersonal(message.From.Id.ToString()))
            {
                var a = ":();'!, .?".ToCharArray();
                switch (message.Text.ToLower().Trim(a).Split(' ')[0])
                {
                    case "/start":
                        Start(message.From.Id.ToString());
                        break;
                    case "общаться":
                        new Anonymchat().NewSession(message.Chat.Id.ToString(), message.From.FirstName,
                                                    Anonymchat.Answers, Anonymchat.NumberGame, 0, -1).Wait();
                        break;
                    case "быкануть":
                        BullAndCows.AddUsers(message.From.Id);
                        break;
                    case "выйти":
                        ExitTurn(message.From.Id.ToString(), message.MessageId);
                        break;
                    case "занулить":
                        new CrossZero().NewSession(message.From.Id.ToString(), message.From.FirstName,
                                                   CrossZero.Answers, CrossZero.NumberGame, 0, -1).Wait();
                        break;
                    case "дурак":
                        new Durak().Start(message);
                        break;
                    case "поддержка":
                        Program.Bot.SendTextMessageAsync(message.Chat.Id,
                                                         "Если обнаружите какой-то баг в играх - отпишите мне @palyon\n\n Приятного использования :)",
                                                         replyMarkup :Keyboards.Keyboard);
                        break;
                }
            }
        }

        /// <summary>
        ///     Стартовое сообщение пользователю
        /// </summary>
        /// <param name="userId">UserId пользователя</param>
        private void Start(string userId)
        {
            var start_message =
                "Хей , мы рады тебя видеть! Наш бот имеет обширный функционал , благодаря которому Вы будете чувствовать себя комфортно.\n\n"
                +
                "Вы сможете поиграть в различные игры и пообщаться в анонимном чате. \n\nТакже в дальнейшем бот будет иметь различные приколюхи в беседах. Удачки! ";
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Список доступных игр", "Games")
            });
            Program.Bot.SendTextMessageAsync(userId, start_message, replyMarkup :keyboard);
        }

        /// <summary>
        ///     Обработка CallbackQuery запросов пользователей
        /// </summary>
        /// <param name="event">CallbackQuery пользователя</param>
        public void PerformEvent(CallbackQuery @event)
        {
            switch (@event.Data)
            {
                case "Games":
                    Program.Bot.SendTextMessageAsync(@event.From.Id, Games, replyMarkup :Keyboards.Keyboard);
                    Program.Bot.AnswerCallbackQueryAsync(@event.Id).Wait();

                    break;
                case "Exit_turn":
                    ExitTurn(@event.From.Id.ToString(), @event.Message.MessageId);

                    break;
                default:
                    new CrossZero().Play(@event);
                    break;
            }
        }

        /// <summary>
        ///     Выход из очереди на любую игру
        /// </summary>
        /// <param name="userId">UserId пользователя</param>
        /// <param name="messageId">MessageId сообщения, которое отправляется пользователю, когда он вступает в очередь на игру</param>
        public static void ExitTurn(string userId, int messageId)
        {
            var flag = false;
            for (var i = 0; i < TurnWaitPlayer.Length; i++)
            {
                var info = GameBotOnline.CheckIndex(userId, TurnWaitPlayer[i]);
                if (info[1] != -1)
                {
                    TurnWaitPlayer[i][info[0]].Users.RemoveAt(info[1]);
                    if (TurnWaitPlayer[i][info[0]].Users.Count == 0)
                        TurnWaitPlayer[i].RemoveAt(info[0]);
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                Program.Bot.SendTextMessageAsync(userId, "Вы успешно вышли из очереди!",
                                                 replyMarkup :Keyboards.Keyboard);
                Program.Bot.DeleteMessageAsync(userId, messageId);
            }
        }

        /// <summary>
        ///     Проверка пользователя на нахождение в очереди на игру.
        /// </summary>
        /// <param name="you">UserId пользователя</param>
        /// <returns>True, если пользователь находится в очереди ожидания соперника.</returns>
        private static bool CheckingInTurnGamePersonal(string you)
        {
            for (var i = 0; i < TurnWaitPlayer.Length; i++)
            {
                var info = GameBotOnline.CheckIndex(you, TurnWaitPlayer[i]);
                if (info[1] != -1) return true;
            }

            return false;
        }
    }
}