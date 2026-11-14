using Microsoft.VisualStudio.TestTools.UnitTesting; // Обязательный using!
using oris2; 
using oris_hw1;
using oris_hw1.framework.core;
namespace TestProject1
{
    [TestClass]
    public sealed class TemplateTests
    {
        [TestMethod]
        public void RenderFromString_Test1()
        {
            HtmlTemplateRenderer renderer = new HtmlTemplateRenderer();

            User user = new User() { Name = "ivan", Group = "11-541", Age = 22, Sex = true };
            string template = "<p>имя: ${User.Name}</p>" +
                        "<p>возраст: ${User.Age}</p>" +
                        "<p>группа: ${User.Group}</p>" +
                        "<p>пол: ${User.Sex}</p>";

            Book book = new Book() { Name = "москва-петушки", Year = "1970", Author = "в. ерофеев" };
            string template1 = "<p>имя: ${Book.Name}</p>" +
                            "<p>автор: ${Book.Author}</p>" +
                            "<p>год: ${Book.Year}</p>";

            Pen pen = new Pen() { Name = "pen", Color = "cyan", Width = 10, Price = 0 };
            string template2 = "<p>имя: ${Pen.Name}</p>" +
                            "<p>цвет: ${Pen.Color}</p>" +
                            "<p>ширина: ${Pen.Width}</p>" +
                            "<p>цена: ${Pen.Price}</p>";

            string result = renderer.RenderFromString(template, user);
            string result1 = renderer.RenderFromString(template1, book);
            string result2 = renderer.RenderFromString(template2, pen);
            Assert.AreEqual(result, "<p>имя: ivan</p><p>возраст: 22</p><p>группа: 11-541</p><p>пол: True</p>");
            Assert.AreEqual(result1, "<p>имя: москва-петушки</p><p>автор: в. ерофеев</p><p>год: 1970</p>");
            Assert.AreEqual(result2, "<p>имя: pen</p><p>цвет: cyan</p><p>ширина: 10</p><p>цена: 0</p>");
        }

        [TestMethod]
        public void RenderFromString_Test2()
        {
            Bag bag = new Bag() { Name = "sumkaa", Size = new Size(1, 1, 1), Color = new Color(234, 2, 445), MadeIn = new MadeIn(1, "russia", "kazan"), Price = 100 };
            string template = "<h1>${Bag.Name} (${Bag.Price})</h1>" +
                "<b>country</b>: ${Bag.MadeIn.Country} ${Bag.MadeIn.City} ${Bag.MadeIn.Code}" +
                "<b>size</b>: ${Bag.Size.Height} ${Bag.Size.Width} ${Bag.Size.Length}" +
                "<b>color</b>: ${Bag.Color.Red} ${Bag.Color.Green} ${Bag.Color.Blue}";
            HtmlTemplateRenderer renderer = new HtmlTemplateRenderer();
            string result = renderer.RenderFromString2(template, bag);
            Assert.AreEqual("<h1>sumkaa (100)</h1><b>country</b>: russia kazan 1<b>size</b>: 1 1 1<b>color</b>: 234 2 445", result);
        }

        [TestMethod]
        public void RenderFromString_Test3()
        {
            string template = "$if (Sex)" +
                                "<p>девушкам бесплатно</p>" +
                                "$else" +
                                "<p>с тебя 5к</p>";
            User user1 = new User() { Name = "ilyas", Age = 19, Group = "11-541", Sex = false };
            User user2 = new User() { Name = "ilyas", Age = 19, Group = "11-541", Sex = true };
            HtmlTemplateRenderer renderer = new HtmlTemplateRenderer();
            string result1 = renderer.RenderFromString3(template, user1);
            string result2 = renderer.RenderFromString3(template, user2);
            Assert.AreEqual("<p>с тебя 5к</p>", result1);
            Assert.AreEqual("<p>девушкам бесплатно</p>", result2);
        }

        [TestMethod]
        public void RenderFromString_Test4()
        {
            User[] userArray = new User[3] { new User() { Name = "ilyas", Age = 19, Group = "11-541", Sex = true }, new User() { Name = "gonduras", Age = 19, Group = "11-541", Sex = true }, new User() { Name = "obeme", Age = 19, Group = "11-541", Sex = true } };
            string template = "<ul>" +
                "$foreach (User user in users)" +
                "{" +
                "<li>user.Name</li>" +  
                "}" +
                "</ul>";
            var dataModel = new { users = userArray };
            HtmlTemplateRenderer renderer = new HtmlTemplateRenderer();
            string result = renderer.RenderFromString4(template, dataModel);
            Assert.AreEqual("<ul><li>ilyas</li><li>gonduras</li><li>obeme</li></ul>", result);
        }
    }
}
