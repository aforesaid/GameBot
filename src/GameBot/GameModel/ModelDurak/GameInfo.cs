using System;
using System.Collections.Generic;
using System.Text;

namespace GameBot.GameModel.ModelDurak
{
    public class GameInfo
    {
        public GameInfo(int count)
        {
            CountUser = count;
        }

        /// <summary>
        ///     Список карт, которые лежат в колоде
        /// </summary>
        public List<string> Deck { get; set; }

        /// <summary>
        ///     Количество активных игроков
        /// </summary>
        public int CountUser { get; set; }

        /// <summary>
        ///     Список карт , которые лежат сейчас на столе
        /// </summary>
        public List<string> Now { get; set; }

        /// <summary>
        ///     Номер игрока, который будет ходить следующим
        /// </summary>
        public int NumUserSelect { get; set; }

        /// <summary>
        ///     Козырь в игре
        /// </summary>
        public char MainSuit { get; set; }

        /// <summary>
        ///     Состояние в игре :
        ///     0 - ход, 1 - отбиваются, 2 - подкидывают
        /// </summary>
        public int Stage { get; set; }

        /// <summary>
        ///     Игрок берёт карты, которые на столе
        /// </summary>
        public bool Take { get; set; }

        /// <summary>
        ///     Количество игроков, у которых <see cref="UserInfo.Pass" /> = true
        /// </summary>
        public int StagePass { get; set; }
    }
}
