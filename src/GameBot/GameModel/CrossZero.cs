using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameBot.ConfigModel;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GameBot.GameModel
{
    /// <summary>
    ///     Реализация игры Крестики - Нолики
    /// </summary>
    public class CrossZero : GameWithOtherPlayer
    {
        /// <summary>
        ///     Информация по активным играм
        /// </summary>
        public static List<string> Turn { get; set; } = new List<string>();

        public static int NumberGame { get; set; } = 2;

        public static string[] Answers { get; set; } =
        {
            @"Соперник найден! Напишите ""!стоп"" , чтобы выйти из игры! Пусть победит сильнейший :)",
            "Подождите немного! Вы и так в очереди!",
            "Вы в очереди! В скором времени вы найдёте соперника! Ожидайте)"
        };

        /// <summary>
        ///     Создание новой активной сессии - игры Крестики-Нолики
        /// </summary>
        /// <param name="you"> UserId польхователя, отправившего сообщение </param>
        /// <param name="name">FirstName пользователя</param>
        /// <param name="answers">Список ответов пользователю</param>
        /// <param name="numberGame">Номер игры</param>
        /// <param name="numberInTurn">Номер в очереди - ожидания</param>
        /// <param name="countUser">Количество игроков</param>
        /// <returns></returns>
        public new async Task NewSession(string you, string name, string[] answers, int numberGame, int numberInTurn,
            int countUser)
        {
            if (await base.NewSession(you, name, answers, numberGame, numberInTurn, countUser))
            {
                var firstUser  = GameBotOnline.Turn[numberGame][^1].Users[0];
                var secondUser = GameBotOnline.Turn[numberGame][^1].Users[1];
                string[] ask =
                {
                    firstUser.Id,
                    secondUser.Id,
                    numberGame.ToString()
                };
                Start(ask).Wait();
            }
        }

        /// <summary>
        ///     Логика игры Крестики - Нолики
        /// </summary>
        /// <param name="events"> CallbackQuery активного игрока</param>
        public void Play(CallbackQuery events)
        {
            for (var i = 0; i < Turn.Count; i++)
            {
                var str = Turn[i];
                var you = events.From.Id.ToString();
                if (str.Contains(you))
                {
                    var info = str.Split(' ');
                    var first = new[]
                    {
                        info[0], info[3]
                    };
                    var second = new[]
                    {
                        info[1], info[4]
                    };
                    var keyboard = Create(events, info[^1], out var success);
                    if (success)
                    {
                        if (FinishGameNouth(keyboard, info, you)) return;
                        if (first[0] == you && info[^1] == "0")
                        {
                            Turn[i] = Turn[i][..^1] + " 1";
                            PlayAndTryGameWin(keyboard, second, first, you, "◯");
                        }
                        else if (info[1] == you && info[^1] == "1")
                        {
                            Turn[i] = Turn[i].Substring(0, Turn[i].Length - 1) + " 0";
                            PlayAndTryGameWin(keyboard, first, second, you, "❌");
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Завершение игры в случае ничьи
        /// </summary>
        /// <param name="keyboard">Активная клавиатура пользователей в игре</param>
        /// <param name="info">Информация по игрокам</param>
        /// <param name="you"> UserId пользователя,сделавшего ход</param>
        /// <returns>True, если игра была завершена</returns>
        private bool FinishGameNouth(InlineKeyboardMarkup keyboard, string[] info, string you)
        {
            if (Nouth(keyboard))
            {
                Program.Bot.DeleteMessageAsync(info[1], int.Parse(info[4]));
                Program.Bot.DeleteMessageAsync(info[0], int.Parse(info[3]));
                Program.Bot.SendTextMessageAsync(info[0],
                                                 "Ухъ, это была жестокая схватка. Попробуйте ещё раз.\nУвы,пока ничья.",
                                                 replyMarkup :Keyboards.Keyboard);
                Program.Bot.SendTextMessageAsync(info[1],
                                                 "Ухъ, это была жестокая схватка. Попробуйте ещё раз.\nУвы,пока ничья.",
                                                 replyMarkup :Keyboards.Keyboard);
                RemoveGame(you);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Рассылка игрокам информации о совершении хода и об окончание игры, в случае победы какого - либо из игроков
        /// </summary>
        /// <param name="keyboard">Активная клавиатура пользователей в игре</param>
        /// <param name="first">Информация по первому игроку</param>
        /// <param name="second">Информация по первому игроку</param>
        /// <param name="you">UserId пользователя,сделавшего ход</param>
        /// <param name="simvol">Символ, по которому провеяется победа в игре (нолик или крестик)</param>
        private void PlayAndTryGameWin(InlineKeyboardMarkup keyboard, string[] first, string[] second, string you,
            string simvol)
        {
            Program.Bot.EditMessageTextAsync(second[0], int.Parse(second[1]), "Ваш ход ✌",
                                             replyMarkup :keyboard);
            Program.Bot.EditMessageTextAsync(first[0], int.Parse(first[1]), "Ход соперника ☯",
                                             replyMarkup :keyboard);
            if (GetResponce(keyboard, simvol))
            {
                Program.Bot.DeleteMessageAsync(second[0], int.Parse(second[1]));
                Program.Bot.DeleteMessageAsync(first[0],  int.Parse(first[1]));
                Program.Bot.SendTextMessageAsync(second[0],
                                                 "Вы победили! Поздравляю!\n\tСыграйте ещё разок :)",
                                                 replyMarkup :Keyboards.Keyboard);
                Program.Bot.SendTextMessageAsync(first[0], "Вы проиграли! Попробуйте ещё раз:(",
                                                 replyMarkup :Keyboards.Keyboard);
                RemoveGame(you);
            }
        }

        /// <summary>
        ///     Проверка на ничью
        /// </summary>
        /// <param name="keyboard">Активная клавиатура пользователей в игре</param>
        /// <returns>True, если в игре ничья</returns>
        private bool Nouth(InlineKeyboardMarkup keyboard)
        {
            var s = keyboard.InlineKeyboard.ToArray();
            for (var i = 0; i < 3; i++)
            {
                var za = s[i].ToArray();
                s[i] = za;
                for (var j = 0; j < 3; j++)
                    if (za[j].Text == "▫")
                        return false;
            }

            return true;
        }

        /// <summary>
        ///     Завершение игры
        /// </summary>
        /// <param name="userId">Сообщение пользователя</param>
        /// <param name="objectGame">Подробная информация об игре</param>
        public override void Quit(Message userId, ObjectGame objectGame)
        {
            base.Quit(userId, objectGame);
            for (var i = 0; i < Turn.Count; i++)
                if (Turn[i].Contains(userId.From.Id.ToString()))
                {
                    var info = Turn[i].Split(' ');
                    Program.Bot.DeleteMessageAsync(info[1], int.Parse(info[4]));
                    Program.Bot.DeleteMessageAsync(info[0], int.Parse(info[3]));
                    Turn.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        ///     Досрочное завершение игры
        /// </summary>
        /// <param name="you">UserId пользователя-инициатора</param>
        public void RemoveGame(string you)
        {
            var where = GameBotOnline.CheckIndex(you, GameBotOnline.Turn[NumberGame]);
            if (where[1] != -1)
                GameBotOnline.Turn[NumberGame].RemoveAt(where[0]);
            for (var i = where[0]; i < GameBotOnline.Turn[NumberGame].Count; i++)
                GameBotOnline.Turn[NumberGame][i].NumberInTurnGame--;
            for (var i = 0; i < Turn.Count; i++)
                if (Turn[i].Contains(you))
                {
                    Turn.RemoveAt(i);
                    break;
                }
        }

        /// <summary>
        ///     Проверка клавиатуры на победу одного из игроков
        /// </summary>
        /// <param name="keyboard">Активная клавиатура пользователей в игре</param>
        /// <param name="st">Символ, на который осуществляется проверка (крестик или нолик)</param>
        /// <returns>True, если какой-то из игроков победил</returns>
        public bool GetResponce(InlineKeyboardMarkup keyboard, string st)
        {
            var str = new string[3, 3];
            var s   = keyboard.InlineKeyboard.ToArray();
            for (var i = 0; i < 3; i++)
            {
                var za = s[i].ToArray();
                s[i] = za;
                for (var j = 0; j < 3; j++)
                    str[i, j] = za[j].Text;
            }

            if (str[0, 0] == str[0, 1] && str[0, 1] == str[0, 2] && str[0, 2] == st ||
                str[0, 0] == str[1, 1] && str[1, 1] == str[2, 2] && str[2, 2] == st ||
                str[0, 0] == str[1, 0] && str[1, 0] == str[2, 0] && str[2, 0] == st ||
                str[2, 0] == str[2, 1] && str[2, 1] == str[2, 2] && str[2, 2] == st ||
                str[0, 2] == str[1, 1] && str[1, 1] == str[1, 2] && str[1, 2] == st ||
                str[2, 0] == str[1, 1] && str[1, 1] == str[0, 2] && str[0, 2] == st ||
                str[1, 0] == str[1, 1] && str[1, 1] == str[1, 2] && str[1, 2] == st ||
                str[0, 1] == str[1, 1] && str[1, 1] == str[2, 1] && str[2, 1] == st ||
                str[0, 2] == str[1, 2] && str[1, 2] == str[2, 2] && str[2, 2] == st)
                return true;
            return false;
        }

        /// <summary>
        ///     Добавление игры в список активных игр
        /// </summary>
        /// <param name="ask">Информация по новой игре</param>
        /// <returns></returns>
        private async Task Start(string[] ask)
        {
            var info   = await StartMessage(ask[0], ask[1]);
            var infoes = ask[0] + " " + ask[1]  + " " + NumberGame;
            Turn.Add(infoes     + " " + info[0] + " " + info[1] + " 1");
        }

        /// <summary>
        ///     Отправка первого сообщения с полем игрокам
        /// </summary>
        /// <param name="you">UserId первого  пользователя</param>
        /// <param name="stranger">UserId второго пользователя</param>
        /// <returns>Возвращает массив MessageId полученных, после отправления</returns>
        private async Task<string[]> StartMessage(string you, string stranger)
        {
            var first = await Program.Bot.SendTextMessageAsync(you, "Ход соперника ☯",
                                                               replyMarkup :Keyboards.CrossZero);
            var second =
                await Program.Bot.SendTextMessageAsync(stranger, "Ваш ход✌", replyMarkup :Keyboards.CrossZero);
            return new[] {first.MessageId.ToString(), second.MessageId.ToString()};
        }

        /// <summary>
        ///     Генерация клавиатуры для игроков
        /// </summary>
        /// <param name="event">CallbackQuery игрока</param>
        /// <param name="str">Символ,который должен походить</param>
        /// <param name="success">Вовзращает true,в случае успешного создания клавиатуры</param>
        /// <returns>Вовзращает обновленную клавиатуру для пользователей</returns>
        public InlineKeyboardMarkup Create(CallbackQuery @event, string str, out bool success)
        {
            var q = int.Parse(@event.Data);
            var s = @event.Message.ReplyMarkup.InlineKeyboard.ToArray().ToArray();
            for (var i = 0; i < 3; i++)
            {
                var za = s[i].ToArray();
                s[i] = za;
            }

            var f = s[q / 3].ToArray();
            if (f[q % 3].Text != "◯" && f[q % 3].Text != "❌")
            {
                if (str == "0")
                    f[q  % 3].Text = "◯";
                else f[q % 3].Text = "❌";
                s[q / 3] = f;
                success  = true;
            }
            else
            {
                success = false;
            }

            var board = new InlineKeyboardMarkup
                (new[]
                {
                    (InlineKeyboardButton[]) s[0],
                    (InlineKeyboardButton[]) s[1],
                    (InlineKeyboardButton[]) s[2]
                });

            return board;
        }
    }
}