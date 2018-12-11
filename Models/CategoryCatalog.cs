using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Athena
{
    public class CategoryCatalog
    {
        //Load category list from file
        public static List<string> categories {get; set;}

        public CategoryCatalog(string filePath)
        {
            //Read file
            try
            {
                var lines = File.ReadAllLines(filePath);
                categories = lines.ToList();               
            }
            catch (Exception e)
            {
                var lines = new List<string>();
                lines.Add("Varios");
                categories = lines.ToList();
            }
        }

        public static List<string> GetList(string filePath)
        {
            //Read file
            try
            {
                var lines = File.ReadAllLines(filePath);
                categories = lines.ToList();
                return categories;
            }
            catch (Exception e)
            {
                var lines = new List<string>();
                lines.Add("Varios");
                categories = lines.ToList();
                return categories;
            }
        }

        /// <summary>
        /// Update category list file with new changes
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool UpdateCategoryListFile(string filePath, List<string> categories)
        {
            //Creates or overwrites file
            StreamWriter writer = File.CreateText(filePath);
            //Write code for each item
            foreach (var category in categories)
            {
                writer.WriteLine(category);
            }
            writer.Close();
            writer.Dispose();

            return true;
        }

    }
}
