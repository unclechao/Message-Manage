using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 消息总线程序
{
    public class MyLocker
    {
        private Mutex m_Mutex = null;
        public MyLocker(string lockName)
        {
            m_Mutex = GetLocker(lockName);
        }
        private static Dictionary<string, Mutex> s_mutexDic = new Dictionary<string, Mutex>();
        public static void Lock(string lockName)
        {
            Mutex mutex = GetLocker(lockName);
            try
            {
                mutex.WaitOne();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void Unlock(string lockName)
        {
            Mutex mutex = GetLocker(lockName);
            mutex.ReleaseMutex();
        }

        private static Mutex GetLocker(string lockName)
        {
            Mutex mutex;
            if (!s_mutexDic.TryGetValue(lockName, out mutex))
            {
                try
                {
                    mutex = Mutex.OpenExisting(lockName);
                }
                catch (WaitHandleCannotBeOpenedException ex)
                {
                    mutex = new Mutex(false, lockName);
                }
                s_mutexDic.Add(lockName, mutex);
            }
            return mutex;
        }
    }
}
