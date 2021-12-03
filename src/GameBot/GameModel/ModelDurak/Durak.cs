using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameBot.ConfigModel;
using GameBot.GameModel.ModelDurak;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using User = GameBot.ConfigModel.User;

namespace GameBot.GameModel
{
    internal  class Durak : GameWithOtherPlayer
    {
        public static int NumberGame { get; set; } = 3;

        /// <summary>
        ///     Выход из игры досрочно
        /// </summary>
        /// <param name="userId"> UserId пользователя, который выходит из игры</param>
        /// <param name="objectGame">Объект игры,из которой пользователь выходит</param>
        public override void Quit(Message userId, ObjectGame objectGame)
        {
            var numberUser = -1;
            if (objectGame.CountPlayer > 2)
                for (var i = 0; i < objectGame.CountPlayer; i++)
                    if (userId.Chat.Id.ToString() == objectGame.Users[i].Id)
                        numberUser = i;
            base.Quit(userId, objectGame);
            if (numberUser != -1)
            {
                var info = (GameInfo) objectGame.Info;
                if (info.NumUserSelect >= numberUser) info.NumUserSelect--;
            }
        }

        /// <summary>
        ///     Создание новой сессии/игры
        /// </summary>
        /// <param name="newGame">Объект игры</param>
        /// <param name="numberGameInTurn">Номер игры в очереди ожидания/></param>
        private static async void NewSession(ObjectGame newGame, int numberGameInTurn)
        {
            var game = new GameInfo(numberGameInTurn)
            {
                CountUser = newGame.CountPlayer
            };

            var cards = new Durak().GeneratedCards();
            for (var i = 0; i < game.CountUser; i++)
            {
                var myCards = new List<string>();
                for (var j = 0; j < 6; j++)
                {
                    myCards.Add(cards[0]);
                    cards.RemoveAt(0);
                }

                var user = new UserInfo
                {
                    Card = myCards
                };
                newGame.Users[i].UserInGame = user;
            }

            game.Deck = cards;
            game.MainSuit = cards[cards.Count - 1][0];
            game.Stage = 0;
            game.Now = new List<string>();
            newGame.NumberGame = 3;
            newGame.Info = game;
            game.NumUserSelect = Math.Max(0, new Durak().FindSelectedUser(newGame));
            newGame.Info = game;
            GameBotOnline.Turn[NumberGame][newGame.NumberInTurnGame] = newGame;
            // UPDATE CARD 
            var result = new Durak().InlineFullGame(GameBotOnline.Turn[NumberGame][newGame.NumberInTurnGame]);
            await new Durak().SendMessage(GameBotOnline.Turn[NumberGame][newGame.NumberInTurnGame], result);
        }

        /// <summary>
        ///     Обработка хода игрока
        /// </summary>
        /// <param name="message">Сообщение пользователя</param>
        /// <param name="objectGame">Подробности игры</param>
        /// <returns></returns>
        public async Task Motion(Message message, ObjectGame objectGame)
        {
            await Program.Bot.DeleteMessageAsync(message.Chat.Id.ToString(), message.MessageId);
            var  numberUser = -1;
            var  number     = objectGame.NumberInTurnGame;
            var  gameInfo   = (GameInfo) objectGame.Info;
            var  stage      = gameInfo.Stage;
            User you        = null;
            for (var i = 0; i < objectGame.Users.Count; i++)
                if (objectGame.Users[i].Id == message.From.Id.ToString())
                {
                    if (i    != gameInfo.NumUserSelect && gameInfo.Stage == 0
                        || i == gameInfo.NumUserSelect && gameInfo.Stage == 1
                        || i == (gameInfo.NumUserSelect + 1) % gameInfo.CountUser
                        && (gameInfo.Stage == 0 || gameInfo.Stage == 2))
                    {
                        await Program.Bot.SendTextMessageAsync(message.From.Id, "Не ваш ход!");
                        return;
                    }

                    numberUser = i;
                    you        = objectGame.Users[i];
                    break;
                }

            if (you == null) return;
            var usersInfo  = (UserInfo) you.UserInGame;
            var numberCard = -1;
            for (var i = 0; i < usersInfo.Card.Count; i++)
                if (usersInfo.Card[i] == message.Text)
                {
                    numberCard = i;
                    break;
                }

            ProcessingMotion(message, objectGame, gameInfo, numberUser, usersInfo, numberCard);
            var game = (GameInfo) objectGame.Info;
            var flag = false;
            for (var i = 0; i < objectGame.Users.Count; i++)
            {
                var user = (UserInfo) objectGame.Users[i].UserInGame;
                if (game.Deck.Count == 0 && user.Card.Count == 0)
                {
                    await Program.Bot.SendTextMessageAsync(objectGame.Users[i].Id,
                                                           "Поздравляем! Вы победили :)",
                                                           replyMarkup :Keyboards.Keyboard);
                    game.CountUser--;
                    if (you.Id             == objectGame.Users[i].Id) flag = true;
                    if (game.NumUserSelect >= i) game.NumUserSelect--;
                    objectGame.Info = game;
                    new Durak().DeleteMessage(objectGame.Users[i].Id, int.Parse(objectGame.Users[i].MessageId), 5000);
                    objectGame.Users.RemoveAt(i);
                    if (objectGame.Users.Count == 1)
                    {
                        await Program.Bot.SendTextMessageAsync(objectGame.Users[0].Id,
                                                               "Упс, увы , вы проиграли :(\n Сыграйте ещё раз.",
                                                               replyMarkup :Keyboards.Keyboard);
                        new Durak().DeleteMessage(objectGame.Users[0].Id, int.Parse(objectGame.Users[0].MessageId),
                                                  3000);
                        for (var j = number + 1; j < GameBotOnline.Turn[NumberGame].Count; j++)
                            GameBotOnline.Turn[NumberGame][i].NumberInTurnGame--;
                        GameBotOnline.Turn[NumberGame].RemoveAt(number);
                        return;
                    }
                }
            }

            if (!flag) objectGame.Users[numberUser].UserInGame = usersInfo;
            GameBotOnline.Turn[NumberGame][number] = objectGame;
            if (game.Stage == 0 && stage == 2)
            {
                UpdateDeck(GameBotOnline.Turn[NumberGame][number]);
                var info = (GameInfo) GameBotOnline.Turn[NumberGame][number].Info;
                info.Take = false;
            }

            if (game.Stage == 2 && stage == 1 || game.Stage == 0 && stage == 2)
                for (var i = 0; i < GameBotOnline.Turn[NumberGame][number].CountPlayer; i++)
                {
                    var us = (UserInfo) GameBotOnline.Turn[NumberGame][number].Users[i].UserInGame;
                    us.Pass                                                    = false;
                    GameBotOnline.Turn[NumberGame][number].Users[i].UserInGame = us;
                }

            var result = InlineFullGame(GameBotOnline.Turn[NumberGame][number]);
            await SendMessage(GameBotOnline.Turn[NumberGame][number], result);
        }

        private void ProcessingMotion(Message message, ObjectGame objectGame, GameInfo gameInfo, int numberUser,
            UserInfo usersInfo, int numberCard)
        {
            switch (gameInfo.Stage)
            {
                //пользователи ходят
                case 0:
                    FirstMotion(message, gameInfo, numberUser, usersInfo, numberCard);
                    break;
                //пользователи отбиваются
                case 1:
                    //
                    SecondMotion(message, gameInfo, numberUser, usersInfo, numberCard);
                    break;
                ////пользователи подкидывают
                case 2:
                    ThirdthMotion(message, objectGame, gameInfo, numberUser, usersInfo, numberCard);
                    break;
            }
        }

        /// <summary>
        ///     Обработка хода, в случае, если игроки "подкидывают"
        /// </summary>
        /// <param name="message"></param>
        /// <param name="objectGame"></param>
        /// <param name="gameInfo"></param>
        /// <param name="numberUser"></param>
        /// <param name="usersInfo"></param>
        /// <param name="numberCard"></param>
        private static void ThirdthMotion(Message message, ObjectGame objectGame, GameInfo gameInfo, int numberUser,
            UserInfo usersInfo, int numberCard)
        {
            if (gameInfo.Stage == 2 && numberUser != (gameInfo.NumUserSelect + 1) % gameInfo.CountUser)
            {
                if (message.Text.ToLower() == "пас!")
                {
                    switch (usersInfo.Pass)
                    {
                        case true:
                            break;
                        case false:
                            gameInfo.StagePass++;
                            usersInfo.Pass = true;
                            if (gameInfo.StagePass == gameInfo.CountUser - 1 && gameInfo.Take)
                            {
                                gameInfo.Stage = 0;
                                var user = (UserInfo) objectGame.
                                    Users[(gameInfo.NumUserSelect + 1) % gameInfo.CountUser].UserInGame;
                                for (var i = 0; i < gameInfo.Now.Count; i++)
                                    if (gameInfo.Now[i] != "-1")
                                        user.Card.Add(gameInfo.Now[i]);
                                gameInfo.Now = new List<string>();
                                objectGame.Users[(gameInfo.NumUserSelect + 1) % gameInfo.CountUser].
                                    UserInGame = user;
                                gameInfo.NumUserSelect = (gameInfo.NumUserSelect + 2) % gameInfo.CountUser;
                            }

                            var j                                                       = 1;
                            while (j < gameInfo.Now.Count && gameInfo.Now[j] != "-1") j += 2;
                            if (gameInfo.StagePass == gameInfo.CountUser - 1 && !gameInfo.Take
                                                                             && j > gameInfo.Now.Count)
                            {
                                gameInfo.Now           = new List<string>();
                                gameInfo.Stage         = 0;
                                gameInfo.NumUserSelect = (gameInfo.NumUserSelect + 1) % gameInfo.CountUser;
                            }

                            if (gameInfo.StagePass == gameInfo.CountUser - 1 && !gameInfo.Take
                                                                             && j <= gameInfo.Now.Count)
                                gameInfo.Stage = 1;
                            break;
                    }
                }
                else if (numberCard != -1)
                {
                    var countCards = 0;
                    var contains   = false;
                    var card       = usersInfo.Card[numberCard];
                    for (var i = 0; i < gameInfo.Now.Count; i++)
                    {
                        if (gameInfo.Now[i]    == "-1") countCards++;
                        if (gameInfo.Now[i][1] == card[1]) contains = true;
                    }

                    var user = (UserInfo) objectGame.Users[gameInfo.NumUserSelect].UserInGame;
                    if (countCards < user.Card.Count && contains)
                    {
                        gameInfo.Now.Add(card);
                        gameInfo.Now.Add("-1");
                        usersInfo.Card.RemoveAt(numberCard);
                    }
                    else if (countCards + 1 >= user.Card.Count)
                    {
                        gameInfo.Stage = 2;
                    }

                    objectGame.Users[gameInfo.NumUserSelect].UserInGame = user;
                }
            }
        }

        /// <summary>
        ///     Обработка хода, в случае, если игроки "отбиваются"
        /// </summary>
        /// <param name="message"></param>
        /// <param name="gameInfo"></param>
        /// <param name="numberUser"></param>
        /// <param name="usersInfo"></param>
        /// <param name="numberCard"></param>
        private static void SecondMotion(Message message, GameInfo gameInfo, int numberUser, UserInfo usersInfo,
            int numberCard)
        {
            if (gameInfo.Stage == 1 && numberUser == (gameInfo.NumUserSelect + 1) % gameInfo.CountUser)
            {
                gameInfo.StagePass = 0;
                if (message.Text.ToLower() == "беру!")
                {
                    //стадия подкидывания
                    gameInfo.Stage = 2;
                    gameInfo.Take  = true;
                }
                else if (numberCard != -1)
                {
                    var j                                                       = 1;
                    while (j < gameInfo.Now.Count && gameInfo.Now[j] != "-1") j += 2;
                    if (j < gameInfo.Now.Count)
                    {
                        if (new Durak().CheckCard(gameInfo.Now[j - 1], message.Text, gameInfo.MainSuit))
                        {
                            usersInfo.Card.RemoveAt(numberCard);
                            gameInfo.Now[j] = message.Text;
                            if (usersInfo.Card.Count == 0)
                            {
                                gameInfo.Stage         = 0;
                                gameInfo.NumUserSelect = (gameInfo.NumUserSelect + 1) % gameInfo.CountUser;
                            }

                            var x                                                       = 1;
                            while (x < gameInfo.Now.Count && gameInfo.Now[x] != "-1") x += 2;
                            if (x > gameInfo.Now.Count) gameInfo.Stage                  =  2;
                        }
                    }
                    else
                    {
                        gameInfo.Stage = 2;
                    }
                }
            }
        }

        /// <summary>
        ///     Обработка хода, в случае, если игрок "ходит"
        /// </summary>
        /// <param name="message"></param>
        /// <param name="gameInfo"></param>
        /// <param name="numberUser"></param>
        /// <param name="usersInfo"></param>
        /// <param name="numberCard"></param>
        private static void FirstMotion(Message message, GameInfo gameInfo, int numberUser, UserInfo usersInfo,
            int numberCard)
        {
            if (numberUser == gameInfo.NumUserSelect)
            {
                if (message.Text.ToLower() == "пас!" && gameInfo.Now.Count != 0)
                {
                    switch (usersInfo.Pass)
                    {
                        case true:
                            break;
                        case false:
                            gameInfo.StagePass++;
                            gameInfo.Stage = 1;
                            usersInfo.Pass = true;
                            break;
                    }

                    return;
                }

                if (numberCard == -1) return;

                // обновить стол
                if (gameInfo.Now.Count == 0)
                {
                    gameInfo.Now.Add(message.Text);
                    gameInfo.Now.Add("-1");
                    usersInfo.Card.RemoveAt(numberCard);
                }
                else
                {
                    var j = 0;
                    while (j < gameInfo.Now.Count)
                    {
                        if (gameInfo.Now[j][1] == message.Text[1])
                        {
                            gameInfo.Now.Add(message.Text);
                            gameInfo.Now.Add("-1");
                            usersInfo.Card.RemoveAt(numberCard);
                            break;
                        }

                        j += 2;
                    }
                }
            }
        }

        /// <summary>
        ///     Удаление сообщений пользователей
        /// </summary>
        /// <param name="id">UserId пользователя</param>
        /// <param name="mesId">MessageId сообщения,которое удаляется</param>
        /// <param name="time">Задержка удаления сообщения (в миллисекундах)</param>
        private async void DeleteMessage(string id, int mesId, int time)
        {
            await Task.Delay(time);
            await Program.Bot.DeleteMessageAsync(id, mesId);
        }

        /// <summary>
        ///     Генерация колоды для новой игры
        /// </summary>
        /// <returns>Рандомная колода</returns>
        private List<string> GeneratedCards()
        {
            var result  = new List<string>();
            var suit    = "♠♥♦♣";
            var bigCard = "JQKA";
            for (var i = 0; i < 4; i++)
            for (var j = 6; j < 15; j++)
                if (j > 10)
                    result.Add(suit[i] + bigCard[j % 11].ToString());
                else
                    result.Add(suit[i] + j.ToString());
            for (var i = 0; i < 100000; i++)
            {
                var x    = new Random().Next(i % 36, 36);
                var y    = new Random().Next(0,      i % 37);
                var turn = result[x];
                result[x] = result[y];
                result[y] = turn;
            }

            return result;
        }

        /// <summary>
        ///     Обновление карт на руках у игроков
        /// </summary>
        /// <param name="game">Объект активной игры</param>
        /// <returns></returns>
        private void UpdateDeck(ObjectGame game)
        {
            var gameInfo   = (GameInfo) game.Info;
            var numberUser = gameInfo.NumUserSelect - 1;
            for (var i = numberUser; i > numberUser - gameInfo.CountUser; i--)
            {
                var j            = i >= 0 ? i : i + gameInfo.CountUser;
                var userSelected = (UserInfo) game.Users[j].UserInGame;
                while (userSelected.Card.Count < 6 && gameInfo.Deck.Count > 0)
                {
                    userSelected.Card.Add(gameInfo.Deck[0]);
                    gameInfo.Deck.RemoveAt(0);
                }

                game.Users[j].UserInGame = userSelected;
            }

            game.Info = gameInfo;
        }

        /// <summary>
        ///     Генерация текста сообщения для пользователей (подробности игры)
        /// </summary>
        /// <param name="objectGame">Объект активной игры</param>
        /// <returns>Текст для пользователей</returns>
        private string InlineFullGame(ObjectGame objectGame)
        {
            var info     = (GameInfo) objectGame.Info;
            var userCard = new string[objectGame.Users.Count];
            for (var i = 0; i < objectGame.Users.Count; i++)
            {
                var userInGame = (UserInfo) objectGame.Users[i].UserInGame;
                userCard[i] += i == info.NumUserSelect
                    ? $"❗{objectGame.Users[i].Name}: x"
                    : $"ᅠ{objectGame.Users[i].Name}: x";
                userCard[i] += userInGame.Card.Count + "🃏";
            }

            string result = null;
            result += "ᅠᅠИгра \"Дурак\"\n\n";
            foreach (var str in userCard)
                result += str + "\n";
            result += "\n";
            result += "Cтол:\n";
            for (var i = 0; i < info.Now.Count; i++)
                if (i % 2 == 1)
                {
                    result += info.Now[i] != "-1" ? info.Now[i] : "🃏";
                    result += "  ";
                }
                else
                {
                    result += info.Now[i];
                }

            result += $"\nКолода: 🃏x{info.Deck.Count} | Козырь: {info.MainSuit}";
            if (info.Stage == 2)
                result += $"\n\n❌Пасанули - {info.StagePass}/{objectGame.Users.Count - 1} игроков ❌";
            return result;
        }

        /// <summary>
        ///     Отправка сообщения пользователям относительно их хода
        /// </summary>
        /// <param name="objectGame">Объект активной игры</param>
        /// <param name="message">Основной текст для всех пользователей</param>
        private async Task SendMessage(ObjectGame objectGame, string message)
        {
            var info = (GameInfo) objectGame.Info;
            var keyboard = new ReplyKeyboardMarkup
            {
                OneTimeKeyboard = true,
                ResizeKeyboard  = true
            };
            for (var i = 0; i < objectGame.Users.Count; i++)
            {
                var user = (UserInfo) objectGame.Users[i].UserInGame;
                user.Card.Sort();
                var keyboards = new KeyboardButton[user.Card.Count / 4 + Math.Min(user.Card.Count % 4, 1) + 1][];
                for (var q = 0; q < user.Card.Count / 4 + Math.Min(user.Card.Count % 4, 1); q++)
                {
                    var buttonLine = new List<KeyboardButton>();
                    for (var j = q * 4; j < user.Card.Count && j < q * 4 + 4; j++)
                        buttonLine.Add(new KeyboardButton(user.Card[j]));
                    keyboards[q] = buttonLine.ToArray();
                }

                if (i == (info.NumUserSelect + 1) % info.CountUser)
                    keyboards[user.Card.Count / 4 + Math.Min(user.Card.Count % 4, 1)] =
                        new[] {new KeyboardButton("Беру!")};
                else
                    keyboards[user.Card.Count / 4 + Math.Min(user.Card.Count % 4, 1)] =
                        new[] {new KeyboardButton("Пас!")};
                keyboard.Keyboard = keyboards;
                switch (info.Stage)
                {
                    case 0:
                        if (info.NumUserSelect == i)
                            message += "\n\nВы должны походить или нажать \"Пас!\":";
                        else if ((info.NumUserSelect + 1) % info.CountUser == i)
                            message += "\n\nОжидайте, вы будете отбиваться:";
                        else
                            message += "\n\nОжидайте, вы будете подкидывать:";
                        break;
                    case 1:
                        if ((info.NumUserSelect + 1) % info.CountUser == i)
                            message += "\n\nВы должны отбиваться:";
                        else
                            message += "\n\nОжидайте,вы будете подкидывать:";
                        break;
                    case 2:
                        if ((info.NumUserSelect + 1) % info.CountUser == i)
                            message += "\n\nВам подкидывают:";
                        else if ((info.NumUserSelect + 1) % info.CountUser == i)
                            message += "\n\nОжидайте, вам подкидывают :";
                        else
                            message += "\n\nВы можете сейчас подкинуть или нажать \"Пас!\" :";
                        break;
                }

                if (objectGame.Users[i].MessageId == null)
                {
                    var mes = await Program.Bot.SendTextMessageAsync(objectGame.Users[i].Id, message,
                                                                     replyMarkup :keyboard);

                    objectGame.Users[i].MessageId = mes.MessageId.ToString();
                }
                else
                {
                    var mes = await Program.Bot.SendTextMessageAsync(objectGame.Users[i].Id, message,
                                                                     replyMarkup :keyboard);
                    await Program.Bot.DeleteMessageAsync(objectGame.Users[i].Id,
                                                         int.Parse(objectGame.Users[i].MessageId));
                    objectGame.Users[i].MessageId = mes.MessageId.ToString();
                }

                objectGame.Users[i].UserInGame = user;
            }

            objectGame.Info = info;
        }

        /// <summary>
        ///     Добавление пользователя в список ожидающих
        /// </summary>
        /// <param name="message">Сообщение пользователя</param>
        public async void Start(Message message)
        {
            var count = message.Text.ToLower().Split(' ').Length;
            if (count > 1)
            {
                var f = message.Text.ToLower().Split(' ')[1];
                var i = int.TryParse(f, out var result);
                if (i)
                {
                    if (result < 2 || result > 5)
                    {
                        await Program.Bot.SendTextMessageAsync(message.Chat.Id,
                                                               "Указано неправильное число игроков , необходимо указать от 2 до 5.");
                        return;
                    }
                }
                else
                {
                    await Program.Bot.SendTextMessageAsync(message.Chat.Id,
                                                           "Указано неправильное число игроков , необходимо указать от 2 до 5.");
                    return;
                }
            }
            else
            {
                await Program.Bot.SendTextMessageAsync(message.Chat.Id,
                                                       "Чтобы поиграть в игру Дурак , напишите сначала \"Дурак\",а после через пробел количество пользователей в игре.\n\nНапример, \"Дурак 2\"");
                return;
            }

            try
            {
                if (await new GameBotOnline().CreateNewGame(message.Chat.Id.ToString(),
                                                                      message.From.FirstName, CrossZero.Answers,
                                                                      NumberGame, int.Parse(message.Text.Split(' ')[1]))
                )
                    NewSession(GameBotOnline.Turn[NumberGame][GameBotOnline.Turn[NumberGame].Count - 1],
                               GameBotOnline.Turn[NumberGame].Count - 1);
            }
            catch
            {
                Console.WriteLine("Ошибка в Start");
            }
        }

        /// <summary>
        ///     Проверка бьётся ли карта другой картой
        /// </summary>
        /// <param name="ar">Карта, которую нужно побить</param>
        /// <param name="br">Карта, которая бьёт другую карту</param>
        /// <param name="suit">Козырь в игре</param>
        /// <returns>True, если этой картой возможно побить</returns>
        private bool CheckCard(string ar, string br, char suit)
        {
            if (ar[0] != suit && br[0] == suit) return true; // простая карта на козырь
            if (ar[0] == br[0])
            {
                ar = ar.Substring(1, ar.Length - 1);
                br = br.Substring(1, br.Length - 1);
                ar = new Durak().Change(ar);
                br = new Durak().Change(br);
                if (int.Parse(ar) < int.Parse(br)) return true;
            }

            return false;
        }

        /// <summary>
        ///     Конвертация карты из строкового представления в числовой
        /// </summary>
        /// <param name="card">Карта, которую нужно конвертировать</param>
        /// <returns></returns>
        private string Change(string card)
        {
            switch (card)
            {
                case "A":
                    card = "14";
                    break;
                case "K":
                    card = "13";
                    break;
                case "Q":
                    card = "12";
                    break;
                case "J":
                    card = "11";
                    break;
            }

            return card;
        }

        /// <summary>
        ///     Поиск номера игрока, который должен ходить следующим
        /// </summary>
        /// <param name="objectGame">Объект активной игры</param>
        /// <returns>Номер игрока</returns>
        private int FindSelectedUser(ObjectGame objectGame)
        {
            var count    = 0;
            var member   = -1;
            var gameInfo = (GameInfo) objectGame.Info;
            for (var i = 0; i < objectGame.Users.Count; i++)
            {
                var user = (UserInfo) objectGame.Users[i].UserInGame;
                for (var j = 0; j < user.Card.Count; j++)
                {
                    var card = user.Card[j];
                    if (card[0] == gameInfo.MainSuit)
                        if (int.Parse(Change(card.Substring(1, card.Length - 1))) > count)
                        {
                            member = i;
                            count  = int.Parse(Change(card.Substring(1, card.Length - 1)));
                        }
                }
            }

            return member;
        }
    }
}
