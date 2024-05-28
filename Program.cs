using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;

namespace PT_Lab12;

[Serializable]
public class DataObject
{
    public int Value { get; set; }
}

public class Server
{
    private UdpClient _server;
    private IPEndPoint _clientEndPoint;

    public Server(int port)
    {
        _server = new UdpClient(port);
        Console.WriteLine("Server started on port " + port);
        Listen();
    }

    private Task<DataObject> ModifyData(DataObject data)
    {
        return Task.Run(() =>
        {
            data.Value *= 2;
            Console.WriteLine("Calculating...");
            Thread.Sleep(1000);
            return data;
        });
    }

    public void Listen()
    {
        _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            try
            {
                byte[] data = _server.Receive(ref _clientEndPoint);
                string jsonData = Encoding.UTF8.GetString(data);
                DataObject receivedData = JsonSerializer.Deserialize<DataObject>(jsonData);
                Console.WriteLine("Received object with value: " + receivedData.Value + " from " + _clientEndPoint.Address + ":" + _clientEndPoint.Port);

                DataObject modifiedData = ModifyData(receivedData).Result;
                Console.WriteLine("Modified object to value: " + modifiedData.Value);

                string responseJson = JsonSerializer.Serialize(receivedData);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
                _server.Send(responseBytes, responseBytes.Length, _clientEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public static void Main(string[] args)
    {
        new Server(12345);
    }
}