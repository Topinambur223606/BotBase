using System;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotBase.Utils
{
    public static class TelegramExtensions
    {
        private static readonly char[] markdownCharacters = new char[] { '\\', '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

        public static string MdEscape(this string source)
        {
            StringBuilder sb = new(source);
            foreach (char c in markdownCharacters)
            {
                sb.Replace(c.ToString(), string.Concat('\\', c));
            }
            return sb.ToString();
        }

        public static Task ReplyWithTextToMessageAsync(this ITelegramBotClient botClient, Message message, string text, ParseMode parseMode = ParseMode.Default)
        {
            return botClient.SendTextMessageAsync(message.Chat.Id, text, parseMode, replyToMessageId: message.MessageId);
        }

        public static string GetDocumentId(this Message message)
        {
            return message.Document?.FileUniqueId ??
                    message.Sticker?.FileUniqueId ??
                    message.Animation?.FileUniqueId ??
                    message.Audio?.FileUniqueId ??
                    message.Video?.FileUniqueId ??
                    message.VideoNote?.FileUniqueId ??
                    message.Voice?.FileUniqueId ??
                    message.Photo?[0].FileUniqueId;
        }

        public static long? GetSourceChannelId(this Message message) => message.ForwardFromChat?.Id;

        public static string GetStickerpackName(this Message message) => message.Sticker?.SetName;

        public static string GetUserFullName(this User user)
        {
            if (user.LastName != null)
            {
                return $"{user.FirstName} {user.LastName}";
            }
            else
            {
                return user.FirstName;
            }
        }

        public static string GetUserLink(this User user)
        {
            return @$"{user.GetUserFullName().MdEscape()} \([{user.Id}](tg://user?id={user.Id})\)";
        }

        public static DateTime GetDate(this Message message)
        {
            return message.EditDate ?? message.Date;
        }
    }
}
