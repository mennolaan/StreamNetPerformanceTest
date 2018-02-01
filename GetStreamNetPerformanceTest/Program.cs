using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetStreamNetPerformanceTest
{
    public class ActivityStartLivePlay
    {
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string LivePlaylistId { get; set; }
        public string LivePlaylistAvatar { get; set; }
        public string LivePlaylistTitle { get; set; }
        public string LivePlaylistDescription { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var baseUrl = new Uri(string.Format("https://us-east-api.stream-io-api.com"));
           
            SlowWay.RequestsCount = FastWay.RequestsCount = 20;
            Stopwatch sw = new Stopwatch();

            Console.WriteLine("Testing Slow Way:");
            sw.Start();
            try
            {
                SlowWay.Fetch().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Http Request failed");
                Console.WriteLine(e.Message);
            }
            sw.Stop();
            TimeSpan slowWayTimeSpan = sw.Elapsed;

            Console.WriteLine();
            Console.WriteLine("Testing Fast Way:");
            sw.Restart();
            try
            {
                FastWay.Fetch().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine("Http Request failed");
                Console.WriteLine(e.Message);

            }
            sw.Stop();
            TimeSpan fastWayTimeSpan = sw.Elapsed;

            Console.WriteLine();
            Console.WriteLine($"Slow Way Completed in {slowWayTimeSpan}");
            Console.WriteLine($"Fast Way Completed in {fastWayTimeSpan}");

            Console.ReadKey();
        }
    }
}
