using ServerCore;
using System.Diagnostics;

class PacketManager
{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion


    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecvMap = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handlerMap =   new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
       _onRecvMap.Add((ushort)EPacketID.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>);
        _handlerMap.Add((ushort)EPacketID.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler);


    }
        
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
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
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T packet = new T();
        packet.Read(buffer);

        Action<PacketSession, IPacket> handlerAction = null;
        if (_handlerMap.TryGetValue(packet.Protocol, out handlerAction))
            handlerAction.Invoke(session, packet);
        else
            Debug.Assert(false);
    }
}
