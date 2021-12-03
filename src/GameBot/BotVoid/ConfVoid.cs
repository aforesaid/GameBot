using Telegram.Bot.Types;

namespace GameBot.BotVoid
{
    public class ConfVoid:IDefaultVoid
    {
        /// <summary>
        ///     Метод, реализующий логику обработки сообщений в группе/чате .
        /// </summary>
        /// <param name="message">Сообщение пользователя</param>
        public void PerformMessage(Message message)
        {
        }

        /// <summary>
        ///     Метод, реализующий логику обработки CallbackQuery в группе/чате .
        /// </summary>
        /// <param name="event">CallbackQuery пользователя</param>
        public void PerformEvent(CallbackQuery @event)
        {
        }
    }
}