using oris_hw1.models;
using ORM;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace oris_hw1.framework.core
{
    [HttpController("users")]
    public class UsersController
    {
        private readonly string _connStr = "Server=localhost,1433;Database=OrisDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
        private readonly OrmContext _orm;

        public UsersController()
        {
            _orm = new OrmContext(_connStr);
        }

        [HttpGET]
        public async Task<(string, int)> Index()
        {
            var users = await _orm.GetAllAsync<User>("Users");
            var sb = new StringBuilder("<h1>Список пользователей</h1><ul>");

            foreach (var u in users)
            {
                sb.Append($@"
                    <li>
                        ID: {u.Id} | Имя: {u.Name} | Email: {u.Email} | Возраст: {u.Age}
                        <button onclick='deleteUser({u.Id})'>Удалить</button>
                        <button onclick='updateUser({u.Id}, `{u.Name}`, `{u.Email}`, {u.Age})'>Изменить</button>
                    </li>");
            }
            sb.Append("</ul>");

            sb.Append(@"
                <h3>Добавить пользователя</h3>
                <form method='POST' action='/users/create'>
                    <input type='text' name='name' placeholder='Имя' required>
                    <input type='email' name='email' placeholder='Email' required>
                    <input type='number' name='age' placeholder='Возраст' required>
                    <button type='submit'>Создать</button>
                </form>

                <script>
                    async function deleteUser(id) {
                        if(confirm('Удалить пользователя?')) {
                            const response = await fetch('/users/delete?id=' + id, { method: 'DELETE' });
                            if(response.status === 204) location.reload();
                            else alert('Ошибка удаления');
                        }
                    }

                    async function updateUser(id, oldName, oldEmail, oldAge) {
                        const name = prompt('Новое имя:', oldName);
                        const email = prompt('Новый Email:', oldEmail);
                        const age = prompt('Новый возраст:', oldAge);
                        
                        if (name && email && age) {
                            // Для PUT данные передаются в теле запроса (x-www-form-urlencoded)
                            const params = new URLSearchParams();
                            params.append('name', name);
                            params.append('email', email);
                            params.append('age', age);

                            const response = await fetch('/users/update?id=' + id, {
                                method: 'PUT',
                                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                                body: params
                            });
                            
                            if(response.status === 204) location.reload();
                            else alert('Ошибка обновления');
                        }
                    }
                </script>
            ");

            return (sb.ToString(), 200);
        }

        [HttpPOST]
        public async Task<(string, int)> Create(NameValueCollection form)
        {
            var newUser = new User
            {
                Name = form["name"],
                Email = form["email"],
                Age = int.Parse(form["age"])
            };

            await _orm.InsertAsync("Users", newUser);
            return ("", 303); // 303 See Other (редирект)
        }

        [HttpDELETE]
        public async Task<(string, int)> Delete(NameValueCollection query)
        {
            // ID приходит в URL (Query String): /database/delete?id=1
            if (!int.TryParse(query["id"], out int id))
                return ("<h1>400 Bad Request</h1>", 400);

            int rowsDeleted = await _orm.DeleteAsync("Users", id);

            if (rowsDeleted == 0)
                return ("<h1>404 Not Found</h1>", 404);

            return ("", 204); // 204 No Content (успешно удалено)
        }

        [HttpPUT]
        public async Task<(string, int)> Update(NameValueCollection query, NameValueCollection form)
        {
            // ID приходит в URL, а данные — в теле запроса (form)
            if (!int.TryParse(query["id"], out int id))
                return ("<h1>400 Bad Request</h1>", 400);

            var updatedUser = new User
            {
                Id = id,
                Name = form["name"],
                Email = form["email"],
                Age = int.Parse(form["age"])
            };

            int rowsUpdated = await _orm.UpdateAsync("Users", id, updatedUser);

            if (rowsUpdated == 0)
                return ("<h1>404 Not Found</h1>", 404);

            return ("", 204); // 204 No Content (успешно обновлено)
        }
    }
}