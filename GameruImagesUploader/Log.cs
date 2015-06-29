using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameruImagesUploader
{
    // Simply logger
    public sealed class Log
    {
        private static string logFilePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\log.txt";

        // Add log message
        public static void Add(String message)
        {
            if (Properties.Settings.Default.Logging == true)
            {
                message = String.Format("{0} {1} - {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), message + Environment.NewLine);
                try
                {
                    File.AppendAllText(logFilePath, message);
                }
                catch { }
            }
        }

        public static void Add(Exception exception)
        {
            var message = exception.ToString();
            Add(message);
        }

        // Delete log file
        public static void Clear()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }
            catch { }
        }
    }
}
