using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Athena
{
    public class Formatter
    {
        /// <summary>
        /// Remove commas from string input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            //TODO: What happens if there is no ,
            return input.Replace(",", ".");
        }

        public static string RemoveInvalidCharacters(string input, out bool foundInvalidCharacter)
        {
            var initialInput = input;
            if(input != null)
            {
                input = input.Replace(",", "");
                input = input.Replace(";", "");
                input = input.Replace("'", "");
                input = input.Replace(@"\", "");
                input = input.Replace(@"/", "");
                input = input.Replace("?", "");
                input = input.Replace("!", "");
                input = input.Replace("&", "");
                input = input.Replace("*", "");
                input = input.Replace("#", "");
                input = input.Replace("%", "");
                input = input.Replace("^", "");
                input = input.Replace("(", "");
                input = input.Replace(")", "");
                input = input.Replace("}", "");
                input = input.Replace("]", "");
                input = input.Replace("[", "");
                input = input.Replace("{", "");
                input = input.Replace("|", "");
                input = input.Replace("=", "");
                input = input.Replace("+", "");
                input = input.Replace("~", "");
                input = input.Replace("`", "");
                input = input.Replace(":", "");
                input = input.Replace("$", "");
                input = input.Replace("\"", "");
                input = input.Replace("<", "");
                input = input.Replace(">", "");
            };

            foundInvalidCharacter = input != initialInput;

            return input;
        }

        public static string RemoveWhiteSpace(string input, out bool foundInvalidCharacter)
        {
            var initialInput = input;
            if (input != null)
            {
                input = input.Replace(" ", "");
            }
            foundInvalidCharacter = input != initialInput;
            return input;
        }

        public static object SanitizeInput(object input, Type type)
        {
            //get all properties
            var propertyInfo = type.GetProperties();
            
            //convert to type
            var inputType = Convert.ChangeType(input, type);
            foreach (var property in propertyInfo)
            {
                int y = 1;
                if (property.PropertyType.Name == "String")
                {
                    y=2;
                }
                //contains value of property
                var x = (property.GetValue(input));
                //find same property in inputType
                var xv = property.Name;
                
            }
            return input;
        }

        public static List<string> BreakDownString(string input, int desiredSize)
        {
            var strings = new List<string>();
            //Get string size
            var size = input.Length;
            int length = 0;
 
            for(var index = 0; index < size; index = index + desiredSize)
            {
                length = (size - index) < desiredSize ? (size - index) : desiredSize;
                strings.Add(input.Substring(index, length));
            }
            return strings;
        }

        public static string FirstLetterUpperConverter(string input)
        {
            var newCategory = input.ToString().ToCharArray();
            string formattedCategory = null;
            string newString = "";
            try
            {
                var upperLetter = char.ToUpper(newCategory[0]);
                newCategory[0] = upperLetter;
                formattedCategory = new string(newCategory);
            }
            catch (Exception e)
            {

            }
            if (formattedCategory != null)
                newString = formattedCategory;

            return newString;
        }
    }
}
