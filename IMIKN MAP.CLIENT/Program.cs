using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace IMIKN_MAP.CLIENT
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient client = new WebClient();
            string downloadString = client.DownloadString("http://localhost:19787/files/events.json");
            var events = JArray.Parse(downloadString);
            Console.WriteLine(events);
        }
    }
}
