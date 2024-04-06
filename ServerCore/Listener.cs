using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        public void Init(IPEndPoint endPoint)
        {
            // 문지기(정확히 말하면 문지기가 들고 있는 핸드폰 만들어 주는 것.)
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 문지기 교육(문지기 핸드폰에 우리가 찾은 주소 연동시켜줌)
            _listenSocket.Bind(endPoint);

            // 영업시작
            _listenSocket.Listen(10); // Backlog == 최대 대기수
        }

        // 손님을 입장시킴 즉, 대리인(Session)을 위한 핸드폰(Socket) 하나 만들어줌.
        public Socket Accept()
        {
            return _listenSocket.Accept();
        }
    }
}
