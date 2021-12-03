using Telegram.Bot.Types;

namespace GameBot.BotVoid
{
    interface IDefaultVoid
    {
        public void PerformEvent(CallbackQuery @event);
        public void PerformMessage(Message message);

    }
}
