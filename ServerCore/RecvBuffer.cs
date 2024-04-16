using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;
        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }
        // 유효한 데이터 범위
        public int DataSize { get { return _writePos - -_readPos; } }
        // 남은 버퍼 사이즈
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        // 유효 범위의 세그먼트. 어디부터 데이터를 읽으면 되냐를 콘텐츠단에 넘겨줌.
        public ArraySegment<byte> ReadSegment 
        { 
            get { return new ArraySegment<byte>(_buffer.Array!, _buffer.Offset + _readPos, DataSize); } 
        }
        // 다음에 데이터를 받을 때 어디서부터 어디까지가 유효범위인지 넘겨줌.
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array!, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0) // rw가 정확히 겹치는 상태
            {
                // 남은데이터가 없으니 복사하지 않고 커서 위치만 리셋
                _readPos = 0;
                _writePos = 0;
            }
            else
            {
                // [][][][][r][][w][][][][] 요런 경우
                // 남은 찌끄레기가 있으면 시작위치로 복사
                Array.Copy(_buffer.Array!, _buffer.Offset + _readPos, _buffer.Array!, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }

    }
}
