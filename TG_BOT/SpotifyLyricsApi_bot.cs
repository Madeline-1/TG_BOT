using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TG_BOT
{
    public class SpotifyLyricsApi_bot
    {
        private readonly TelegramBotClient botClient = new TelegramBotClient("МійТокен");
        private readonly CancellationToken cancellationToken = new CancellationToken();
        private readonly ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };

        // Зберігає останній ID треку на чат
        private static readonly Dictionary<long, string> lastSongByChat = new();
        // Зберігає очікувані дії після кнопок
        private static readonly Dictionary<long, string> pendingAction = new();


        // Запускає бота та починає отримувати оновлення
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMe();
            Console.WriteLine($"Бот @{botMe.Username} почав працювати");
            Console.ReadKey();
        }


        // Обробляє помилки, які виникають під час роботи бота
        private Task HandlerError(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiEx => $"Помилка в Telegram API:\n[{apiEx.ErrorCode}] {apiEx.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }


        // Обробляє оновлення від Telegram
        private async Task HandlerUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
                await HandlerMessageAsync(update.Message);
        }

        // Обробляє повідомлення від користувача
        private async Task HandlerMessageAsync(Message message)
        {
            var chatId = message.Chat.Id;

            // Обробка відкладених дій
            if (pendingAction.TryGetValue(chatId, out var action))
            {
                pendingAction.Remove(chatId);

                if (action == "delete" && int.TryParse(message.Text, out var delIdx))
                {
                    await DeleteFavoriteAsync(chatId, delIdx);
                    return;
                }
                if (action == "update")
                {
                    var parts = message.Text.Split(' ', 2);
                    if (parts.Length == 2 && int.TryParse(parts[0], out var updIdx))
                    {
                        await UpdateFavoriteAsync(chatId, updIdx, parts[1]);
                        return;
                    }
                }

                if (action == "add_name")
                {
                    var displayName = message.Text.Trim();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        await botClient.SendMessage(chatId, "Назва не може бути порожньою. Спробуйте ще раз.");
                        return;
                    }

                    if (!lastSongByChat.TryGetValue(chatId, out var songId))
                    {
                        await botClient.SendMessage(chatId, "Виникла помилка: не знайдено останнього ID пісні. Спробуйте спочатку знайти слова пісні.");
                        return;
                    }
                    await AddFavoriteAsync(chatId, songId, displayName);
                    return;
                }

                // Якщо жодна з дій не спрацювала
                await botClient.SendMessage(chatId, "Невірний ввід. Спробуйте ще раз.");
                return;
            }

            // Основна обробка команд / меню
            switch (message.Text)
            {
                case "/start":
                    await botClient.SendMessage(chatId, "Привіт! Я бот для отримання текстів пісень з Spotify. Введіть /keyboard для старту.");
                    break;

                case "/keyboard":
                    var mainKb = new ReplyKeyboardMarkup(new[]
                    {
                    new KeyboardButton[] { "Знайти слова пісні", "Список обраних" }
                })
                    { ResizeKeyboard = true };
                    await botClient.SendMessage(chatId, "Виберіть опцію:", replyMarkup: mainKb);
                    break;

                case "Знайти слова пісні":
                    await botClient.SendMessage(chatId, "Введіть ID пісні або посилання на Spotify.");
                    break;

                case "Список обраних":
                    await ShowFavoritesAsync(chatId);
                    break;

                case "Додати до обраних":
                    if (lastSongByChat.TryGetValue(chatId, out var lastId))
                    {
                        await botClient.SendMessage(chatId, "Як назвати цю пісню в списку обраних?");
                        pendingAction[chatId] = "add_name";
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Спершу знайдіть слова для треку.");
                    }
                    break;

                case "Видалити з обраних":
                    await botClient.SendMessage(chatId, "Введіть індекс елемента для видалення:");
                    pendingAction[chatId] = "delete";
                    break;

                case "Оновити обране":
                    await botClient.SendMessage(chatId, "Введіть індекс та новий ID через пробіл, наприклад: 0 5w5wBkH4ana9waptVsxJCq");
                    pendingAction[chatId] = "update";
                    break;

                default:
                    // Якщо це не команда, намагаємося витягти ID та показати слова пісні
                    var songId = ExtractSongId(message.Text);
                    if (songId != null)
                        await SendLyricsAsync(chatId, songId);
                    else
                        await botClient.SendMessage(chatId, "Не розпізнано ID треку.");
                    break;
            }
        }

        // Вилучає ID пісні з посилання
        private string ExtractSongId(string text)
        {
            const string marker = "track/";
            if (text.Contains(marker))
            {
                var parts = text.Split(marker, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 1 ? parts[1].Split('?')[0] : null;
            }
            return text.Length == 22 ? text : null;
        }

        // Основний метод для знаходження слів пісні
        private async Task SendLyricsAsync(long chatId, string songId)
        {
            using var client = new HttpClient();
            var url = Constants.TimeAddress + songId;
            var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                await botClient.SendMessage(chatId, "Не вдалося отримати слова пісні.");
                return;
            }

            var lines = JsonConvert.DeserializeObject<List<string>>(await resp.Content.ReadAsStringAsync());
            if (lines == null || !lines.Any())
            {
                await botClient.SendMessage(chatId, "Слова не знайдено.");
                return;
            }

            // Запам'ятовуємо ID останньої пісні для цього чату
            lastSongByChat[chatId] = songId;

            var text = string.Join("\n", lines);
            var kb = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { "Знайти слова пісні", "Список обраних" },
            new KeyboardButton[] { "Додати до обраних", "Видалити з обраних", "Оновити обране" }
        })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, text, replyMarkup: kb);
        }

        // Метод додавання до списку обраних
        private async Task AddFavoriteAsync(long chatId, string songId, string displayName)
        {
            using var client = new HttpClient();
            // Додаємо displayName у запит (передаємо його як query-параметр)
            var url = $"{Constants.AddFavoriteAddress}?songId={Uri.EscapeDataString(songId)}&displayName={Uri.EscapeDataString(displayName)}";
            var resp = await client.PostAsync(url, null);

            if (resp.IsSuccessStatusCode)
            {
                await botClient.SendMessage(chatId, "Пісню успішно додано до обраних під обраною назвою.");
            }
            else
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                await botClient.SendMessage(chatId, $"Помилка додавання до обраних: {errorContent}");
            }
        }

        // Видаляє пісню зі списку обраних за індексом
        private async Task DeleteFavoriteAsync(long chatId, int index)
        {
            using var client = new HttpClient();
            var resp = await client.DeleteAsync(Constants.RemoveFavoriteAddress + index);
            await botClient.SendMessage(chatId, resp.IsSuccessStatusCode ? "Видалено з обраних." : "Помилка видалення.");
        }

        private async Task UpdateFavoriteAsync(long chatId, int index, string newId)
        {
            using var client = new HttpClient();
            var url = Constants.UpdateFavoriteAddress + index + "&newSongId=" + newId;
            var resp = await client.PutAsync(url, null);
            await botClient.SendMessage(chatId, resp.IsSuccessStatusCode ? "Оновлено обране." : "Помилка оновлення.");
        }

        // Показує список обраних пісень
        private async Task ShowFavoritesAsync(long chatId)
        {
            using var client = new HttpClient();
            var resp = await client.GetAsync(Constants.FavoritesAddress);
            if (!resp.IsSuccessStatusCode)
            {
                await botClient.SendMessage(chatId, "Не вдалося отримати список обраних.");
                return;
            }

            var content = await resp.Content.ReadAsStringAsync();
            JArray arr;
            try { arr = JArray.Parse(content); }
            catch
            {
                await botClient.SendMessage(chatId, "Некоректна відповідь API.");
                return;
            }

            if (!arr.Any())
            {
                await botClient.SendMessage(chatId, "Список обраних порожній.");
                return;
            }

            var lines = arr.Select(t =>
            {
                var idx = t["Index"] ?? t["index"];
                var displayName = t["DisplayName"] ?? t["displayName"];
                var id = t["SongId"] ?? t["songId"];
                return $"{idx}: {displayName} - ({id})";
            });

            var kb = new ReplyKeyboardMarkup(new[]
            {
            new KeyboardButton[] { "Знайти слова пісні", "Список обраних" },
            new KeyboardButton[] { "Видалити з обраних", "Оновити обране" }
        })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, string.Join("\n", lines), replyMarkup: kb);
        }
    }
}
