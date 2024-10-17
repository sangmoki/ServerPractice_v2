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

        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            
            // recevArgs에 버퍼를 할당해주어야 받을 수 있다.
            recvArgs.SetBuffer(new byte[1024], 0, 1024);
            // 버퍼를 할당받아 생성한다.
            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);
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
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            // 보류중인 것이 없으면 다음 프로세스 진행
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
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
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                // 더 이상 받을 데이터가 없다면 소켓을 닫아준다.
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }
        #endregion
    }
}
