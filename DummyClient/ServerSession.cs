using ServerCore;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace DummyClient
{

    public class PlayerInfoReq
    {
        public long PlayerID;
        public string PlayerName;

        public struct Skill
        {
            public int Id;
            public short Level;
            public float Duration;
            public void Read(ReadOnlySpan<byte> span, ref ushort byteCount)
            {
                this.Id = BitConverter.ToInt32(span.Slice(byteCount, span.Length - byteCount));
                byteCount += sizeof(int);
                this.Level = BitConverter.ToInt16(span.Slice(byteCount, span.Length - byteCount));
                byteCount += sizeof(short);
                this.Duration = BitConverter.ToSingle(span.Slice(byteCount, span.Length - byteCount));
                byteCount += sizeof(float);
            }
            public bool TryWrite(Span<byte> span, ref ushort byteCount)
            {
                bool isSuccess = true;
                isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.Id);
                byteCount += sizeof(int);
                isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.Level);
                byteCount += sizeof(short);
                isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.Duration);
                byteCount += sizeof(float);
                return isSuccess;
            }
        }
        public List<Skill> Skills = new List<Skill>();


        public void Read(ArraySegment<byte> seg)
        {
            ushort byteCount = 0;

            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
            byteCount += sizeof(ushort);
            byteCount += sizeof(ushort);
            this.PlayerID = BitConverter.ToInt64(span.Slice(byteCount, span.Length - byteCount));
            byteCount += sizeof(long);
            ushort PlayerNameLength = BitConverter.ToUInt16(span.Slice(byteCount, span.Length - byteCount));
            byteCount += sizeof(ushort);
            PlayerName = Encoding.Unicode.GetString(span.Slice(byteCount, PlayerNameLength));
            byteCount += PlayerNameLength;
            this.Skills.Clear();
            ushort SkillLength = BitConverter.ToUInt16(span.Slice(byteCount, span.Length - byteCount));
            byteCount += sizeof(ushort);
            for (int i = 0; i < SkillLength; ++i)
            {
                Skill skill = new Skill();
                skill.Read(span, ref byteCount);
                Skills.Add(skill);
            }
        }
        public ArraySegment<byte> WriteOrNull()
        {
            ArraySegment<byte> openSeg = SendBufferHelper.OpenOrNull(4096);
            bool isSuccess = true;
            ushort byteCount = 0;
            Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

            byteCount += sizeof(ushort);
            isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)EPacketID.PlayerInfoReq);
            byteCount += sizeof(ushort);
            isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.PlayerID);
            byteCount += sizeof(long);
            ushort PlayerNameLength = (ushort)Encoding.Unicode.GetBytes(this.PlayerName, 0, this.PlayerName.Length, openSeg.Array!, openSeg.Offset + byteCount + sizeof(ushort));
            isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), PlayerNameLength);
            byteCount += sizeof(ushort);
            byteCount += PlayerNameLength;
            isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)this.Skills.Count);
            byteCount += sizeof(ushort);

            foreach (Skill skill in this.Skills)
            {
                isSuccess &= skill.TryWrite(span, ref byteCount);
            }
            isSuccess &= BitConverter.TryWriteBytes(span, byteCount);
            if (!isSuccess)
                return null;
            return SendBufferHelper.Close(byteCount);
        }
    }


    public enum EPacketID
    {
        PlayerInfoReq = 1,
        PlayerInfoOK = 2,
    }
    public class ServerSession : ServerCore.Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Client.ServerSession.OnConnected : {endPoint}");
            PlayerInfoReq packet = new PlayerInfoReq() { PlayerID = 1001, PlayerName  = "Lee"};
            packet.Skills.Add(new PlayerInfoReq.Skill() { Id = 101, Level = 10, Duration = 5.0f});
            packet.Skills.Add(new PlayerInfoReq.Skill() { Id = 102, Level = 5, Duration = 3.0f});
            packet.Skills.Add(new PlayerInfoReq.Skill() { Id = 103, Level = 2, Duration = 2.0f});
            packet.Skills.Add(new PlayerInfoReq.Skill() { Id = 104, Level = 1, Duration = 1.0f});

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
