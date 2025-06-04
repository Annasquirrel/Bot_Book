using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot_Book.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using static Bot_Book.Models.Book;

namespace Bot_Book.Clients
{
    public class BookClient
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl = Constants.BaseUrl;

        public BookClient()
        {
            _client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
        }

        //Пошук за назвою
        public async Task<BookRoot> GetBook(string title)
        {
            var url = $"GetDateBook?title={Uri.EscapeDataString(title)}";
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BookRoot>(content);
        }

        //Додавання книги
        public async Task<string> AddBookAsync(string title, string author, string pageCount)
        {
            var url = $"GetBookByTitleAuthorAndPageCount?title={Uri.EscapeDataString(title)}" +
                      $"&author={Uri.EscapeDataString(author)}&pageCount={Uri.EscapeDataString(pageCount)}";
            var resp = await _client.GetAsync(url);
            return resp.IsSuccessStatusCode
                ? "✅ Книгу успішно додано до бази."
                : $"❌ Помилка: {resp.ReasonPhrase}";
        }

        //Додавання коментаря 
        public async Task<string> UpdateCommentAsync(string title, string comment)
        {
            var body = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var url = $"UpdateComment?title={Uri.EscapeDataString(title)}&comment={Uri.EscapeDataString(comment)}";
            var resp = await _client.PutAsync(url, body);
            return resp.IsSuccessStatusCode
                ? "✅ Коментар збережено."
                : $"❌ Помилка: {resp.ReasonPhrase}";
        }

        //Видалення книги
        public async Task<string> DeleteBookAsync(string title)
        {
            var url = $"DeleteBook?title={Uri.EscapeDataString(title)}";
            var resp = await _client.DeleteAsync(url);
            return resp.IsSuccessStatusCode
                ? "🗑️ Книгу вилучено."
                : $"❌ Помилка: {resp.ReasonPhrase}";
        }

        //Отримання всієї бібліотеки користувача
        public class SimpleBook
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string PageCount { get; set; }
            public string Description { get; set; }
            public string Comment { get; set; }
        }

        public async Task<List<SimpleBook>> GetAllBooksAsync()
        {
            var resp = await _client.GetAsync("GetAllBooks");
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SimpleBook>>(json);
        }
    }
}


