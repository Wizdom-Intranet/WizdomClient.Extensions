using System;
using Wizdom.Client;
using Wizdom.Client.Extensions;

namespace TestClient
{
    class Program
    {
        private static string token { get; set; }
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var client = new WizdomClient(new DeviceCodeTokenHandler());
            var e = await client.ConnectAsync();
            Console.WriteLine(e.spHostURL);
            var items = await client.Noticeboard().GetItemsAsync();

            foreach (var item in items.data)
            {
                Console.WriteLine($"{item.created.ToString()} - {item.heading}");
            }

            Console.WriteLine("Done");
        }
    }
}
