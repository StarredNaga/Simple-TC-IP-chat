using Models;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TCPClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Client client = new(new());
            CancellationTokenSource cts = new CancellationTokenSource();

            Console.WriteLine("Enter name");

            var name = Console.ReadLine();

            client.Connect(name, 50005, IPAddress.Parse("127.0.0.1"));

            Task.Factory.StartNew(() => MessageHandler(cts, client));

            while(true)
            {
                var message = Console.ReadLine();

                if (message == "exit") break;

                client.SendMessage(message);
            }

            client.SendMessage("END");

            cts.Cancel();
            client.Disconnect();
        }

        public static async void MessageHandler(CancellationTokenSource cts, Client client)
        {
            await Task.Run(() =>
            {
                while(!cts.IsCancellationRequested)
                {
                    Console.WriteLine(client.ReceiveMessage());
                }
            });
        }
    }
}
