﻿using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DommyClient
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            for (int i = 0; i < 5; i++)
            {
                // 서버에게 보낼 데이터 생성
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World! {i}");

                // 서버에 데이터 전송
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 클라이언트가 보낸 메세지를 받고 난 후 콜백 함수
        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");
        }

        // send 이후 콜백 함수
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 커넥터 연결 및 세션 생성
            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {            
                // 클라이언트의 입장 번호 설정
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    // 서버에 입장 요청
                    socket.Connect(endPoint);
                    Console.WriteLine($"Connected To {socket.RemoteEndPoint.ToString()}");

                    /*for (int i = 0; i < 5; i++)
                    {
                        // 서버에게 보낼 데이터 생성
                        byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World! {i}");

                        // 서버에게 데이터 전송
                        int sendBytes = socket.Send(sendBuff);
                    }
                    // 서버에서 보낸 데이터 받기
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = socket.Receive(recvBuff);

                    // 받은 데이터 바이트로 변환
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Server] {recvData}");*/

                    // 서버에서 접속 해제
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                Thread.Sleep(100);
            }


            
        }
    }
}