using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

// DNS (Domain Name System)
string host = Dns.GetHostName();
IPHostEntry ipHost = Dns.GetHostEntry(host);
IPAddress ipAdrr = ipHost.AddressList[0];
IPEndPoint endPoint = new IPEndPoint(ipAdrr, 7777);


while (true)
{
    // 휴대폰 설정
    Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    
    try
    {
        // 문지기에게 입장 문의 
        socket.Connect(endPoint);
        Debug.Assert(socket != null && socket.RemoteEndPoint != null);
        Console.WriteLine($"Conectted To {socket.RemoteEndPoint.ToString()}");

        // 보낸다

        byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello Sevver World!!");
        int sendBytes = socket.Send(sendBuffer);

        // 받는다.
        byte[] recvBuffer = new byte[1024];
        int recvBytes = socket.Receive(recvBuffer);
        string recvData = Encoding.UTF8.GetString(recvBuffer, 0, recvBytes);
        Console.WriteLine($"[From Server] : {recvData}");

        // 나간다.
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
    catch (Exception e)
    {
        Console.WriteLine(e.ToString());
    }
    Thread.Sleep(500);
}

