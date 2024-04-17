using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public const string ID = "Sever.ClientSession";
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"{ID}.OnConnected : {endPoint}");
            Thread.Sleep(5000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"{ID}.OnDisconnected : {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"{ID}.OnSend() Transferred bytes : {numOfBytes}");
        }
    }
}
