using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // Server끼리 Communication을 하기 위해서도 Connecter는 필수로 필요하게 됨.
    public class Connecter
    {
        Func<Session>? _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            // 휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;

            // 상대방 주소 넣어주는 것
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;
            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Debug.Assert(args != null && args.UserToken != null);
            Socket socket = args.UserToken as Socket;
            if (socket == null)
            {
                Debug.Assert(false);
                return;
            }

            bool isPending = socket.ConnectAsync(args);
            if (isPending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory!.Invoke();
                session.Start(args.ConnectSocket!);
                session.OnConnected(args.RemoteEndPoint!);

            }
            else
            {
                Console.WriteLine($"OnConnectCompleted failed {args.SocketError}");
            }
        }



    }
}
