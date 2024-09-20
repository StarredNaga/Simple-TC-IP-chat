using System;
using System.Net;
using System.Threading;
using TCPServer.Models;

namespace TCPServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CancellationTokenSource cts = new();

            Server server = new(50005, IPAddress.Any, cts);

            server.Start();

            Console.ReadLine();

            cts.Cancel();
        }
    }
}
