using System.Net.Http;
using System.Timers;

namespace Banlink.Utilities
{
    public class Uptime
    {
        public static void ContactUptimeKuma(object source, ElapsedEventArgs e)
        {
            var config = Configuration.ReadConfig(Banlink.ConfigPath);
            var kumaUrl = config.UptimeKuma;
            var url = kumaUrl + $"?ping={Banlink.Client.Ping}";

            var web = new HttpClient();

            var content = web.GetAsync(url).GetAwaiter().GetResult();
        }
    }
}