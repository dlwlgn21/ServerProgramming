using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PacketGenerator
{
    internal class PacketFormat
    {
        // {0} 패킷 등록
        public static string sManagerFormat =
@"using ServerCore;
using System.Diagnostics;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {{
        get
        {{
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }}
    }}
    #endregion


    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecvMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handlerMap =   new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}
    }}
        
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort byteCount = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array!, buffer.Offset);
        byteCount += sizeof(ushort);
        ushort id = BitConverter.ToUInt16(buffer.Array!, buffer.Offset + byteCount);
        byteCount += sizeof(ushort);
        Action<PacketSession, ArraySegment<byte>> makePacketAction = null;
        if (_onRecvMap.TryGetValue(id, out makePacketAction))
            makePacketAction.Invoke(session, buffer);
        else
            Debug.Assert(false);
    }}

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> handlerAction = null;
        if (_handlerMap.TryGetValue(packet.Protocol, out handlerAction))
            handlerAction.Invoke(session, packet);
        else
            Debug.Assert(false);
    }}
}}
";

        // {0} 패킷 이름
        public static string sManagerRegisterFormat =
@"       _onRecvMap.Add((ushort)EPacketID.{0}, MakePacket<{0}>);
        _handlerMap.Add((ushort)EPacketID.{0}, PacketHandler.{0}Handler);
";



        // {0} 패킷 이름/번호목록
        // {1} 패킷 목록
        public static string sFileForamat =
@"using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using ServerCore;

public enum EPacketID
{{
    {0}
}}

interface IPacket
{{ 
	ushort Protocol {{ get; }}
	void Read(ArraySegment<byte> seg);
	ArraySegment<byte> WriteOrNull();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string sPacketEnumFormat =
@"{0} = {1},";



        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 read
        // {3} 멤버 변수 write
        public static string sPacketFormat =
@"
public class {0} : IPacket
{{
    {1}
	public ushort Protocol {{ get {{ return (ushort)EPacketID.{0}; }} }}
    
    public void Read(ArraySegment<byte> seg)
    {{
        ushort byteCount = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        byteCount += sizeof(ushort);
        byteCount += sizeof(ushort);
        {2}
    }}
    public ArraySegment<byte> WriteOrNull()
    {{
        ArraySegment<byte> openSeg = SendBufferHelper.OpenOrNull(4096);
        bool isSuccess = true;
        ushort byteCount = 0;
        Span<byte> span = new Span<byte>(openSeg.Array, openSeg.Offset, openSeg.Count);

        byteCount += sizeof(ushort);
        isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)EPacketID.{0});
        byteCount += sizeof(ushort);
        {3}
        isSuccess &= BitConverter.TryWriteBytes(span, byteCount);
        if (!isSuccess)
            return null;
        return SendBufferHelper.Close(byteCount);
    }}
}}
";
        // {0} 변수 형식
        // {1} 변수 이름
        public static string sMemberVariableFormat =
@"public {0} {1};";

        // {0} 리스트, 구조체 이름 [파스칼 표기법]
        // {1} 멤버 변수들 [파스칼 표기법]
        // {2} 멤버 변수 read
        // {3} 멤버 변수 write
        public static string sMemberVariableListFormat =
@"        
public class {0}
{{
    {1}
    public void Read(ReadOnlySpan<byte> span, ref ushort byteCount)
    {{
        {2}
    }}
    public bool TryWrite(Span<byte> span, ref ushort byteCount)
    {{
        bool isSuccess = true;
        {3}
        return isSuccess;
    }}
}}
public List<{0}> {0}s = new List<{0}>();
";




        // {0} 변수 이름
        // {1} To~ 변수 형식
        // {2} 변수 형식
        public static string sReadFormat =
@"this.{0} = BitConverter.{1}(span.Slice(byteCount, span.Length - byteCount));
byteCount += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string sReadByteFormat =
@"this.{0} = ({1})seg.Array[seg.Offset + byteCount];
byteCount += sizeof({1});";
        // {0} 변수 이름
        public static string sReadStringFormat =
@"ushort {0}Length = BitConverter.ToUInt16(span.Slice(byteCount, span.Length - byteCount));
byteCount += sizeof(ushort);
PlayerName = Encoding.Unicode.GetString(span.Slice(byteCount, {0}Length));
byteCount += {0}Length;";

        // {0} 구조체, 리스트 이름 [파스칼 표기법]
        // {1} 구조체, 리스트 이름 [카멜 표기법]
        public static string sReadListFormat =
@"this.{0}s.Clear();
ushort {0}Length = BitConverter.ToUInt16(span.Slice(byteCount, span.Length - byteCount));
byteCount += sizeof(ushort);
for (int i = 0; i < {0}Length; ++i)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref byteCount);
    {0}s.Add({1});
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string sWriteFormat =
@"isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), this.{0});
byteCount += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string sWrtieByteFormat =
@"openSeg.Array[openSeg.Offset + byteCount] = ({1})this.{0};
byteCount += sizeof({1});";

        // {0} 변수 이름
        public static string sWrtieStringFormat =
@"ushort {0}Length = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, openSeg.Array!, openSeg.Offset + byteCount + sizeof(ushort));
isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), {0}Length);
byteCount += sizeof(ushort);
byteCount += {0}Length;";

        // {0} 구조체, 리스트 이름 [파스칼 표기법]
        // {1} 구조체, 리스트 이름 [카멜 표기법]
        public static string sWriteListFormat =
@"isSuccess &= BitConverter.TryWriteBytes(span.Slice(byteCount, span.Length - byteCount), (ushort)this.{0}s.Count);
byteCount += sizeof(ushort);

foreach ({0} {1} in this.{0}s)
{{
    isSuccess &= {1}.TryWrite(span, ref byteCount);
}}";
    }
}
