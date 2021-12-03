using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace GameBot.ConfigModel
{
    /// <summary>
    ///     Каркас модели для игр, которые проводятся с другими пользователями
    /// </summary>
    internal abstract class GameWithOtherPlayer
    {
        public const int CountGame = 4;

        /// <summary>
        ///     Выход из игры досрочно
        /// </summary>
        /// <param name="userId"> UserId пользователя, который выходит из игры</param>
        /// <param name="objectGame">Объект игры,из которой пользователь выходит</param>
        public virtual void Quit(Message userId, ObjectGame objectGame)
        {
            var gameInfo   = GameBotOnline.Turn[objectGame.NumberGame][objectGame.NumberInTurnGame];
            var numberUser = -1;
            for (var i = 0; i < objectGame.CountPlayer; i++)
                if (userId.Chat.Id.ToString() == objectGame.Users[i].Id)
                    numberUser = i;
            GameBotOnline.Turn[objectGame.NumberGame][objectGame.NumberInTurnGame].CountPlayer--;
            GameBotOnline.Turn[objectGame.NumberGame][objectGame.NumberInTurnGame].Users.RemoveAt(numberUser);
            var firstText = objectGame.NumberGame == 0
                ? "Ваш собеседник покинул диалог!"
                : "Ваш соперник покинул игру!";
            var secondText = objectGame.NumberGame == 0
                ? "Вы покинули диалог"
                : "Вы покинули игру!";
            Program.Bot.SendTextMessageAsync(userId.Chat.Id.ToString(), secondText, replyMarkup :Keyboards.Keyboard);

            if (gameInfo.Users.Count == 1)
            {
                for (var i = objectGame.NumberInTurnGame + 1; i < GameBotOnline.Turn[objectGame.NumberGame].Count; i++)
                    GameBotOnline.Turn[objectGame.NumberGame][i].NumberInTurnGame--;
                GameBotOnline.Turn[objectGame.NumberGame].RemoveAt(objectGame.NumberInTurnGame);
                Program.Bot.SendTextMessageAsync(gameInfo.Users[0].Id, firstText, replyMarkup :Keyboards.Keyboard);
            }
        }

        /// <summary>
        ///     Создание новой сессии/игры
        /// </summary>
        /// <param name="userId">UserId пользователя</param>
        /// <param name="name">FirstName пользователя</param>
        /// <param name="answers">Список ответов для пользователя</param>
        /// <param name="numberGame">Номер игры</param>
        /// <param name="numberInTurn">Номер в очереди ожидания игр</param>
        /// <param name="countUser">Количество игроков в игре</param>
        /// <returns></returns>
        public virtual async Task<bool> NewSession(string userId, string name, string[] answers, int numberGame,
            int numberInTurn, int countUser) =>
            await new GameBotOnline().CreateNewGame(userId, name, answers, numberGame, countUser);
        
    }
}