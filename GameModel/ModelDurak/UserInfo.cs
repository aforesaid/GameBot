using System;
using System.Collections.Generic;
using System.Text;

namespace GameBot.GameModel.ModelDurak
{
    /// <summary>
    ///     Детали пользователя в игре Дурак
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        ///     Список карт игрока
        /// </summary>
        public List<string> Card { get; set; }

        /// <summary>
        ///     Состояние игрока, если "пасанул" - true
        /// </summary>
        public bool Pass { get; set; }
    }
}
