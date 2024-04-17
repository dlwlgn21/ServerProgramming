using System.Diagnostics;
using System.Net;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;
using ServerCore;
using Server;

internal class Program
{
    static Listener sListener = new Listener();

    private static void Main(string[] args)
    {
        PacketManager.Instance.Register();

        // DNS (Domain Name System)
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAdrr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAdrr, 7777);

        sListener.Init(endPoint, () => { return new ClientSession(); });
        Console.WriteLine("Listening...");
        while (true)
        {
            ;
        }
    }
}