using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Bot_Book.Clients;
using static Bot_Book.Models.Book;

namespace Bot_Book
{
    public class BookSearch124_Bot
    {
        private TelegramBotClient botClient = new TelegramBotClient("7794838059:AAFKU-al3h1qxLEEek_yghvFnATCBTv_OxM");
        private CancellationToken cancellationToken = new CancellationToken();
        private ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        private Dictionary<long, string> userStates = new();
        private Dictionary<long, Dictionary<string, string>> tempData = new();

        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMe();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Помилка: {exception.Message}");
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }

        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            long chatId = message.Chat.Id;
            string text = message.Text ?? string.Empty;

            if (userStates.TryGetValue(chatId, out var state))
            {
                var client = new BookClient();
                switch (state)
                {
                    case "waiting_for_title":
                        Clear(chatId);
                        var data = await client.GetBook(text);
                        await SendSearchResult(chatId, data);
                        return;

                    case "add_title":
                        SaveTemp(chatId, "title", text);
                        userStates[chatId] = "add_author";
                        await botClient.SendMessage(chatId, "Вкажіть автора:");
                        return;

                    case "add_author":
                        SaveTemp(chatId, "author", text);
                        userStates[chatId] = "add_pages";
                        await botClient.SendMessage(chatId, "Скільки сторінок?");
                        return;

                    case "add_pages":
                        SaveTemp(chatId, "pages", text);
                        var d = tempData[chatId];
                        string addResult = await client.AddBookAsync(d["title"], d["author"], d["pages"]);

                        Clear(chatId);
                        await botClient.SendMessage(chatId, addResult);
                        return;

                    case "comment_title":
                        SaveTemp(chatId, "title", text);
                        userStates[chatId] = "comment_text";
                        await botClient.SendMessage(chatId, "Введіть коментар:");
                        return;

                    case "comment_text":
                        SaveTemp(chatId, "comment", text);
                        var c = tempData[chatId];
                        var books = await client.GetAllBooksAsync();
                        var bookToComment = books.FirstOrDefault(b =>
                            b.Title.Equals(c["title"], StringComparison.OrdinalIgnoreCase));

                        string putResult;
                        putResult = await client.UpdateCommentAsync(c["title"], c["comment"]);

                        Clear(chatId);
                        await botClient.SendMessage(chatId, putResult);
                        return;

                    case "delete_title":
                        {
                            string delResult = await client.DeleteBookAsync(text);
                            Clear(chatId);
                            await botClient.SendMessage(chatId, delResult);
                            return;
                        }

                }
            }

            switch (text)
            {
                case "/start":
                case "/inline":
                    var kb = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[]{ "Знайти книгу", "Переглянути бібліотеку" },
                    new KeyboardButton[]{ "Додати книгу", "Видалити книгу" },
                    new KeyboardButton[]{ "Додати коментар до книги" }
                })
                    { ResizeKeyboard = true };
                    await botClient.SendMessage(chatId, "Оберіть дію:", replyMarkup: kb);
                    break;

                case "Знайти книгу":
                    userStates[chatId] = "waiting_for_title";
                    await botClient.SendMessage(chatId, "Введіть назву книги:");
                    break;

                case "Додати книгу":
                    userStates[chatId] = "add_title";
                    tempData[chatId] = new();
                    await botClient.SendMessage(chatId, "Введіть назву книги:");
                    break;

                case "Додати коментар до книги":
                    userStates[chatId] = "comment_title";
                    tempData[chatId] = new();
                    await botClient.SendMessage(chatId, "Введіть назву книги, до якої хочете додати/змінити коментар:");
                    break;

                case "Видалити книгу":
                    userStates[chatId] = "delete_title";
                    await botClient.SendMessage(chatId, "Введіть назву книги для видалення:");
                    break;

                case "Переглянути бібліотеку":
                    var list = await new BookClient().GetAllBooksAsync();
                    if (list.Count == 0)
                    {
                        await botClient.SendMessage(chatId, "📚 Бібліотека порожня.");
                    }
                    else
                    {
                        foreach (var b in list)
                        {
                            string info = $"📖 <b>{b.Title}</b>\n" +
                                          $"✍️ {b.Author}\n" +
                                          $"📄 {b.PageCount} стор.\n" +
                                          (string.IsNullOrWhiteSpace(b.Comment) ? "" : $"💬 {b.Comment}\n");
                            await botClient.SendMessage(chatId, info, parseMode: ParseMode.Html);
                        }
                    }
                    break;

                default:
                    await botClient.SendMessage(chatId, "Не розпізнано команду. Натисніть /start.");
                    break;
            }
        }

        private void SaveTemp(long chatId, string key, string value)
        {
            if (!tempData.ContainsKey(chatId)) tempData[chatId] = new();
            tempData[chatId][key] = value;
        }
        private void Clear(long chatId)
        {
            userStates.Remove(chatId);
            tempData.Remove(chatId);
        }

        private async Task SendSearchResult(long chatId, BookRoot data)
        {
            if (data?.items == null || data.items.Count == 0)
            {
                await botClient.SendMessage(chatId, "❌ Книгу не знайдено.");
                return;
            }

            foreach (var item in data.items.Take(5))
            {
                string reply =
                    $"📖 <b>{item.volumeInfo.title}</b>\n" +
                    $"✍️ {string.Join(", ", item.volumeInfo.authors ?? new List<string> { "Невідомо" })}\n" +
                    $"📄 {item.volumeInfo.pageCount?.ToString() ?? "Невідомо"} стор.\n\n" +
                    $"{(string.IsNullOrWhiteSpace(item.volumeInfo.description) ? "Опис відсутній." : item.volumeInfo.description)}";
                await botClient.SendMessage(chatId, reply, parseMode: ParseMode.Html);
            }
        }
    }
}
