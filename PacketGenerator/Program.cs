using PacketGenerator;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace PacketGenerator
{
    internal class Program
    {
        public static string genPackets;
        static void Main(string[] args)
        {
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreComments = true,
                IgnoreWhitespace = true
            };

            using (XmlReader reader = XmlReader.Create("PDL.xml", settings))
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
                File.WriteAllText("GenPackets.cs", genPackets);

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