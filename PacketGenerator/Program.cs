using System.Diagnostics;
using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        public static string genPackets;
        public static ushort packetID = 0;
        public static string packetEnums;
        public static string serverRegister;
        public static string clientRegister;
        static void Main(string[] args)
        {
            string pdlFilePath = "../../PDL.xml";
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            if (args.Length >= 1)
                pdlFilePath = args[0];

            using (XmlReader reader = XmlReader.Create(pdlFilePath, settings))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                    {
                        ParsePacket(reader);
                    }
                    Console.WriteLine(reader.Name + " " + reader["name"]);
                }

                string finalFileText = string.Format(PacketFormat.sFileForamat, packetEnums, genPackets);
                File.WriteAllText("GenPackets.cs", finalFileText);

                string clientManagerText = string.Format(PacketFormat.sManagerFormat, clientRegister);
                File.WriteAllText("ClientPacketManager.cs", clientManagerText);

                string serverManagerText = string.Format(PacketFormat.sManagerFormat, serverRegister);
                File.WriteAllText("ServerPacketManager.cs", serverManagerText);
            }

            static void ParsePacket(XmlReader reader)
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    Debug.Assert(false, $"ParsePacket reader.NodeType == XmlNodeType.EndElement");
                    return;
                }

                if (reader.Name.ToLower() != "packet")
                {
                    Debug.Assert(false, $"ParsePacket reader.Name.ToLower() != packet");
                    return;
                }

                string packetName = reader["name"]!;
                if (string.IsNullOrEmpty(packetName))
                {
                    Debug.Assert(false, $"ParsePacket string.IsNullOrEmpty(packetName)");
                    return;
                }

                Tuple<string, string, string> tuple = ParseMembersOrNull(reader);
                Debug.Assert(tuple != null);
                genPackets += string.Format(
                    PacketFormat.sPacketFormat,
                    packetName, tuple.Item1, tuple.Item2, tuple.Item3
                );
                packetEnums += string.Format(PacketFormat.sPacketEnumFormat, packetName, ++packetID) +Environment.NewLine + "\t";
                if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
                    clientRegister += string.Format(PacketFormat.sManagerRegisterFormat, packetName) + Environment.NewLine;
                else
                    serverRegister += string.Format(PacketFormat.sManagerRegisterFormat, packetName) + Environment.NewLine;
            }
            static Tuple<string, string, string> ParseListOrNull(XmlReader reader)
            {
                string structName = reader["name"]!;
                if (string.IsNullOrEmpty(structName))
                {
                    Debug.Assert(false, "structName is invalid");
                    return null;
                }

                Tuple<string, string, string> tuple = ParseMembersOrNull(reader);
                if (tuple == null)
                {
                    Debug.Assert(false);
                    return null;
                }
                string memberCode = string.Format(PacketFormat.sMemberVariableListFormat, 
                    structName,
                    tuple.Item1,
                    tuple.Item2,
                    tuple.Item3
                );
                string readCode = string.Format(PacketFormat.sReadListFormat, structName, FirstCharToLowerOrNull(structName));
                string wrtieCode = string.Format(PacketFormat.sWriteListFormat, structName, FirstCharToLowerOrNull(structName));
                return new Tuple<string, string, string>(memberCode, readCode, wrtieCode);
            }

            //static string FirstCharToUpperOrNull(string input)
            //{
            //    if (string.IsNullOrEmpty(input))
            //    {
            //        Debug.Assert(false);
            //        return null;
            //    }
            //    return input[0].ToString().ToUpper() + input.Substring(1);
            //}

            static string FirstCharToLowerOrNull(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    Debug.Assert(false);
                    return null;
                }
                return input[0].ToString().ToLower() + input.Substring(1);
            }
            static string ToMemberTypeOrNull(string memberType)
            {
                switch (memberType)
                {
                    case "bool":
                        return "ToBoolean";
                    case "short":
                        return "ToInt16";
                    case "ushort":
                        return "ToUInt16";
                    case "int":
                        return "ToInt32";
                    case "long":
                        return "ToInt64";
                    case "float":
                        return "ToSingle";
                    case "double":
                        return "ToDouble";
                    default:
                        Debug.Assert(false);
                        return null;
                }
            }
            // {1} 멤버 변수들
            // {2} 멤버 변수 read
            // {3} 멤버 변수 write
            static Tuple<string, string, string> ParseMembersOrNull(XmlReader reader)
            {
                string packetName = reader["name"]!;

                string memberVarCode = "";
                string readMethodCode = "";
                string writeMethodCode = "";

                int depth = reader.Depth + 1;
                while (reader.Read())
                {
                    if (reader.Depth != depth)
                        break;

                    string memberName = reader["name"]!;
                    if (string.IsNullOrEmpty(memberName))
                    {
                        Debug.Assert(false, "ParseMembers Member without name");
                        return null;
                    }
                    if (!string.IsNullOrEmpty(memberVarCode))
                        memberVarCode += Environment.NewLine;
                    if (!string.IsNullOrEmpty(readMethodCode))
                        readMethodCode += Environment.NewLine;
                    if (!string.IsNullOrEmpty(writeMethodCode))
                        writeMethodCode += Environment.NewLine;


                    string memberType = reader.Name.ToLower();
                    switch (memberType)
                    {
                        case "byte":
                        case "sbyte":
                            memberVarCode += string.Format(PacketFormat.sMemberVariableFormat, memberType, memberName);
                            readMethodCode += string.Format(PacketFormat.sReadByteFormat, memberName, memberType);
                            writeMethodCode += string.Format(PacketFormat.sWrtieByteFormat, memberName, memberType);
                            break;
                        case "bool":
                        case "short":
                        case "ushort":
                        case "int":
                        case "long":
                        case "float":
                        case "double":
                            memberVarCode += string.Format(PacketFormat.sMemberVariableFormat, memberType, memberName);
                            readMethodCode += string.Format(PacketFormat.sReadFormat, memberName, ToMemberTypeOrNull(memberType), memberType);
                            writeMethodCode += string.Format(PacketFormat.sWriteFormat, memberName, memberType);
                            break;
                        case "string":
                            memberVarCode += string.Format(PacketFormat.sMemberVariableFormat, memberType, memberName);
                            readMethodCode += string.Format(PacketFormat.sReadStringFormat, memberName);
                            writeMethodCode += string.Format(PacketFormat.sWrtieStringFormat, memberName);
                            break;
                        case "list":
                            Tuple<string, string, string> tuple = ParseListOrNull(reader);
                            if (tuple == null)
                            {
                                Debug.Assert(false);
                                return null;
                            }
                            memberVarCode += tuple.Item1;
                            readMethodCode += tuple.Item2;
                            writeMethodCode += tuple.Item3;
                            break;
                        default:
                            Debug.Assert(false);
                            return null;
                    }
                }

                memberVarCode = memberVarCode.Replace("\n", "\n\t");
                readMethodCode = readMethodCode.Replace("\n", "\n\t\t");
                writeMethodCode = writeMethodCode.Replace("\n", "\n\t\t");
                return new Tuple<string, string, string>(memberVarCode, readMethodCode, writeMethodCode);
            }
        }




    }
}