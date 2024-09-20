using Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPServer.Models;

public class Server
{
    public Server(int port, IPAddress address, CancellationTokenSource cts)
    {
        _server = new(address, port);
        _cts = cts;
        _clients = new List<Client>();
    }

    private TcpListener _server;
    private List<Client> _clients = null!;
    private CancellationTokenSource _cts;

    public void Start()
    {
        _server.Start();

        Console.WriteLine("Server start");

        while (!_cts.IsCancellationRequested)
        {
            var user = _server.AcceptTcpClient();

            ClientValidation(user);
        }

        Stop();
    }

    private void ClientValidation(TcpClient user)
    {
        Client client = new(user);

        var stream = client.GetStream();

        var name = ReceiveMessage(stream);

        if (!name.Contains("Name:"))
        {
            Console.WriteLine("Access denide");

            return;
        }

        name = name.Split(':')[1];

        client.SetName(name);

        var id = Guid.NewGuid().ToString();

        client.SetId(id);

        SendMessage($"Id:{id}", stream);

        _clients.Add(client);

        Console.WriteLine($"{client.GetName()} was connected");

        BroadcastMessage($"{client.GetName()} was connected", client.GetId());

        Task.Factory.StartNew(() => ClientHandler(client));
    }

    public async void ClientHandler(Client client)
    {
        var stream = client.GetStream();

        await Task.Run(() => 
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var message = ReceiveMessage(stream);

                    if (message == "END") break;

                    Console.WriteLine($"{client.GetName()}: {message}");

                    BroadcastMessage($"{client.GetName()}: {message}", client.GetId());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{client.GetName()} was disconnected");

                BroadcastMessage($"{client.GetName()} was disconnected", client.GetId());
            }
            finally
            {
                Console.WriteLine($"{client.GetName()} was disconnected");

                BroadcastMessage($"{client.GetName()} was disconnected", client.GetId());

                _clients.Remove(client);
            }
        });
    }

    public void BroadcastMessage(string message, string id)
    {
        foreach (var client in _clients)
        {
            if(client.GetId() != id)
            {
                var stream = client.GetStream();

                SendMessage(message, stream);
            }
        }
    }

    public string ReceiveMessage(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int size = 0;

        size = stream.Read(buffer, 0, buffer.Length);

        string message = Encoding.UTF8.GetString(buffer, 0, size);

        return message;
    }


    public void SendMessage(string message, NetworkStream stream)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);

        stream.Write(buffer, 0, buffer.Length);
    }

    public void Stop()
    {
        foreach (var client in _clients)
        {
            client.Disconnect();

            _clients.Remove(client);
        }

        _server.Stop();
    }
}