using oris_hw1.framework.core;
using ORM;
using System.Net;
using System.Text;
using System.Text.Json;
using oris_hw1.models;

namespace oris
{
    public class Settings
    {
        public string[] Prefixes { get; set; }
    }

    class Program
    {
        static async Task Main()
        {
            string connStr = "Server=localhost,1433;Database=OrisDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

            // Создаем экземпляр твоей ORM (назови класс так, как ты его назвал)
            var orm = new OrmContext(connStr);

            //// 1. Тест GetAll
            //var allUsers = await orm.GetAllAsync<User>("Users");
            //Console.WriteLine($"Загружено пользователей: {allUsers.Count}");

            //// 2. Тест Insert
            var newUser = new User { Name = "Test User", Email = "test@test.com", Age = 99 };
            //int newId = await orm.InsertAsync("Users", newUser);
            //Console.WriteLine($"Добавлен новый пользователь с ID: {newId}");

            // 3. Тест Update
            newUser.Age = 100;
            int updated = await orm.UpdateAsync("Users", 3, newUser);
            Console.WriteLine($"Обновлено строк: {updated}");

            // 4. Тест Delete
            int deleted = await orm.DeleteAsync("Users", 4);
            Console.WriteLine($"Удалено строк: {deleted}");
            string pathSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
            string jsonString = File.ReadAllText(pathSettings);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, options);

            HttpServer server = new HttpServer();
            server.Start();
            Console.WriteLine("write 'stop' to stop the server");
            Console.WriteLine("http://127.0.0.1:8888/login.html");
            while (true)
            {
                string command = Console.ReadLine();
                if (command == "stop") { server.Stop(); break; }
            }
        }
    }
}

