using System.Reflection;
using WebServer;

namespace ConsoleWebServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string websitePath = GetWebsitePath();
            Server.Start(websitePath);
            Console.ReadLine();
        }

        public static string GetWebsitePath()
        {
            // Path ou our exe.
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebSite");
        }
    }
}
