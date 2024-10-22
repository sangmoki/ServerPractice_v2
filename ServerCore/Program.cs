using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Collections.Specialized.BitVector32;

namespace ServerCore
{
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
            // 클라이언트보낸다
            //clientSocket.Send(sendBuff);

            Send(sendBuff);

            Thread.Sleep(1000);

            // Disconnect를 두번 하더라도
            // InterLocked를 사용한 flag를 통해 문제 없이 작동한다.
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        // 클라이언트가 보낸 메세지를 받고 난 후 콜백 함수
        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        // send 이후 콜백 함수
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}");
        }
    }

    class Program
    {
        // Listener 클래스 객체 생성
        static Listener _listener = new Listener();

        // 초기 핸들러 세팅
        /*static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 클라이언트가 보낸 메세지를 받는다. - 바이트 단위로
                //byte[] recvBuff = new byte[1024];
                //int recvBytes = clientSocket.Receive(recvBuff);
                // UTF8로 인코딩하여 클라이언트가 보낸 메시지를 가져온다.
                //string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                //Console.WriteLine($"[From Client] {recvData}");


                // Session 클래스 객체에 소켓을 넣어 초기화
                GameSession session = new GameSession();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                // 클라이언트보낸다
                //clientSocket.Send(sendBuff);

                session.Send(sendBuff);

                Thread.Sleep(1000);

                // Disconnect를 두번 하더라도
                // InterLocked를 사용한 flag를 통해 문제 없이 작동한다.
                session.Disconnect();
                session.Disconnect();

                // 클라이언트 연결 종료
                //clientSocket.Shutdown(SocketShutdown.Both);
                //clientSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }*/

        static void Main(string[] args)
        {
            // Socket 생성 시 인자
            // 첫번 째 인자 - 네트워크
            // 두번 째 인자 - 소켓 타입
            // 세번 째 인자 - 프로토콜

            // 첫 번째 인자 - DNS 사용
            // 내 로컬의 dns를 사용하여 호스트 이름을 가져온다.
            string host = Dns.GetHostName();
            // 호스트 이름을 이용하여 IPHostEntry를 가져온다.
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            // 트래픽이 많이 들어오는 경우 AddressList를 사용하여 트래픽을 분산시킬 수 있다.
            IPAddress ipAddr = ipHost.AddressList[0];
            // 포트 번호를 지정하여 IPEndPoint를 생성한다.
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 소켓 생성 (문지기 역할)
            // TCP로 진행하는데, TCP로 할 때는 SocketType을 Stream으로 맞춰주어야 한다.
            // Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            _listener.Init(endPoint, () => { return new GameSession(); });

            // 소켓 교육
            // listenSocket.Bind(endPoint);

            // 소켓 대기열 - backlog : 최대 대기 수
            // 10개의 클라이언트가 대기하며, 10개 초과 시 거부한다.
            // listenSocket.Listen(10);

            // 프로그램이 종료되지 않도록 임시로 반복 사용
            while (true)
            {
                // Console.WriteLine("Listening...");

                // 클라이언트를 입장시킨다.
                // 실제로 사용하지는 않지만 연습용도로 입장 허용
                // 다음 단계로 넘어갈 수 없으면 대기열에서 기다린다.
                // Socket clientSocket = _listener.Accept();

                // 클라이언트가 보낸 메세지를 받는다. - 바이트 단위로
                // byte[] recvBuff = new byte[1024];
                // int recvBytes = clientSocket.Receive(recvBuff);
                // UTF8로 인코딩하여 클라이언트가 보낸 메시지를 가져온다.
                // string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                // Console.WriteLine($"[From Client] {recvData}");

                // 클라이언트보낸다
                // byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                // clientSocket.Send(sendBuff);

                // 클라이언트 연결 종료
                // clientSocket.Shutdown(SocketShutdown.Both);
                // clientSocket.Close();

            }
        }
    }

    /* ThreadLocal을 이용하여 쓰레드의 고유공간을 제공
        class Program
        {
            // 고치는 순간 다른 친구들에게도 영향을 줄 수 있기 때문에 ThreadLocal 이용한다.
            // static string ThreadName;

            // ThreadLocal은 쓰레드마다 고유한 공간을 제공해준다. 
            static ThreadLocal<string> ThreadName = new ThreadLocal<string>(() =>
            {
                return $"My Name is {Thread.CurrentThread.ManagedThreadId}";
            });

            static void WhoAmI()
            {
                bool repeat = ThreadName.IsValueCreated;

                if (repeat)
                    // 이미 만들어져 있으면 true로 반환
                    Console.WriteLine(ThreadName.Value + " (repeat)");
                else
                    // 한번도 만든적 없으면 fasle로 반환
                    Console.WriteLine(ThreadName.Value);
            }

            static void Main(string[] args)
            {
                ThreadPool.SetMinThreads(1, 1);
                ThreadPool.SetMaxThreads(3, 3);

                // 기존 Task로 사용하던 방법에서 Parallel로 사용하는 방법
                // Invoke를 통하여 task를 여러번 사용할 수 있다.
                Parallel.Invoke(WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI, WhoAmI);

                ThreadName.Dispose();
            }
        }
    */

    /* Lock을 이용하는 방법 3가지와 ReaderWriterLock
            class Program
            {
                // Lock을 이용하는 방법
                // 다만 상호배제성을 띈다.
                // 1. 근성 - 계속 반복을 돌며 Try하는 방법
                static object _lock = new object();
                // 2. 양보 - 다른 쓰레드에게 순서를 양보 후 Try하는 방법
                static SpinLock _lock2 = new SpinLock();
                // 3. 갑질 - 다른 쓰레드를 호출하여 대신 Try하는 방법
                static Mutext _lock3 = new Mutext();
                // 4. 읽기 쓰기 잠금 - 읽기와 쓰기를 나누어 잠금하는 방법
                ReaderWriterLockSlim _lock4 = new ReaderWriterLockSlim();

                class Reward
                {

                }


                static Reward GetRewardById(int id)
                {
                    // 읽기
                    _lock4.EnterReadLock();

                    _lock4.ExitReadLock();

                    return null;
                }

                static void AddReward(Reward reward)
                {
                    _lock4.EnterWriteLock();

                    _lock4.ExitWriteLock();
                }

                static void Main(string[] args)
                {
                    lock (_lock)
                    {

                    }

                }
            }
        */

    /*  Event보다 효과적인 Mutex 사용
        class Program
        {
            static int _num = 0;

            // 기존 Event는 bool 값만을 가지고 있다면,
            // Mutex는 커널 동기화 객체를 사용하여 여러 쓰레드가 동시에 접근하지 못하도록 한다.
            static Mutex _lock = new Mutex(); // 커널 동기화 객체

            static void Thread_1()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WaitOne();
                    _num++;
                    _lock.ReleaseMutex();
                }
            }

            static void Thread_2()
            {
                for (int i = 0; i < 100000; i++)
                {
                    _lock.WaitOne();
                    _num--;
                    _lock.ReleaseMutex();
                }
            }

            static void Main(string[] args)
            {
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();
                t2.Start();

                // Task가 끝날 때까지 대기
                Task.WaitAll(t1, t2);

                Console.WriteLine(_num);
            }
        }
    */

    /*  Lock Event WaitOne, Set, Reset
    class Lock
    {
        // 톨게이트를 연 상태로 시작할 것인지
        // bolean <- 커널 
        // AutoResetEvent _available = new AutoResetEvent(true);

        ManualResetEvent _available = new ManualResetEvent(false);

        public void Acquire()
        {
            _available.WaitOne(); // 입장을 시도한다.

            // AutoResetEvent 같은 경우 입장 시도 후 Reset이 암묵적으로 내포 되어있다.
            _available.Reset(); // bool -> false - 문을 닫는다
        }

        public void Release()
        {
            // event의 상태를 true로 변경해준다.
            _available.Set();
        }
    }
*/

    /* SpinLock과 Context Switching 
        class SpinLock
        {
            volatile int _locked = 0;

            public void Acquire()
            {
                // 원래는 0이었는데 1로 치환한다.
                // 만약 다른 친구가 1로 치환했다면 계속 반복한다.
                // 즉, SpinLock을 통하여 예외처리 느낌으로 처리할 수 있다.
                while (true)
                {
                    // 1번째 방법
                    // _locked를 1로 변경한다.
                    //int original = Interlocked.Exchange(ref _locked, 1);
                    //if (original == 0)
                    //    break;

                    // 2번째 방법 - CAS Compare-And-Swap
                    // 조건 - _locked를 비교하여 0이라면 1로 치환
                    int expected = 0; // 예상한 값
                    int desired = 1; // 예상한 값이 맞다면 바꿀 값

                    //int original = Interlocked.CompareExchange(ref _locked, 1, 0);
                    //if (original == 0)
                    if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                        break;

                    // Context Switing의 3가지 방법
                    Thread.Sleep(1); // ms 단위로 휴식한다.(무조건)
                    Thread.Sleep(0); // 조건부 양보 -> 나보다 우선순위가 높은 애들한테 양보 -> 우선순위가 같거나 낮으면 그냥 실행.
                    Thread.Yield(); // 관대한 양보 -> 조건 없이 양보 -> 지금 실행 가능한 쓰레드 있으면 실행 -> 없으면 다시 실행
                }
            }

            public void Release()
            {
                _locked = 0;
            }
        }

        class Program
        {
            static int _num = 0;
            static SpinLock _lock = new SpinLock();

            static void Thread_1()
            {
                for (int i = 0; i < 1000000; i++)
                {
                    _lock.Acquire();
                    _num++;
                    _lock.Release();
                }
            }

            static void Thread_2()
            {
                for (int i = 0; i < 1000000; i++)
                {
                    _lock.Acquire();
                    _num--;
                    _lock.Release();
                }
            }

            static void Main(string[] args)
            {
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();
                t2.Start();

                // Task가 끝날 때까지 대기
                Task.WaitAll(t1, t2);

                Console.WriteLine(_num);
            }
        }
    */

    /*  DeadLock 발생 조건 확인 및 적립
        // 각각의 스레드에서 서로를 호출하는 상황 연출
        // 서로를 호출하는 상황에서 Lock이 걸려있어 DeadLock이 발생하는 것을 확인할 수 있다.
        class SessionManager
        {
            static object _lock = new object();

            public static void Test()
            {
                lock (_lock)
                {
                    UserManager.TestUser();
                }
            }

            public static void TestSession()
            {
                lock (_lock)
                {
                }
            }
        }

        class UserManager
        {
            static object _lock = new object();

            public static void Test()
            {
                lock (_lock)
                {
                    SessionManager.TestSession();
                }
            }

            public static void TestUser() 
            {
                lock (_lock)
                {

                }
            }
        }

        class Program
        {
            static int number = 0;
            static object _obj = new object();

            static void Thread_1()
            {
                for (int i = 0; i < 10; i++)
                {

                    SessionManager.Test();

                    // DeadLock 발생 조건
                    // Enter와 Exit가 걸려있을 때 조건으로 return을 걸어놓으면
                    // DeadLock 발생 확률이 있으므로 잘 캐치해야 한다.

                    // 이런 경우 try catch finally를 사용하여 예외처리를 해주어야 한다.
                    // finally는 무조건 한번은 실행하기 때문

                    // 아래의 코드와 같이 직접 Enter, Exit를 사용하는 것 보다는
                    // lock을 사용하는 것이 좋다. (내부적으로 Enter, Exit를 한다.
                    // lock (_obj)
                    // {
                    //    number++;
                    // }

                    // 먼저 점유 (잠금)
                    // 문을 잠그면 잠금 해제 전까지 다른 쓰레드에서 기다린다.
                    // 즉, 상호 배제(Mutual Exclusion)를 보장한다.
                    //Monitor.Enter(_obj);
                    //number++;
                    // 잠금 해제
                    //Monitor.Exit(_obj);
                }
            }

            static void Thread_2()
            {
                for (int i = 0; i < 10; i++)
                {
                    UserManager.Test();

                    // 먼저 점유
                    // Monitor.Enter(_obj);
                    // number--;
                    // Monitor.Exit(_obj);
                }
            }

            static void Main(string[] args)
            {
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();

                Thread.Sleep(100);
                t2.Start();

                Task.WaitAll(t1, t2);

                Console.WriteLine(number);
            }
        }
*/

    /*  경합조건에서 발생하는 문제점과 Interlocked를 활용한 해결
        // 경합조건
        // 여러 쓰레드가 하나의 자원을 사용할 때 발생하는 문제
        // 순서가 보장되지 않는다.
        // -> 순서를 보장하기 위해 lock을 사용한다.

        // Interlocked의 장점
        // 1. 원자성을 보장
        // 2. 순서를 보장
        // 3. 속도가 빠르다.
        // Interlocked의 단점 
        // -> 1.원자성을 보장하기 때문에 속도가 빠르지만 느리다.

        static int number = 0;

        // atomic = 원자성 -> 더이상 분리될 수 없는 최소의 원소단위
        static void Thread_1()
        {
            for (int i = 0; i < 100000; i++)
            {
                // 원자성을 이용하여 순서를 보장
                // 아래의 코드를 Increment로 대체 -> 원자성을 이용하여 순서를 보장
                // ref가 붙은 이유는 number의 값이 뭔지는 모르지만 참조하여 무조건 1 늘려라 라는 명령


                int afterValue = Interlocked.Increment(ref number);

                //number++;
                //int temp = number; // 0
                //temp += 1; // 1
                //number = temp; // 1
            }
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
            {
                Interlocked.Decrement(ref number);

                //number--;
                //int temp = number; // 0
                //temp -= 1; // -1
                //number = temp; // -1
            }
        }

        static void Main(string[] args)
        {
            Task t1 = new Task(Thread_1);
            Task t2 = new Task(Thread_2);

            // 원자(atomic, volatile) 단위로 덧셈 뺄셈을 진행하기 때문에 순서가 보장된다.
            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);

            Console.WriteLine(number);
        }
    */

    /* 멀티 쓰레드 환경에서의 문제점과 해결법 Memory Barrier
        // 메모리 배리어 -> 멀티 쓰레드 환경에서 발생하는 순서의 가시성(인식)을 늘려 뒤바뀜 문제 해결
        // A) 코드 재배치 억제 -> Memory Barrier
        // B) 가시성 -> Volatile

        // 1) Full Memory Barrier - (ASM : MFENCE, C#: Thread.MemoryBarrier) : Store/Load가 모두 재배치 되지 않도록 한다.
        // 2) Store Memory Barrier - (ASM : SFENCE) : Store만 재배치 되지 않도록 한다.
        // 3) Load Memory Barrier - (ASM : LFENCE) : Load만 재배치 되지 않도록 한다.
        // 가시성 -> 변수를 읽을 때 메모리에서 읽어오도록 한다.

        // Store를 사용하면 물내림 작업 (Memory Barrier)를 사용
        // Load를 사용하기 이전에 물내림 작업 (Memory Barrier)를 사용
        // 즉, Store를 사용하면 최신화를 해주어야 하고
        // Load 전에 최신화가 되었는지 확인해야 한다.

        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread_1()
        {
            y = 1; // Store y

            // 메모리 배리어
            // y와 x 사이에 선을 그어 코드 재배치를 억제한다.
            Thread.MemoryBarrier();
            r1 = x; // Load x
        }        

        static void Thread_2()
        {
            x = 1; // Store x

            Thread.MemoryBarrier();
            r2 = y; // Load y
        }

        static void Main(string[] args)
        {
            int count = 0;
            while(true)
            {
                count++;
                x = y = r1 = r2 = 0;

                // 멀티 쓰레드 환경
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);
                t1.Start();
                t2.Start();

                Task.WaitAll(t1, t2);

                // 멀티쓰레드 환경에서는 
                // 하드웨어 자체에서 최적화를 하기 때문에 -? 순서를 뒤바꾸기도 한다.
                // 그래서 r1, r2가 0이 나올 수 있다.
                if (r1 == 0 && r2 == 0)
                    break;
            }

            Console.WriteLine($"{count}번 만에 빠져나옴");

        }
    */

    /* 캐시 테스트 전 2차배열 하드코딩
            static void Main(string[] args)
            {
                int[,] arr = new int[10000, 10000];
                {
                    long now = DateTime.Now.Ticks;
                    for (int y = 0; y < 10000; y++)
                        for (int x = 0; x < 10000; x++)
                            arr[y, x] = 1;

                    long end = DateTime.Now.Ticks;
                    Console.WriteLine($"(y, x) 순서 걸린 시간 {end - now}");
                }

                {
                    long now = DateTime.Now.Ticks;
                    for (int y = 0; y< 10000; y++)
                        for (int x = 0; x< 10000; x++)
                            arr[x, y] = 1;

                    long end = DateTime.Now.Ticks;
                    Console.WriteLine($"(x, y) 순서 걸린 시간 {end - now}");
                }
            }
    */

    /* 최적화로 인한 문제 해결 방안
            // 전역으로 설정되어 모든 Thread 들이 사용 가능하다.
            // volatile : 컴파일러가 최적화 하지 않도록 하는 키워드(휘발성)
            // 즉, 최적화 하지 말고 있는 그대로 사용
            // Realase 시점에서 최적화 하게 되면 정상적으로 작동하지 않을 수 있다.
            volatile static bool _stop = false;

            static void ThreadMain()
            {
                Console.WriteLine("쓰레드 시작!");

                while (_stop == false)
                {
                }

                Console.WriteLine("쓰레드 종료!");
            }

            static void Main(string[] args)
            {
                Task t = new Task(ThreadMain);
                t.Start();

                // 1초동안 슬립
                Thread.Sleep(1000);

                _stop = true;

                Console.WriteLine("Stop 호출");
                Console.WriteLine("종료 대기중");
                t.Wait();

                Console.WriteLine("종료 성공");
            }
    */

    /*   Thread, ThreadPool, Task 비교
            static void MainThread(object state)
            {
                for (int i = 0; i < 5; i++)
                    Console.WriteLine("Hello Thread!");
            }
            static void Main(string[] args)
            {
                // 최소 실행 쓰레드
                ThreadPool.SetMinThreads(1, 1);
                // 최대 실행 쓰레드
                ThreadPool.SetMaxThreads(5, 5);

                // 쓰레드랑은 다르지만 비슷한 역할을 하는 Task
                // 뒤에 TaskCreationOptions와 같은 option을 통하여 별도의 thread를 실행한다.
                // Thread와 ThreadPool의 장점을 뽑아서 사용하는 느낌
                for (int i = 0; i < 5; i++)
                {
                    // 오래걸리는 작업을 할떄 LongRunning 사용 
                    Task t = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
                    t.Start();
                }

                for (int i = 0; i < 4; i++)
                    ThreadPool.QueueUserWorkItem((obj) => {  while (true) { } });

                // ThreadPool 테스트 -> background로 실행
                ThreadPool.QueueUserWorkItem(MainThread);

                while (true)
                {

                }
                    // 기본 쓰레드 사용
                    Thread t = new Thread(MainThread);
                    // 쓰레드 이름 지정
                    t.Name = "Test Thread";
                    // 백그라운드에서 실행할 것인지 설정 (기본값은 false)
                    t.IsBackground = true;
                    // thread 시작
                    t.Start();
                    Console.WriteLine("Wating for Thread!");
                    // thread가 종료될 때까지 대기
                    t.Join();
                    Console.WriteLine("Hello World!");
            }
    */

    // }
}