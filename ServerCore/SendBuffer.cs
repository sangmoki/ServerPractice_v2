using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    // SendBuffer 클래스를 도와주는 클래스
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = 4096 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            // 현재 버퍼가 null이면 새로운 버퍼를 생성한다.
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // 기존에 있던 버퍼가 reserveSize보다 작으면 새로운 버퍼를 생성한다.
            if (reserveSize > CurrentBuffer.Value.FreeSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            // reserveSize만큼 사용한 후 _usedSize를 증가시킨다.
            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int reserveSize)
        {
            // reserveSize만큼 사용한 후 _usedSize를 증가시킨다.
            return CurrentBuffer.Value.Close(reserveSize);
        }
    }

    public class SendBuffer
    {
        
        // [][][][][][][][][][]
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize {  get { return _buffer.Length - _usedSize; } }

        // byte가 큰 buffer 생성
        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte> Open(int reserveSize)
        {
            // 이전 사이즈가 프리사이즈보다 크면 null을 반환한다.
            if (reserveSize > FreeSize)
                return null;

            // reserveSize만큼 사용한 후 _usedSize를 증가시킨다.
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
