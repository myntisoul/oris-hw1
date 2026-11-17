using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using oris_hw1.models; // Твой namespace с классом User
using System.Collections.Generic;
using System.Threading.Tasks;
using oris_hw1;
using ORM;

namespace TestProject1
{
    [TestClass]
    public class OrmTests
    {
        private readonly string _connStr = "Server=localhost,1433;Database=OrisDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";
        private OrmContext _orm;

        // Этот метод запускается ПЕРЕД каждым тестом. 
        // Он очищает базу, чтобы тесты не мешали друг другу.
        [TestInitialize]
        public void Setup()
        {
            _orm = new OrmContext(_connStr);

            using var connection = new SqlConnection(_connStr);
            connection.Open();
            using var command = new SqlCommand("DELETE FROM Users; DBCC CHECKIDENT ('Users', RESEED, 0);", connection);
            command.ExecuteNonQuery();
        }

        [TestMethod]
        public async Task Insert_And_GetAll_ShouldWork()
        {
            // Arrange
            var newUser = new User { Name = "TestUser", Email = "test@test.com", Age = 25 };

            // Act
            int newId = await _orm.InsertAsync("Users", newUser);
            var users = await _orm.GetAllAsync<User>("Users");

            // Assert
            Assert.IsTrue(newId > 0, "ID должен быть больше нуля");
            Assert.AreEqual(1, users.Count, "В базе должен быть ровно 1 пользователь");
            Assert.AreEqual("TestUser", users[0].Name, "Имя не совпадает");
        }

        [TestMethod]
        public async Task Update_ShouldModifyExistingUser()
        {
            // Arrange
            var user = new User { Name = "OldName", Email = "old@test.com", Age = 20 };
            int id = await _orm.InsertAsync("Users", user);

            // Act
            user.Id = id;
            user.Name = "NewName";
            user.Age = 30;
            int rowsAffected = await _orm.UpdateAsync("Users", id, user);

            var updatedUser = (await _orm.GetAllAsync<User>("Users"))[0];

            // Assert
            Assert.AreEqual(1, rowsAffected, "Должна обновиться 1 строка");
            Assert.AreEqual("NewName", updatedUser.Name, "Имя не обновилось");
            Assert.AreEqual(30, updatedUser.Age, "Возраст не обновился");
        }

        [TestMethod]
        public async Task Delete_ShouldRemoveUser()
        {
            // Arrange
            var user = new User { Name = "ToDelete", Email = "del@test.com", Age = 99 };
            int id = await _orm.InsertAsync("Users", user);

            // Act
            int rowsAffected = await _orm.DeleteAsync("Users", id);
            var users = await _orm.GetAllAsync<User>("Users");

            // Assert
            Assert.AreEqual(1, rowsAffected, "Должна удалиться 1 строка");
            Assert.AreEqual(0, users.Count, "В базе не должно остаться пользователей");
        }
    }
}