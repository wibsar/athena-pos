using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Athena
{
    public class Logger
    {
        #region Fields

        private static Logger _log = null;
        private string _logFilePath;

        #endregion

        #region Properties

        public string LogFilePath
        {
            get { return _logFilePath; }
            set { _logFilePath = value; }
        }

        #endregion

        #region Constructor

        private Logger(string filePath)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
            //Read inventory CSV format
            LogFilePath = filePath;
        }

        public static Logger GetInstance(string filePath)
        {
            if (_log == null)
                _log = new Logger(filePath);
            return _log;
        }

        #endregion

        #region Methods

        public void Write(string username, string location, string message)
        {
            //Get date time
            var time = DateTime.Now.ToString("G");
            string data = string.Format("{0}{1}{2}{3}", time.PadRight(30), username.PadRight(25), location.PadRight(80), message) + Environment.NewLine;

            File.AppendAllText(LogFilePath, data);
        }

        #endregion
    }
}
