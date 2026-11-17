//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Specialized;
//using System.Threading.Tasks;
//using oris_hw1.framework.core; // Namespace твоего контроллера

//namespace TestProject1
//{
//    [TestClass]
//    public class ControllerTests
//    {
//        [TestMethod]
//        public async Task Index_ShouldReturn200AndHtml()
//        {
//            // Arrange
//            var controller = new DatabaseController(); // Или UsersController

//            // Act
//            var result = await controller.Index();

//            // Assert
//            Assert.AreEqual(200, result.Item2, "Код ответа должен быть 200");
//            Assert.IsTrue(result.Item1.Contains("Список пользователей"), "HTML должен содержать заголовок");
//        }

//        [TestMethod]
//        public async Task Create_ShouldReturn303Redirect()
//        {
//            // Arrange
//            var controller = new DatabaseController();
//            var form = new NameValueCollection
//            {
//                { "name", "ControllerTest" },
//                { "email", "ctrl@test.com" },
//                { "age", "22" }
//            };

//            // Act
//            var result = await controller.Create(form);

//            // Assert
//            Assert.AreEqual(303, result.Item2, "При создании должен быть редирект 303");
//        }

//        [TestMethod]
//        public async Task Delete_WithInvalidId_ShouldReturn400()
//        {
//            // Arrange
//            var controller = new DatabaseController();
//            var query = new NameValueCollection { { "id", "abc" } }; // Передаем строку вместо числа

//            // Act
//            var result = await controller.Delete(query);

//            // Assert
//            Assert.AreEqual(400, result.Item2, "При неверном ID должен быть 400 Bad Request");
//        }
//    }
//}