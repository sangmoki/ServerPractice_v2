using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    abstract class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            // recevArgs에 버퍼를 할당해주어야 받을 수 있다.
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            // 버퍼를 할당받아 생성한다.
            RegisterRecv();
        }

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Send(byte[] sendBuff)
        {
            // 한번에 하나의 스레드만 접근할 수 있도록 lock을 건다.
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }

            //_socket.Send(sendBuff);
            //sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            //_sendArgs.SetBuffer(sendBuff, 0, sendBuff.Length);
            //RegisterSend();
        }

        public void Disconnect()
        {
            // 연결이 이미 끊어졌으면 끊는다.
            if (Interlocked.Exchange(ref _disconnected, 1) == 1);
                return;

            OnDisconnected(_socket.RemoteEndPoint);

            // 클라이언트 연결 종료
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        // send 이후 콜백 함수 호출
                        OnSend(_sendArgs.BytesTransferred);

                        if (_sendQueue.Count > 0)
                            RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv()
        {
            // 보류중인 것이 없으면 다음 프로세스 진행
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
                OnRecvCompleted(null, _recvArgs);
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // 받은 데이터 처리 콜백 함수 호출
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    // 다시 받기 위해 등록
                    RegisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }
}
