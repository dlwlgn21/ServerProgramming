using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class SendBufferHelper
    {
        // RaceCondition을 피하기 위해서 ThreadLocal로 만듦.
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });
        public static int ChunckSize { get; set; } = 4096 * 100;

        public static ArraySegment<byte> OpenOrNull(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunckSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunckSize);
            return CurrentBuffer.Value.OpenOrNull(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value!.Close(usedSize);
        }
    }

    public class SendBuffer
    {
        // [u][][][][] [][][][][]
        byte[] _buffer;
        int _usedSize = 0;

        public SendBuffer(int chuncksize)
        {
            _buffer = new byte[chuncksize];
        }
        public int FreeSize { get { return _buffer.Length - _usedSize; } }
        public ArraySegment<byte> OpenOrNull(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;
            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
