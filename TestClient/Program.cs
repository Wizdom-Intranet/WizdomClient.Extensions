using Microsoft.Identity.Client;
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
            var client = new WizdomClient(new DeviceCodeTokenHandler(delegate (DeviceCodeResult deviceCodeResult) { Console.WriteLine($"Please open {deviceCodeResult.VerificationUrl} and input code: {deviceCodeResult.UserCode}"); }));
            var environment = await client.ConnectAsync();
            Console.WriteLine($"\nConnected to {environment.appUrl} running Wizdom v.{environment.wizdomVersion.ToString()} as {environment.currentPrincipal.loginName}\n");
            var items = await client.Noticeboard().GetItemsAsync();

            foreach (var item in items.data)
            {
                Console.WriteLine($"{item.created.ToString()} - {item.heading}");
            }

            Console.WriteLine("Done");
        }
    }
}
