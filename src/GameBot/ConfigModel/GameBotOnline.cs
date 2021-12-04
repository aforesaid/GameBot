using System.Collections.Generic;
using System.Threading.Tasks;
using GameBot.BotVoid;
using GameBot.GameModel;
using GameBot.GameModel.ModelDurak;
using Telegram.Bot.Types;

namespace GameBot.ConfigModel
{
    public class GameBotOnline
    {
        public delegate void QuitD(Message mes, ObjectGame objectGame);

        /// <summary>
        ///     Список всех игр
        /// </summary>
        public static List<ObjectGame>[] Turn = new List<ObjectGame>[100];

        /// <summary>
        ///     Стандартное количество игроков для каждой игры
        /// </summary>
        private static readonly Dictionary<int, int> CountPlayer = new()
        {
            [0] = 2,
            [1] = 1,
            [2] = 2,
            [3] = 2
        };

        public static QuitD[] Quits { get; } =
        {
            new Anonymchat().Quit,
            new BullAndCows().Exit,
            new CrossZero().Quit,
            new Durak().Quit
        };

        /// <summary>
        ///     Поиск игры по её участнику
        /// </summary>
        /// <param name="userId">UserId пользователя</param>
        /// <returns>Объект, представляющий игру</returns>
        public ObjectGame CheckGame(string userId)
        {
            for (var i = 0; i < GameWithOtherPlayer.CountGame; i++)
            {
                var result = CheckIndex(userId, Turn[i]);
                if (result[0] != -1)
                    return Turn[i][result[0]];
            }

            return null;
        }

        /// <summary>
        ///     Добавляет пользователя к одной из игр, которая в ожидании, создает игру, если набралось необходимое число людей
        /// </summary>
        /// <param name="userId">UserId пользователя</param>
        /// <param name="name">FirstName пользователя</param>
        /// <param name="answer">Список ответов пользователю</param>
        /// <param name="numberGame">Номер игры</param>
        /// <param name="countUser">Количество игроков необходимое для начала игры</param>
        /// <returns>True, если набирается нужное количество игроков и игра переводится в активные</returns>
        public async Task<bool> CreateNewGame(string userId, string name, string[] answer, int numberGame,
            int countUser)
        {
            var player = countUser < 0 ? CountPlayer[numberGame] : countUser;
            var index  = FindWithCountPlayer(PersonalVoid.TurnWaitPlayer[numberGame], player);
            if (PersonalVoid.TurnWaitPlayer[numberGame] != null && index != -1)
            {
                var where = CheckIndex(userId, PersonalVoid.TurnWaitPlayer[numberGame]);
                if (where[0] == -1)
                {
                    PersonalVoid.TurnWaitPlayer[numberGame][index].Users.Add(new User
                    {
                        Id   = userId,
                        Name = name
                    });
                    await Program.Bot.SendTextMessageAsync(userId, answer[2], replyMarkup :Keyboards.KeyboardTurn);

                    if (PersonalVoid.TurnWaitPlayer[numberGame][index].Users.Count
                        == PersonalVoid.TurnWaitPlayer[numberGame][index].CountPlayer
                    ) // если набралось количество игроков
                    {
                        for (var i = 0; i < PersonalVoid.TurnWaitPlayer[numberGame][index].Users.Count; i++)
                        {
                            var mes =
                                await Program.Bot.
                                    SendTextMessageAsync(PersonalVoid.TurnWaitPlayer[numberGame][index].Users[i].Id,
                                                         answer[0]);
                            //добавить изменение количества игроков                    
                            PersonalVoid.TurnWaitPlayer[numberGame][index].Users[i].MessageId =
                                mes.MessageId.ToString();
                        }

                        if (Turn[numberGame] == null) Turn[numberGame] = new List<ObjectGame>();
                        PersonalVoid.TurnWaitPlayer[numberGame][index].NumberInTurnGame = Turn[numberGame].Count;
                        Turn[numberGame].Add(PersonalVoid.TurnWaitPlayer[numberGame][index]);
                        PersonalVoid.TurnWaitPlayer[numberGame].RemoveAt(index);
                        return true;
                    }

                    return false;
                }

                await Program.Bot.SendTextMessageAsync(userId, answer[1]);
            }
            else
            {
                if (PersonalVoid.TurnWaitPlayer[numberGame] == null)
                    PersonalVoid.TurnWaitPlayer[numberGame] = new List<ObjectGame>();
                PersonalVoid.TurnWaitPlayer[numberGame].Add(new ObjectGame
                {
                    NumberGame = numberGame,
                    Users = new List<User>
                    {
                        new User
                        {
                            Id   = userId,
                            Name = name
                        }
                    },
                    CountPlayer = player
                });
                await Program.Bot.SendTextMessageAsync(userId, answer[2], replyMarkup :Keyboards.KeyboardTurn);
            }

            return false;
        }

        /// <summary>
        ///     Поиск пользователя среди участников игр
        /// </summary>
        /// <param name="you">userId пользователя</param>
        /// <param name="objects">Cписок игр</param>
        /// <returns>Два числа, характеризующие адрес игры в списке</returns>
        public static int[] CheckIndex(string you, List<ObjectGame> objects)
        {
            if (objects != null)
                for (var i = 0; i < objects.Count; i++)
                    if (objects[i].Users != null)
                        for (var j = 0; j < objects[i].Users.Count; j++)
                            if (you == objects[i].Users[j].Id)
                                return new[] {i, j};
            return new[] {-1, -1};
        }

        /// <summary>
        ///     Поиск игры в списке с таким же количеством игроков
        /// </summary>
        /// <param name="objects">Список игр в очереди</param>
        /// <param name="count">Количество игроков</param>
        /// <returns>Номер игры в списке.Возвращает -1, если такой игры нет</returns>
        public static int FindWithCountPlayer(List<ObjectGame> objects, int count)
        {
            if (objects != null)
                for (var i = 0; i < objects.Count; i++)
                    if (count == objects[i].CountPlayer)
                        return i;
            return -1;
        }
    }
}