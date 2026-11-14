using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace oris_hw1.kontrolnaya
{
    [HttpController("books")]
    public class BooksController
    {
        [HttpGET]
        public (string, int) Index(NameValueCollection query)
        {
            var books = BookStorage.Books;
            string filter = query["author"];

            if (!string.IsNullOrEmpty(filter))
            {
                books = books.Where(b => b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            books = books.OrderBy(b => b.Title).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("<h1>Каталог книг</h1>");
            sb.Append("<form method='GET' action='/books'><input type='text' name='author' placeholder='поиск по автору'><button type='submit'>искать</button></form>");
            sb.Append(@"<h3>добавить книгу</h3>
            <form method='POST' action='/books/create'>
                <input type='text' name='title' required placeholder='Название (2-100 симв.)'>
                <input type='text' name='author' required placeholder='Автор (2-60 симв.)'>
                <button type='submit'>Добавить</button>
            </form>");
            sb.Append("<ul>");
            foreach (var b in books)
            {
                sb.Append($"<li><a href='/books/details?id={b.Id}'>{WebUtility.HtmlEncode(b.Title)}</a> — {WebUtility.HtmlEncode(b.Author)} ({b.Status})</li>");
            }
            sb.Append("</ul>");

            return (sb.ToString(), 200);
        }

        [HttpGET]
        public (string, int) Details(NameValueCollection query)
        {
            if (!int.TryParse(query["id"], out int id))
                return ("<h1>400 bad request: неверный id</h1>", 400);

            var book = BookStorage.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return ("<h1>404 not found: книга не найдена</h1>", 404);

            string html = $@"
                <h1>{WebUtility.HtmlEncode(book.Title)}</h1>
                <p>автор: {WebUtility.HtmlEncode(book.Author)}</p>
                <p>статус: {book.Status}</p>
                <a href='/books'>назад к списку</a>
            ";
            return (html, 200);
        }

        [HttpPOST]
        public (string, int) Create(NameValueCollection form)
        {
            string title = form["title"];
            string author = form["author"];

            if (string.IsNullOrWhiteSpace(title) || title.Length < 2 || title.Length > 100 ||
                string.IsNullOrWhiteSpace(author) || author.Length < 2 || author.Length > 60)
            {
                return ("<h1>400 bad request: неверная длина данных</h1>", 400);
            }

            var newBook = new Book
            {
                Id = BookStorage.NextId++,
                Title = title,
                Author = author,
                Status = "unread"
            };
            BookStorage.Books.Add(newBook);

            return ("", 303);
        }

        [HttpPUT]
        public (string, int) Update(NameValueCollection query, NameValueCollection form)
        {
            if (!int.TryParse(query["id"], out int id))
                return ("<h1>400 bad request: неверный id</h1>", 400);

            var book = BookStorage.Books.FirstOrDefault(b => b.Id == id);
            if (book == null)
                return ("<h1>404 not found</h1>", 404);

            string title = form["title"];
            string author = form["author"];
            string status = form["status"];

            if (string.IsNullOrWhiteSpace(title) || title.Length < 2 || title.Length > 100 ||
                string.IsNullOrWhiteSpace(author) || author.Length < 2 || author.Length > 60 ||
                (status != "unread" && status != "read"))
            {
                return ("<h1>400 bad request: неверные данные</h1>", 400);
            }

            book.Title = title;
            book.Author = author;
            book.Status = status;

            return ("", 204);
        }
    }
}