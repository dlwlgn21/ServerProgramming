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
    public abstract class PacketSession : Session
    {
        public static readonly int HEADER_SIZE = 2;
        // [Size(2)][PacketID(2)]
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            int processLength = 0;
            Console.WriteLine($"PacketSession.OnRecv() ByteCount : {buffer.Count}");
            while (true)
            {
                // 최소한 헤더는 파싱할 수 있는지 확인해야 함.
                if (buffer.Count < HEADER_SIZE)
                    break;
                // 패킷이 완전체로 도착했는지 확인.
                ushort dataSize = BitConverter.ToUInt16(buffer.Array!, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // 여기까지 왔으면 어떻게든 패킷을 조립은 가능.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array!, buffer.Offset, dataSize));
                processLength += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array!, buffer.Offset + dataSize, buffer.Count - dataSize);
            }
            return processLength;
        }

        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket? _socket;
        int _disconnected = 0;
        object _lock = new object();

        RecvBuffer _recvBuffer = new RecvBuffer(1024);

        Queue<ArraySegment<byte>> _sendQ = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _sendPendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendAsyncEventArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvAsyncEventArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);


        public void Start(Socket socket)
        {
            _socket = socket;

            // 나중에 C++ 서버 만들떄도 이 부분과 비슷하게 만드니까 유심히 보라고함요
            _recvAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            RegisterRecv();

            _sendAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
        }
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;
            OnDisconnected(_socket!.RemoteEndPoint!);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public void Send(ArraySegment<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendQ.Enqueue(sendBuffer);
                if (_sendPendingList.Count == 0)
                    RegisterSend();
            }
        }
        #region NetworkComunication
        void RegisterSend()
        {
            while (_sendQ.Count > 0)
            {
                // ArraySegment -> 어떤 배열의 일부를 나타내는 구조체, 즉, 스택에 할당됨. 값이 복사되는 형태로 작동함.
                ArraySegment<byte> buffer = _sendQ.Dequeue();
                _sendPendingList.Add(buffer);
            }
            _sendAsyncEventArgs.BufferList = _sendPendingList;
            bool isPending = _socket!.SendAsync(_sendAsyncEventArgs);
            if (isPending == false)
                OnSendCompleted(null, _sendAsyncEventArgs);
        }

        void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendAsyncEventArgs.BufferList = null;
                        _sendPendingList.Clear();
                        OnSend(_sendAsyncEventArgs.BytesTransferred);
                        if (_sendQ.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }


        void RegisterRecv()
        {
            // Buffer Setting
            _recvBuffer.Clean();
            ArraySegment<byte> writeSegment = _recvBuffer.WriteSegment;
            _recvAsyncEventArgs.SetBuffer(writeSegment.Array, writeSegment.Offset, writeSegment.Count);


            bool isPending = _socket!.ReceiveAsync(_recvAsyncEventArgs);
            if (isPending == false)
                OnRecvCompleted(null, _recvAsyncEventArgs);
        }
        void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // Write Cursor를 이동시킨다.
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Debug.Assert(false, "Failed RecvBuffer.OnWrite()");
                        Disconnect();
                        return;
                    }
                    
                    // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다.
                    int processLength = OnRecv(_recvBuffer.ReadSegment);
                    if (processLength < 0 || processLength > _recvBuffer.DataSize)
                    {
                        Debug.Assert(false, "Failed Session.OnRecv()");
                        Disconnect();
                        return;
                    }

                    // 여기까지 왔다는건 데이터를 잘 받았다는 것
                    // Read Cursor 이동
                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Debug.Assert(false, "Failed RecvBuffer.OnRead()");
                        Disconnect();
                        return;
                    }

                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    #endregion
    }
}
