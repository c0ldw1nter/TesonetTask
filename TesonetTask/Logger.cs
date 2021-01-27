using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TesonetTask
{
    public class Logger
    {
        public static string LogLocation { get; set; }

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(LogLocation, true))
                {
                    sw.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {message}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
