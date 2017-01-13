using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using CinegyType.RSS.Controller.Model;

namespace CinegyType.RSS.Controller
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var url = ConfigurationManager.AppSettings.Get("RSS_Url");
                var startPubDateStr = ConfigurationManager.AppSettings.Get("RSS_StartPubDate");

                var formatDateStr = ConfigurationManager.AppSettings.Get("RSS_DateTimeFormat");
                

                var output = ConfigurationManager.AppSettings.Get("Template_File");

                var variableName = ConfigurationManager.AppSettings.Get("Target_TextVariable");

                var interval = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Update_Interval"));

                var host = ConfigurationManager.AppSettings.Get("Playout_Hostname");
                var instance = Convert.ToInt32(ConfigurationManager.AppSettings.Get("Playout_Instance"));

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("     Input parameters ----------  ");
                Console.WriteLine("RSS_Url: {0}", url);
                Console.WriteLine("RSS_StartPubDate: {0}", startPubDateStr);
                Console.WriteLine("RSS_DateTimeFormat: {0}", formatDateStr);
                Console.WriteLine("Template_File: {0}", output);
                Console.WriteLine("Target_TextVariable: {0}", variableName);
                Console.WriteLine("Update_Interval: {0}", interval);
                Console.WriteLine("Playout_Hostname: {0}", host);
                Console.WriteLine("Playout_Instance: {0}", instance);
                Console.WriteLine();
                Console.ResetColor();

                #region Validating arguments

                if (string.IsNullOrEmpty(url)) throw new ArgumentException("Empty RSS-feed url");

                if (string.IsNullOrEmpty(output)) throw new ArgumentException("Empty template file url");

                if (string.IsNullOrEmpty(formatDateStr)) throw new ArgumentException("Empty DateTime format");

                if (!File.Exists(output)) throw new ArgumentException("Template file not exists");

                if (interval < 0) throw new ArgumentException("Update interval is out of range");

                if (string.IsNullOrEmpty(host)) throw new ArgumentException("Empty playout hostname");

                if (interval < 0) throw new ArgumentException("Instance number is out of range");

                #endregion

                DateTime startPubDate;

                try
                {
                    startPubDate = DateTime.ParseExact(startPubDateStr,
                        formatDateStr,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error datetime format. Apply default start time. {0}", e.Message);
                    startPubDate = DateTime.Now.AddDays(-1);
                }

                var rssSettings = new RssSettings { Url = url, StartPubDate = startPubDate, DateTimeFormat = formatDateStr};
                var playout = new PlayoutSettings { Id = instance, Hostname = host };
                var settings = new Settings(rssSettings, output, variableName, playout);
                var controller = new Controller(settings, interval);

                while (Console.KeyAvailable) Console.ReadKey(true);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press 'Escape' to exit.");
                Console.WriteLine();
                Console.ResetColor();

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) { /* ignore other key */}

                controller.Dispose();

            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed with exception: {0}" + exception.Message);
                Console.ReadLine();
            }
        }
    }
}
