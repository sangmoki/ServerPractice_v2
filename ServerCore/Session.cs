using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;
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

        public void Send(byte[] sendBuff)
        {
            // 한번에 하나의 스레드만 접근할 수 있도록 lock을 건다.
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
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

            // 클라이언트 연결 종료
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

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
                        if (_sendQueue.Count > 0)
                            RegisterSend();
                        else
                            _pending = false;
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
                    // 받은 데이터 처리
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

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
