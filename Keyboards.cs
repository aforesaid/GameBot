using Telegram.Bot.Types.ReplyMarkups;

namespace GameBot
{
    internal static class Keyboards
    {
        /// <summary>
        ///     Шаблон клавиатуры для игры Крестики - Нолики
        /// </summary>
        public static InlineKeyboardMarkup CrossZero = new InlineKeyboardMarkup
            (new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("▫", "0"), InlineKeyboardButton.WithCallbackData("▫", "1"),
                    InlineKeyboardButton.WithCallbackData("▫", "2")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("▫", "3"), InlineKeyboardButton.WithCallbackData("▫", "4"),
                    InlineKeyboardButton.WithCallbackData("▫", "5")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("▫", "6"), InlineKeyboardButton.WithCallbackData("▫", "7"),
                    InlineKeyboardButton.WithCallbackData("▫", "8")
                }
            });

        /// <summary>
        ///     Клавиатура для очереди
        /// </summary>
        public static InlineKeyboardMarkup KeyboardTurn = new InlineKeyboardMarkup(new[]
        {
            InlineKeyboardButton.WithCallbackData("Покинуть очередь ❌", "Exit_turn")
        });

        /// <summary>
        ///     Стартовая клавиатура
        /// </summary>
        public static ReplyKeyboardMarkup Keyboard { get; } = new ReplyKeyboardMarkup
        {
            OneTimeKeyboard = true,
            ResizeKeyboard  = true,
            Keyboard = new[]
            {
                new[]
                {
                    new KeyboardButton("Общаться")
                },
                new[]
                {
                    new KeyboardButton("Быкануть"), new KeyboardButton("Занулить"), new KeyboardButton("Дурак")
                },
                new[]
                {
                    new KeyboardButton("Поддержка")
                }
            }
        };
    }
}