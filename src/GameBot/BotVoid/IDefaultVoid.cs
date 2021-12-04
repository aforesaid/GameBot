using Telegram.Bot.Types;

namespace GameBot.BotVoid
{
    public interface IDefaultVoid
    { 
        void PerformEvent(CallbackQuery @event);
        void PerformMessage(Message message);
    }
}
