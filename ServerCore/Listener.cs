using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Listener
    {
        // Socket 객체 생성
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {            
            // 소켓 생성 (문지기 역할)
            // TCP로 진행하는데, TCP로 할 때는 SocketType을 Stream으로 맞춰주어야 한다.
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            // 소켓 교육
            _listenSocket.Bind(endPoint);

            // 소켓 대기열 - backlog : 최대 대기 수
            // 10개의 클라이언트가 대기하며, 10개 초과 시 거부한다.
            _listenSocket.Listen(10);

            // 한번만 만들어줘도 재사용이 가능하다.
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            // 이벤트 부여
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);
        }

        // 액세스 등록 함수
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 새로 null로 초기화해주어야 한다.
            args.AcceptSocket = null;

            // 그냥 Accept 할 시 블로킹 계열 함수 사용으로
            // 접속이 막힐 경우 무한대기 할 수 있다.
            // AcceptAsync를 사용하면 비동기로 처리하기 때문에
            // 기다리지 않고 정보를 받을 수 있다.
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        // 논 블로킹 방식
        // -> 비동기 방식을 통하여 여러 사용자들이 접근할 때 이용
        // 액세스 완료 시 호출 함수 
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 소켓 에러가 없을 시
            if (args.SocketError == SocketError.Success)
            {
                // Client Socket을 뱉어주는걸 여기서 해준다.
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Console.WriteLine(args.SocketError.ToString());
            
            // 다음 클라이언트를 위한 소켓 생성
            RegisterAccept(args);
        }

        // public Socket Accept()
        // {
        //    return _listenSocket.Accept();
        // }
    }

}
