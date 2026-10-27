using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace oris2
{
    public class HtmlTemplateRenderer : IHtmlTemplateRenderer
    {
        public string RenderFromFile(string filePath, object dataModel)
        {
            throw new NotImplementedException();
        }

        public string RenderFromString(string htmlTemplate, object dataModel)
        {
            //2)
            //Type type = dataModel.GetType();
            //Regex regex = new Regex(@"\$\{(.*?)\}");
            //string result = regex.Replace(htmlTemplate, (match) =>
            //{
            //    string fullPath = match.Groups[1].Value;
            //    string propertyName = fullPath.Split('.')[1];
            //    PropertyInfo property = type.GetProperty(propertyName);
            //    if (property != null)
            //    {
            //        var value = property.GetValue(dataModel);
            //        return value?.ToString() ?? "";
            //    }
            //    return match.Value;
            //});
            //return result;

            //3)
            //Type type = dataModel.GetType();
            //Regex regex = new Regex(@"\$\{(.*?)\}");
            //string result = regex.Replace(htmlTemplate, (match) =>
            //{
            //    string[] fullPath = match.Groups[1].Value.Split(".");
            //    //Console.WriteLine(fullPath);
            //    //foreach (string s in fullPath) Console.WriteLine(s);
            //    if (fullPath.Length == 3)
            //    {
            //        string propertyName1 = fullPath[1];
            //        string propertyName2 = fullPath[2];
            //        PropertyInfo property1 = type.GetProperty(propertyName1);
            //        if (property1 != null)
            //        {
            //            object property1Value = property1.GetValue(dataModel);
            //            if (property1Value != null)
            //            {
            //                Type property1Type = property1Value.GetType();
            //                PropertyInfo property2 = property1Type.GetProperty(propertyName2);
            //                if (property2 != null)
            //                {
            //                    var value = property2.GetValue(property1Value);
            //                    return value?.ToString() ?? "";
            //                }
            //            }
            //        }
            //        return match.Value;
            //    }
            //    else 
            //    {
            //        string propertyName = fullPath[1];
            //        PropertyInfo property = type.GetProperty(propertyName);
            //        if (property != null)
            //        {
            //            var value = property.GetValue(dataModel);
            //            return value?.ToString() ?? "";
            //        }
            //        return match.Value;
            //    }

            //});
            //return result;

            //4)
            //Type type = dataModel.GetType();
            //Regex regex = new Regex(@"\$if\s*\((?<field>\w+)\)(?<truePart>.*?)\$else(?<falsePart>.*)", RegexOptions.Singleline);
            //string result = regex.Replace(htmlTemplate, (match) =>
            //{
            //    string field = match.Groups["field"].Value;
            //    bool condition = false;
            //    PropertyInfo prop = type.GetProperty(field);
            //    if (prop != null && prop.PropertyType == typeof(bool))
            //    {
            //        condition = (bool)prop.GetValue(dataModel);
            //    }

            //    if (condition) return match.Groups["truePart"].Value;
            //    else return match.Groups["falsePart"].Value;
            //});
            //return result;

            //5)
            string result = Regex.Replace(htmlTemplate, @"\$foreach\s*\((?<type>\w+)\s+(?<varName>\w+)\s+in\s+(?<array>\w+)\)\s*\{(?<body>.*?)\}", (match) =>
            {
                string arrayName = match.Groups["array"].Value;
                string varName = match.Groups["varName"].Value;
                string body = match.Groups["body"].Value;
                Type arrayType = dataModel.GetType();
                PropertyInfo arrayProp = arrayType.GetProperty(arrayName);
                //Console.WriteLine(arrayProp.Name);
                if (arrayProp == null) return match.Value;
                
                object arrayObj = arrayProp.GetValue(dataModel);
                if (arrayObj is not IEnumerable enumerable) return match.Value;

                StringBuilder sb = new StringBuilder();
                foreach (var item in enumerable)
                {
                    string renderedItem = Regex.Replace(body, $@"{varName}\.(\w+)", (itemMatch) =>
                    {
                        string propName = itemMatch.Groups[1].Value;
                        PropertyInfo property = item.GetType().GetProperty(propName);
                        if (property == null) return itemMatch.Value;
                        object val = property.GetValue(item);
                        return val?.ToString() ?? "";
                    });
                    sb.Append(renderedItem);
                }
                
                return sb.ToString();
            });
            return result;
        }

        public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
        {
            throw new NotImplementedException();
        }
    }
}
