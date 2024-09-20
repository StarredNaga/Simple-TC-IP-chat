using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Models;

public class Client
{
    public Client(TcpClient client)
    {
        _client = client;

        if(_client.Connected) _stream = client.GetStream();
    }

    private TcpClient _client;
    private NetworkStream _stream;
    private string _name;
    private string _id;

    public string GetName() => _name;

    public string GetId() => _id;

    public NetworkStream GetStream() => _stream;

    public void SetName(string name) => _name = name;

    public void SetId(string id) => _id = id;

    public void Connect(string name, int port, IPAddress address)
    {
        _name = name;

        _client.Connect(address, port);

        _stream = _client.GetStream();

        TryConnect();
    }

    private void TryConnect()
    {
        SendMessage($"Name:{_name}");

        string id = ReceiveMessage();

        if (!id.Contains("Id:"))
        {
            Console.WriteLine("Server acces denide!");

            Disconnect();

            return;
        }

        id = id.Split(':')[1];

        _id = id;

        Console.WriteLine("Connection succes");
    }

    public void SendMessage(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);

        _stream.Write(buffer, 0, buffer.Length);
    }

    public string ReceiveMessage()
    {
        byte[] buffer = new byte[1024];
        int size = 0;

        size = _stream.Read(buffer, 0, buffer.Length);

        string message = Encoding.UTF8.GetString(buffer,0, size);

        return message;
    }

    public void Disconnect()
    {
        _client.Close();
        _client.Dispose();
    }
}