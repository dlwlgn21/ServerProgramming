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
            // 손님이 보낸 메시지를 받는다.
            byte[] recvBuffer = new byte[1024];
            int recvBytes = clientSocket.Receive(recvBuffer); // 클라이언트에서 보내준 데이터는 recvBuffer에 저장이 됨.
            string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
            Console.WriteLine($"From Client : {recvData}");

            // 내가 메시지를 손님에게 보낸다.
            byte[] sendBuffer = Encoding.UTF8.GetBytes("Welcom To MMO RPG Server!");
            clientSocket.Send(sendBuffer);

            // 쫓아낸다.
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
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