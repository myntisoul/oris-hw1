using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ORM;
using oris_hw1.models;

namespace oris_hw1.framework.core
{
    [HttpController("database")]
    public class DatabaseController
    {
        private readonly string _connStr = "Server=localhost,1433;Database=OrisDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        // Твоя ORM (замени MyOrm на имя твоего класса)
        private readonly OrmContext _orm;

        public DatabaseController()
        {
            _orm = new OrmContext(_connStr);
        }

        // GET /users
        [HttpGET]
        public async Task<(string, int)> Index()
        {
            // Вот она — магия твоей ORM! Одна строчка вместо 30 строк из старого кода.
            var users = await _orm.GetAllAsync<User>("Users");

            var sb = new StringBuilder("<h1>Список пользователей</h1><ul>");
            foreach (var u in users)
            {
                sb.Append($"<li>ID: {u.Id} | Имя: {u.Name} | Email: {u.Email} | Возраст: {u.Age}</li>");
            }
            sb.Append("</ul>");

            // Форма для добавления
            sb.Append(@"
                <h3>Добавить пользователя</h3>
                <form method='POST' action='/users/create'>
                    <input type='text' name='name' placeholder='Имя' required>
                    <input type='email' name='email' placeholder='Email' required>
                    <input type='number' name='age' placeholder='Возраст' required>
                    <button type='submit'>Создать</button>
                </form>
            ");

            return (sb.ToString(), 200);
        }

        // POST /users/create
        [HttpPOST]
        public async Task<(string, int)> Create(NameValueCollection form)
        {
            var newUser = new User
            {
                Name = form["name"],
                Email = form["email"],
                Age = int.Parse(form["age"])
            };

            // Используем твою ORM для вставки
            await _orm.InsertAsync("Users", newUser);

            // После создания отправляем пользователя обратно на список (редирект 303)
            return ("", 303);
        }
    }
}

