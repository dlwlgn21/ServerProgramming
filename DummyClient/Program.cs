using DummyClient;
using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;



internal class Program
{
    private static void Main(string[] args)
    {
        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAdrr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdrr, 7777);

        Connecter connecter = new Connecter();
        connecter.Connect(endPoint, () => { return new ServerSession(); });

        while (true)
        {
            //휴대폰 설정
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Thread.Sleep(500);
        }
    }
}
