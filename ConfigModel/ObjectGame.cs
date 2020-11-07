using System.Collections.Generic;

namespace GameBot.ConfigModel
{
    /// <summary>
    ///     Модель игры
    /// </summary>
    internal class ObjectGame
    {
        /// <summary>
        ///     Участники игры
        /// </summary>
        public List<User> Users { get; set; }

        /// <summary>
        ///     Номер игры
        /// </summary>
        public int NumberGame { get; set; }

        /// <summary>
        ///     Номер игрока, который ходит
        /// </summary>
        public int NumberInTurnGame { get; set; }

        /// <summary>
        ///     Количество игроков
        /// </summary>
        public int CountPlayer { get; set; }

        /// <summary>
        ///     Дополнительная информация об игре
        /// </summary>
        public object Info { get; set; }
    }

    /// <summary>
    ///     Модель игрока любой игры
    /// </summary>
    internal class User
    {
        /// <summary>
        ///     FirstName пользователя
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     UserId пользователя
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     MessageId сообщения, которое отсылал бот
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        ///     Номер игрока в <see cref="ObjectGame.Users" />
        /// </summary>
        public object UserInGame { get; set; }
    }
}