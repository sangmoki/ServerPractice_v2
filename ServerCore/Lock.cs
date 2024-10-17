/*using System;

namespace ServerCore
{
    // 재귀적 락을 허용할 것인지 (No)
    // SpinLock을 사용하기 위한 정책 (5000번 -> Yield)
    class Lock
    {
        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        // [Unused(1)] [WriteThreadId(15비트)] [ReadCount(16비트)]
        // WriteThreadId : 락을 획득한 스레드의 ID
        // ReadCount : 현재 읽고 있는 스레드의 수
        int _flag = EMPTY_FLAG;

        public void WriteLock()
        {
            // 최종적으로 원하는 값
            int desired = (Thread.CurrentThjread.ManagedThreadId << 16) & WRITE_MASK;

            // 아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                        return;

                    // 시도를 해서 성공하면 return
                    if (_flag == EMPTY_FLAG)
                       _flag = desired;
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {

        }

        public void ReadLock()
        {

        }

        public void ReadUnlock()
        {

        }
    }
}
*/