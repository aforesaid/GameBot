using System.Threading.Tasks;
using GameBot.ConfigModel;
using Telegram.Bot.Types;

namespace GameBot.GameModel
{
    /// <summary>
    ///     Реализует все функции для анонимного чата.
    /// </summary>
    internal class Anonymchat : GameWithOtherPlayer
    {
        public static int NumberGame { get; set; } = 0;

        public static string[] Answers { get; set; } =
        {
            @"Собеседник найден! Напишите ""!стоп"" , чтобы покинуть собеседника! Можете общаться :)",
            "Подождите немного! Вы и так в очереди!",
            "Вы в очереди! В скором времени вы найдёте собеседника! Ожидайте)"
        };

        /// <summary>
        ///     Переадресация сообщения на другого пользователя , с кем связан этот пользователь.
        /// </summary>
        /// <param name="message">Сообщение, которое нужно обработать.</param>
        /// <param name="infoGame">Информация по активной игре. </param>
        public async Task SendMessage(Message message, ObjectGame infoGame)
        {
            string stranger;
            if (message.From.Id.ToString() == infoGame.Users[0].Id) stranger = infoGame.Users[1].Id;
            else stranger                                                    = infoGame.Users[0].Id;
            await Program.Bot.SendTextMessageAsync(stranger, message.Text);
        }
    }
}