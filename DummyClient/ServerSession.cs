using ServerCore;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DummyClient
{


    public class ServerSession : ServerCore.Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Client.ServerSession.OnConnected : {endPoint}");
            C_PlayerInfoReq packet = new C_PlayerInfoReq() { PlayerID = 1001, PlayerName  = "Lee"};
            var skill = new C_PlayerInfoReq.Skill() { Id = 101, Level = 10, Duration = 5.0f };
            skill.Attributes.Add(new C_PlayerInfoReq.Skill.Attribute() { Att = 77 }); 
            packet.Skills.Add(skill);
            packet.Skills.Add(new C_PlayerInfoReq.Skill() { Id = 102, Level = 5, Duration = 3.0f});
            packet.Skills.Add(new C_PlayerInfoReq.Skill() { Id = 103, Level = 2, Duration = 2.0f});
            packet.Skills.Add(new C_PlayerInfoReq.Skill() { Id = 104, Level = 1, Duration = 1.0f});

            ArraySegment<byte> seg = packet.WriteOrNull();
            if (seg != null)
                Send(seg);
            else
                Debug.Assert(false, "PlayerInfoReq.WrtieOrNull Failed!");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"Client.ServerSession.OnDisconnected : {endPoint}");
        }


        // 이동 패킷 (3,2) 좌표로 이동하고 싶다!
        // 15 3 2 
        public override int OnRecv(ArraySegment<byte> buffer)
        {
            Debug.Assert(buffer.Array != null);
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"Client.ServerSession.[From Server] {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Client.ServerSession.Transferred bytes : {numOfBytes}");
        }
    }
}
