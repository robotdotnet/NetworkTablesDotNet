using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetworkTables.Message.MsgType;

namespace NetworkTables
{
    internal class Message
    {
        public enum MsgType : uint
        {
            kUnknown = 0xffff,
            kKeepAlive = 0x00,
            kClientHello = 0x01,
            kProtoUnsup = 0x02,
            kServerHelloDone = 0x03,
            kServerHello = 0x04,
            kClientHelloDone = 0x05,
            kEntryAssign = 0x10,
            kEntryUpdate = 0x11,
            kFlagsUpdate = 0x12,
            kEntryDelete = 0x13,
            kClearEntries = 0x14,
            kExecuteRpc = 0x20,
            kRpcResponse = 0x21
        };

        public delegate NtType GetEntryTypeFunc(uint id);

        private MsgType m_type = 0;
        private string m_str = "";
        private Value m_value = new Value();
        uint m_id;
        uint m_flags;
        uint m_seq_num_uid;

        public Message()
        {
            m_type = MsgType.kUnknown;
            m_id = 0;
            m_flags = 0;
            m_seq_num_uid = 0;
        }

        private Message(MsgType type)
        {
            m_type = type;
            m_id = 0;
            m_flags = 0;
            m_seq_num_uid = 0;
        }

        public MsgType Type()
        {
            return m_type;
        }

        public bool Is(MsgType type)
        {
            return m_type == type;
        }

        public string Str()
        {
            return m_str;
        }

        public Value Val()
        {
            return m_value;
        }

        public uint Id() => m_id;

        public uint Flags() => m_flags;

        public uint SeqNumUid() => m_seq_num_uid;

        const uint kClearAllMagic = 0xD06CB27Au;

        public void Write(WireEncoder encoder)
        {
            switch (m_type)
            {
                case kKeepAlive:
                    encoder.Write8((byte)kKeepAlive);
                    break;
                case kClientHello:
                    encoder.Write8((byte)kClientHello);
                    encoder.Write16((ushort)encoder.ProtoRev);
                    if (encoder.ProtoRev < 0x0300u) return;
                    encoder.WriteString(m_str);
                    break;
                case kProtoUnsup:
                    encoder.Write8((byte)kProtoUnsup);
                    encoder.Write16((ushort)encoder.ProtoRev);
                    break;
                case kServerHelloDone:
                    encoder.Write8((byte)kServerHelloDone);
                    break;
                case kServerHello:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kServerHello);
                    encoder.Write8((byte)m_flags);
                    encoder.WriteString(m_str);
                    break;
                case kClientHelloDone:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kClientHelloDone);
                    break;
                case kEntryAssign:
                    encoder.Write8((byte)kEntryAssign);
                    encoder.WriteString(m_str);
                    encoder.WriteType(m_value.Type);
                    encoder.Write16((ushort)m_id);
                    encoder.Write16((ushort)m_seq_num_uid);
                    if (encoder.ProtoRev >= 0x0300u) encoder.Write8((byte)m_flags);
                    encoder.WriteValue(m_value);
                    break;
                case kEntryUpdate:
                    encoder.Write8((byte)kEntryUpdate);
                    encoder.Write16((ushort)m_id);
                    encoder.Write16((ushort)m_seq_num_uid);
                    if (encoder.ProtoRev >= 0x0300u) encoder.WriteType(m_value.Type);
                    encoder.WriteValue(m_value);
                    break;
                case kFlagsUpdate:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kFlagsUpdate);
                    encoder.Write16((ushort)m_id);
                    encoder.Write8((byte)m_flags);
                    break;
                case kEntryDelete:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kEntryDelete);
                    encoder.Write16((ushort)m_id);
                    break;
                case kClearEntries:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kClearEntries);
                    encoder.Write32((uint)kClearAllMagic);
                    break;
                case kExecuteRpc:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kExecuteRpc);
                    encoder.Write16((ushort)m_id);
                    encoder.Write16((ushort)m_seq_num_uid);
                    encoder.WriteValue(m_value);
                    break;
                case kRpcResponse:
                    if (encoder.ProtoRev < 0x0300u) return;  // new message in version 3.0
                    encoder.Write8((byte)kRpcResponse);
                    encoder.Write16((ushort)m_id);
                    encoder.Write16((ushort)m_seq_num_uid);
                    encoder.WriteValue(m_value);
                    break;
                default:
                    break;
            }
        }

        public static Message Read(WireDecoder decoder, GetEntryTypeFunc getEntryType)
        {
            byte msgType = 0;
            if (!decoder.Read8(ref msgType)) return null;
            MsgType mtype = (MsgType)msgType;
            var msg = new Message(mtype);
            NtType type = 0;
            byte tmpB = 0;
            ushort tmpUs = 0;
            uint tmpUi = 0;
            switch (mtype)
            {
                case MsgType.kKeepAlive:
                    break;
                case MsgType.kClientHello:
                    ushort protoRev = 0;
                    if (!decoder.Read16(ref protoRev)) return null;
                    msg.m_id = protoRev;
                    if (protoRev >= 0x0300u)
                    {
                        if (!decoder.ReadString(ref msg.m_str)) return null;
                    }
                    break;
                case MsgType.kProtoUnsup:
                    ushort rdproto = 0;
                    if (!decoder.Read16(ref rdproto)) return null;
                    msg.m_id = rdproto;
                    break;
                case MsgType.kServerHelloDone:
                    break;
                case MsgType.kServerHello:
                    if (decoder.ProtoRev < 0x0300u)
                    {
                        decoder.Error = "received SERVER_HELLO_DONE in protocol < 3.0";
                        return null;
                    }
                    byte rdflgs = 0;
                    if (!decoder.Read8(ref rdflgs)) return null;
                    msg.m_flags = rdflgs;
                    if (!decoder.ReadString(ref msg.m_str)) return null;
                    break;
                case MsgType.kClientHelloDone:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "recieved SERVER_HELLO_DONE in protocol < 3.0";
                        return null;
                    }
                    break;
                case MsgType.kEntryAssign:
                    if (!decoder.ReadString(ref msg.m_str)) return null;
                    if (!decoder.ReadType(ref type)) return null;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_seq_num_uid = tmpUs;
                    if (decoder.ProtoRev >= 0x0300)
                    {
                        if (!decoder.Read8(ref tmpB)) return null;
                        msg.m_flags = tmpB;
                    }
                    msg.m_value = decoder.ReadValue(type);
                    if (msg.m_value == null) return null;
                    break;
                case kEntryUpdate:
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_seq_num_uid = tmpUs;
                    if (decoder.ProtoRev >= 0x0300)
                    {
                        if (!decoder.ReadType(ref type)) return null;
                    }
                    else
                    {
                        type = getEntryType(msg.m_id);
                    }
                    //Debug
                    msg.m_value = decoder.ReadValue(type);
                    if (msg.m_value == null) return null;
                    break;
                case kFlagsUpdate:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received FLAGS_UPDATE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    if (!decoder.Read8(ref tmpB)) return null;
                    msg.m_flags = tmpB;
                    break;
                case MsgType.kEntryDelete:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received ENTRY_DELETE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    break;
                case kClearEntries:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received CLEAR_ENTRIES in protocol < 3.0";
                        return null;
                    }
                    uint magic = 0;
                    if (!decoder.Read32(ref magic)) return null;
                    if (magic != kClearAllMagic)
                    {
                        decoder.Error = "received incorrect CLEAR_ENTRIES magic value, ignoring";
                        return null;
                    }
                    break;
                case kExecuteRpc:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received EXECUTE_RPC in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_seq_num_uid = tmpUs;
                    ulong size = 0;
                    if (!decoder.ReadUleb128(out size)) return null;
                    byte[] results = null;
                    if (!decoder.Read(out results, (int)size)) return null;
                    msg.m_value = Value.MakeRpc(results, (int)size);
                    break;
                case kRpcResponse:
                    if (decoder.ProtoRev < 0x0300)
                    {
                        decoder.Error = "received RPC_RESPONSE in protocol < 3.0";
                        return null;
                    }
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_id = tmpUs;
                    if (!decoder.Read16(ref tmpUs)) return null;
                    msg.m_seq_num_uid = tmpUs;
                    ulong size2 = 0;
                    if (!decoder.ReadUleb128(out size2)) return null;
                    byte[] results2 = null;
                    if (!decoder.Read(out results2, (int)size2)) return null;
                    msg.m_value = Value.MakeRpc(results2, (int)size2);
                    break;
                default:
                    decoder.Error = "unrecognized message type";
                    //Info
                    return null;
                    //TODO:: Tons of these
            }
            return msg;
        }

        public static Message KeepAlive()
        {
            return new Message(MsgType.kKeepAlive);
        }

        public static Message ProtoUnsup() => new Message(MsgType.kProtoUnsup);

        public static Message ServerHelloDone() => new Message(MsgType.kServerHelloDone);

        public static Message ClientHelloDone() => new Message(MsgType.kClientHelloDone);

        public static Message ClearEntries() => new Message(MsgType.kClearEntries);

        public static Message ClientHello(string selfId)
        {
            var msg = new Message(MsgType.kClientHello);
            msg.m_str = selfId;
            return msg;
        }

        public static Message ServerHello(uint flags, string selfId)
        {
            var msg = new Message(MsgType.kServerHello);
            msg.m_str = selfId;
            msg.m_flags = flags;
            return msg;
        }

        public static Message EntryAssign(string name, uint id, uint seqNum, Value value, EntryFlags flags)
        {
            var msg = new Message(MsgType.kEntryAssign);
            msg.m_str = name;
            msg.m_value = value;
            msg.m_id = id;
            msg.m_flags = (uint)flags;
            msg.m_seq_num_uid = seqNum;
            return msg;
        }

        public static Message EntryUpdate(uint id, uint seqNum, Value value)
        {
            var msg = new Message(MsgType.kEntryUpdate);
            msg.m_value = value;
            msg.m_id = id;
            msg.m_seq_num_uid = seqNum;
            return msg;
        }

        public static Message FlagsUpdate(uint id, EntryFlags flags)
        {
            var msg = new Message(MsgType.kFlagsUpdate);
            msg.m_id = id;
            msg.m_flags = (uint)flags;
            return msg;
        }

        public static Message EntryDelete(uint id)
        {
            var msg = new Message(MsgType.kEntryDelete);
            msg.m_id = id;
            return msg;
        }

        public static Message ExecuteRpc(uint id, uint uid, byte[] param)
        {
            var msg = new Message(kExecuteRpc);
            msg.m_value = Value.MakeRpc(param, param.Length);
            msg.m_id = id;
            msg.m_seq_num_uid = uid;
            return msg;
        }

        public static Message RpcResponse(uint id, uint uid, byte[] results)
        {
            var msg = new Message(kRpcResponse);
            msg.m_value = Value.MakeRpc(results, results.Length);
            msg.m_id = id;
            msg.m_seq_num_uid = uid;
            return msg;
        }


    }
}
