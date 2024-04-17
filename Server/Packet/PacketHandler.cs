using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PacketHandler
{

    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket iPacket)
    {
        C_PlayerInfoReq packet = iPacket as C_PlayerInfoReq;
        Debug.Assert(packet != null);
        Console.WriteLine($"PacketHandler.PlayerInfoReqHandler() PlayerInfoReq : {packet.PlayerID}, Name = {packet.PlayerName}");

        foreach (C_PlayerInfoReq.Skill skill in packet.Skills)
        {
            Console.WriteLine($"Skill ID: {skill.Id}, Level : {skill.Level}, Duration : {skill.Duration}");
        }
    }
}
