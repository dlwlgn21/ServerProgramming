using ServerCore;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Server
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
            ushort byteCount = 0;
            ushort size = BitConverter.ToUInt16(buffer.Array!, buffer.Offset);
            byteCount += sizeof(ushort);
            ushort id = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + byteCount);
            byteCount += sizeof(ushort);
            switch ((EPacketID) id)
            {
                case EPacketID.PlayerInfoReq:
                    {
                        PlayerInfoReq p = new PlayerInfoReq();
                        p.Read(buffer);
                        Console.WriteLine($"{ID}.OnRecvPacket() PlayerInfoReq : {p.PlayerID}, Name = {p.PlayerName}");

                        foreach (PlayerInfoReq.Skill skill in p.Skills)
                        {
                            Console.WriteLine($"Skill ID: {skill.Id}, Level : {skill.Level}, Duration : {skill.Duration}");
                        }
                        
                        
                        break;

                    }
                case EPacketID.PlayerInfoOK:
                    break;
            }
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"{ID}.OnSend() Transferred bytes : {numOfBytes}");
        }
    }
}
