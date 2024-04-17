using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using ServerCore;

public enum EPacketID
{
    C_PlayerInfoReq = 1,
	S_Test = 2,
	
}

interface IPacket
{ 
	ushort Protocol { get; }
	void Read(ArraySegment<byte> seg);
	ArraySegment<byte> WriteOrNull();
}


public class C_PlayerInfoReq : IPacket
{
    public byte TestByte;
	public long PlayerID;
	public string PlayerName;
	        
	public class Skill
	{
	    public int Id;
		public short Level;
		public float Duration;
		        
		public class Attribute
		{
		    public int Att;
		    public void Read(ReadOnlySpan<byte> span, ref ushort byteCount)
		    {
		        this.Att = BitConverter.ToInt32(span.Slice(byteCount, span.Length - byteCount));
				byteCount += sizeof(int);
		    }
		    public bool TryWrite(Span<byte> span, ref ushort byteCount)
		    {
		        bool isSuccess = true;
		        isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.Att);
				byteCount += sizeof(int);
		        return isSuccess;
		    }
		}
		public List<Attribute> Attributes = new List<Attribute>();
		
	    public void Read(ReadOnlySpan<byte> span, ref ushort byteCount)
	    {
	        this.Id = BitConverter.ToInt32(span.Slice(byteCount, span.Length - byteCount));
			byteCount += sizeof(int);
			this.Level = BitConverter.ToInt16(span.Slice(byteCount, span.Length - byteCount));
			byteCount += sizeof(short);
			this.Duration = BitConverter.ToSingle(span.Slice(byteCount, span.Length - byteCount));
			byteCount += sizeof(float);
			this.Attributes.Clear();
			ushort AttributeLength = BitConverter.ToUInt16(span.Slice(byteCount, span.Length - byteCount));
			byteCount += sizeof(ushort);
			for (int i = 0; i < AttributeLength; ++i)
			{
			    Attribute attribute = new Attribute();
			    attribute.Read(span, ref byteCount);
			    Attributes.Add(attribute);
			}
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
			isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)this.Attributes.Count);
			byteCount += sizeof(ushort);
			
			foreach (Attribute attribute in this.Attributes)
			{
			    isSuccess &= attribute.TryWrite(span, ref byteCount);
			}
	        return isSuccess;
	    }
	}
	public List<Skill> Skills = new List<Skill>();
	
	public ushort Protocol { get { return (ushort)EPacketID.C_PlayerInfoReq; } }
    
    public void Read(ArraySegment<byte> seg)
    {
        ushort byteCount = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        byteCount += sizeof(ushort);
        byteCount += sizeof(ushort);
        this.TestByte = (byte)seg.Array[seg.Offset + byteCount];
		byteCount += sizeof(byte);
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
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)EPacketID.C_PlayerInfoReq);
        byteCount += sizeof(ushort);
        openSeg.Array[openSeg.Offset + byteCount] = (byte)this.TestByte;
		byteCount += sizeof(byte);
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

public class S_Test : IPacket
{
    public int TestInt;
	public ushort Protocol { get { return (ushort)EPacketID.S_Test; } }
    
    public void Read(ArraySegment<byte> seg)
    {
        ushort byteCount = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        byteCount += sizeof(ushort);
        byteCount += sizeof(ushort);
        this.TestInt = BitConverter.ToInt32(span.Slice(byteCount, span.Length - byteCount));
		byteCount += sizeof(int);
    }
    public ArraySegment<byte> WriteOrNull()
    {
        ArraySegment<byte> openSeg = SendBufferHelper.OpenOrNull(4096);
        bool isSuccess = true;
        ushort byteCount = 0;
        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

        byteCount += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)EPacketID.S_Test);
        byteCount += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.TestInt);
		byteCount += sizeof(int);
        isSuccess &= BitConverter.TryWriteBytes(span, byteCount);
        if (!isSuccess)
            return null;
        return SendBufferHelper.Close(byteCount);
    }
}

