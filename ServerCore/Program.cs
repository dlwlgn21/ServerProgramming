using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ServerCore;
using System.Security.Cryptography;

internal class Program
{
    static Listener sListener = new Listener();
    static void OnAcceptHandler(Socket clientSocket)
    {
        try
        {
            // 내가 메시지를 손님에게 보낸다.
            Session session = new Session();
            session.Start(clientSocket);
            byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcom To MMO RPG Server!");
            session.Send(sendBuffer);

            Thread.Sleep(1000);

            session.Disconnect();
            session.Disconnect();

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
    private static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAdrr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdrr, 7777);

        sListener.Init(endPoint, OnAcceptHandler);
        Console.WriteLine("Listening...");
        while (true)
        {

        }
    }
}