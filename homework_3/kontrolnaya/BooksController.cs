using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace oris_hw1.kontrolnaya
{
    [HttpController]
    public class BooksController
    {
        [HttpGET]
        public (string, int) GetBooks(NameValueCollection query)
        {
            StringBuilder sb = new StringBuilder();
            var books = BookStorage.Books;
            string filter = query["author"];
            if (filter != null)
            {
                books = books.Where(b => b.Author.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            sb.Append("<form method='GET' action='/books'><input type='text' name='author'" +
                "placeholder='поиск по автору'><button type='submit'>искать</button></form>");
            sb.Append(@"
            <h3>добавить книгу</h3>
            <form method='POST' action='/books/create'>
                <input type='text' name='title' required placeholder='Название (2-100 симв.)'>
                <input type='text' name='author' required placeholder='Автор (2-60 симв.)'>
                <button type='submit'>Добавить</button>
            </form>
                ");
            sb.Append("<ul>");
            foreach (var b in books)
            {
                sb.Append($"<li><a href='/books/details?id={b.Id}'>{WebUtility.HtmlEncode(b.Title)}</a> — {WebUtility.HtmlEncode(b.Author)} ({b.Status})</li>");
            }
            sb.Append("</ul>");

            return (sb.ToString(), 200);
        }
        [HttpGET]
        public (string, int) GetDetails(NameValueCollection query)
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
    }
}
