using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Athena
{
    public class FileIO
    {
        /// <summary>
        /// Get the file name with no extension
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileNameFromPathWithoutExtension(string filePath)
        {
            //If there is no \ in the file path just return the original string
            if (!filePath.Contains(@"\"))
                return filePath;

            //Grab the part of the string after the last \
            var fileNameWithExtension = filePath.Split('\\').Last();

            //If there is no period (and thus no file extension) return the string
            if (!fileNameWithExtension.Contains(@"."))
                return fileNameWithExtension;

            //Split the file name and extension on the . character
            var periodSplit = fileNameWithExtension.Split('.');

            //Ideally the file name shouldn't have any periods in it except for the file extension one.
            if (periodSplit.Length == 2)
                return periodSplit.First();

            var fileNameBuilder = new StringBuilder();

            //However, handle stupid file paths here
            for (var i = 0; i < periodSplit.Length - 1; ++i)
            {
                fileNameBuilder.Append(periodSplit[i]);
            }

            return fileNameBuilder.ToString();
        }

        public static string GetFolderFromFilePathString(string filePath)
        {
            try
            {
                return GetFolderFromFilePathString(filePath);
            }
            catch (Exception e)
            {
                //TODO: Add exception
            }

            return string.Empty;
        }

        public static bool MoveFile(string sourceFolderPath, string fileName, string targetFolderPath)
        {
            try
            {
                return MoveFile(sourceFolderPath, fileName, targetFolderPath);
            }
            catch (Exception e)
            {

            }
            return false;
        }

        public static void MoveFileToUserDefinedFolder(string sourceFileFullPath)
        {
            //Open dialog and select jpg image
            var dialog = new Microsoft.Win32.SaveFileDialog(){AddExtension = true, DefaultExt = "csv"};
            //Display dialog
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                var fullName = dialog.FileName;
                File.Copy(sourceFileFullPath, fullName);
            }
        }

        public static void FileBackUp(string fileFullPath, string newFileFolderPath)
        {
            //Set date format
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            var currentTime = DateTime.Now;
            var fileName = Path.GetFileNameWithoutExtension(fileFullPath);
            
            //Load inventory csv file and create a backup copy
            var inventoryFileBackUpCopyName = //Constants.DataFolderPath + Constants.InventoryBackupFolderPath
                                              newFileFolderPath + fileName + currentTime.Day.ToString("00") + currentTime.Month.ToString("00") + 
                                              currentTime.Year.ToString("0000") +currentTime.Hour.ToString("00") + currentTime.Minute.ToString("00") + 
                                              currentTime.Second.ToString("00") + ".csv";

            File.Copy(fileFullPath, inventoryFileBackUpCopyName);
        }
    }
}
