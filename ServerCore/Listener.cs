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
    // Server 입장에서 연결을 기다리는 녀석
    public class Listener
    {
        Socket? _listenSocket;
        Func<Session>? _sessionFactory; // Action<T>와는 인자는 안받고 T를 return 해줄 수 있음.
        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            // 문지기(정확히 말하면 문지기가 들고 있는 핸드폰 만들어 주는 것.)
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;


            // 문지기 교육(문지기 핸드폰에 우리가 찾은 주소 연동시켜줌)
            _listenSocket.Bind(endPoint);

            // 영업시작
            _listenSocket.Listen(10); // Backlog == 최대 대기수

            // Client에서 Connect 요청이오면 OnAcceptCompleted가 콜백됨.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        
        }

        // 비동기, 즉 동시에 처리되지 않고 비동기로 처리함.
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // SocketAsyncEventArgs를 재사용하기 때문에 깨끗이 밀어줘야 한다. 
            args.AcceptSocket = null;

            // 당장 완료한다는 보장은 없음. 다만 요청을 할 뿐. 요기가 계속 호출되는 부분.
            bool isPending = _listenSocket!.AcceptAsync(args);

            // 만약에 isPending이 계속 무한대로 false가 되면 어떻게 될까??
            // StackOverFlow가 발생하지 않을까?? 함수가 서로 물고있으니까!
            // 하지만 isPending이 계속 false가 뜨는 경우가 현실적으로 일어날 수 없다고 함.
            // Init()->_listenSocket.Listen(10); // Backlog == 최대 대기수 이곳에서도 이미 방어코드가 작성되어 있음!
            if (isPending == false)
                OnAcceptCompleted(null, args);
        }

        // 이 함수는 RedZone!! 멀티쓰레딩에 안전하게 코드를 짜야한다.
        void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // 손님을 입장시킴 즉, 대리인(Session)을 위한 핸드폰(Socket) 하나 만들어줌.
                Session session = _sessionFactory!.Invoke();
                session.Start(args.AcceptSocket!);
                session.OnConnected(args.AcceptSocket!.RemoteEndPoint!);
            }
            else
                Console.WriteLine(args.SocketError.ToString());
            RegisterAccept(args); // 모든 일이 끝난 다음번 client를 위해서 또 한번 등록을 해줌.
        }
    }
}
