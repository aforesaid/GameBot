using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameBot.ConfigModel;
using Telegram.Bot.Types;
using User = GameBot.ConfigModel.User;

namespace GameBot.GameModel
{
    /// <summary>
    ///     Реализация игры Быки и Коровы
    /// </summary>
    internal class BullAndCows
    {
        /// <summary>
        ///     Метод, реализующий логику игры Быки и Коровы
        /// </summary>
        /// <param name="message"> Сообщение пользователья</param>
        /// <param name="dataGame"> Информация по игре пользователя</param>
        /// <returns></returns>
        public async Task InGame(Message message, ObjectGame dataGame)
        {
            var  mesText    = message.Text.ToLower();
            var  id         = message.From.Id;
            byte bull       = 0;
            byte cow        = 0;
            var  info       = (GameInfo) dataGame.Info;
            var  hidden     = info.NumberIsHidden;
            var  number     = new int[10];
            var  badRequest = true;
            if (mesText.Length == 5)
                for (var i = 0; i < mesText.Length; i++)
                {
                    badRequest = int.TryParse(mesText[i].ToString(), out var index);
                    if (!badRequest) break;
                    if (number[index] != 0)
                    {
                        badRequest = false;
                        break;
                    }

                    number[index] = i + 1;
                }
            else
                badRequest = false;

            if (!badRequest)
            {
                await BadRequest(id);
                return;
            }

            info.Motion++;
            GameBotOnline.Turn[1][dataGame.NumberInTurnGame].Info = info;
            for (var i = 0; i < mesText.Length; i++)
            {
                var index = Convert.ToInt32(hidden[i].ToString());
                if (number[index] == 0) continue;
                if (number[index] == i + 1)
                    bull++;
                else
                    cow++;
            }

            if (bull == 5)
            {
                GameBotOnline.Turn[1].RemoveAt(dataGame.NumberInTurnGame);
                await Program.Bot.SendTextMessageAsync(id,
                                                       "Поздравляю! Вы выиграли!\n"         +
                                                       $"Количество шагов: {info.Motion}\n" +
                                                       "Чтобы снова сыграть , выберите \"Быкануть\".",
                                                       replyMarkup :Keyboards.Keyboard);
            }
            else
            {
                await Program.Bot.SendTextMessageAsync(id,
                                                       $"Коров: {cow}\n"  +
                                                       $"Быков: {bull}\n" +
                                                       $"Шаг: {info.Motion}");
            }
        }

        /// <summary>
        ///     Выполняется в случае, если текст пользователя не соответствует правилам игры.
        /// </summary>
        /// <param name="id"> UserId игрока</param>
        /// <returns></returns>
        private async Task BadRequest(int id)
        {
            await Program.Bot.SendTextMessageAsync(id,
                                                   "Нужно прислать пять разных цифр без пробелов и других разделителей.\n"
                                                   +
                                                   @"Ты еще не угадал все цифры." + "\n"
                                                   + @"(Чтобы прекратить игру, отправь ""!стоп"" .");
        }

        /// <summary>
        ///     Добавление нового игрока
        /// </summary>
        /// <param name="id"> UserId игрока</param>
        public static void AddUsers(int id)
        {
            var    rnd = new Random();
            bool   bcow;
            string step;
            var    hidden = "";
            while (hidden.Length < 5)
            {
                bcow = true;
                var z = rnd.Next(0, 10);
                step = hidden;
                for (var i = 0; i < step.Length; i++)
                    if (step[i].ToString() == z.ToString())
                    {
                        bcow = false;
                        break;
                    }

                if (bcow)
                    hidden += z.ToString();
            }

            if (GameBotOnline.Turn[1] == null) GameBotOnline.Turn[1] = new List<ObjectGame>();
            GameBotOnline.Turn[1].Add(new ObjectGame
            {
                Users            = new List<User> {new User {Id = id.ToString()}},
                CountPlayer      = 1,
                NumberGame       = 1,
                NumberInTurnGame = GameBotOnline.Turn[1].Count,
                Info = new GameInfo
                {
                    NumberIsHidden = hidden,
                    Motion         = 0
                }
            });
            Program.Bot.SendTextMessageAsync(id,
                                             @"Вы выбрали игру ""Быки и коровы"" ."
                                             + "\n\nЯ загадал 5 цифр. Все цифры различны. \n"         +
                                             "Ты должен отгадать их в той же последовательности.\n\n" +
                                             "Я дам тебе подсказки: корова - угадана сама цифра, но не угадана её позиция, бык - угадана цифра с её позицией."
                                             +
                                             " Когда будет 5 быков , ты победишь . Удачи!\n\nА теперь отправляй 5 разных цифр без разделителей , пока не угадаешь.");
        }

        /// <summary>
        ///     Досрочный выход из игры
        /// </summary>
        /// <param name="message"> Сообщение пользователя </param>
        /// <param name="objectGame"> Информация по активной игре </param>
        public async void Exit(Message message, ObjectGame objectGame)
        {
            var id     = message.From.Id;
            var info   = (GameInfo) objectGame.Info;
            var hidden = info.NumberIsHidden;
            GameBotOnline.Turn[1].RemoveAt(objectGame.NumberInTurnGame);
            await Program.Bot.SendTextMessageAsync(id, "Вы вышли из игры\n"      +
                                                       "А было загадано число: " + hidden,
                                                   replyMarkup :Keyboards.Keyboard);
        }

        /// <summary>
        ///     Информация по игре класса <see cref="BullAndCows" />
        /// </summary>
        private class GameInfo
        {
            /// <summary>
            ///     Загаданное число
            /// </summary>
            public string NumberIsHidden { get; set; }

            /// <summary>
            ///     Номер хода
            /// </summary>
            public int Motion { get; set; }
        }
    }
}