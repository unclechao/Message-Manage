using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace 消息总线程序
{
    public class MessageManage
    {
        private static SharedMemory sm = new SharedMemory("test", 100000);
        private static Dictionary<int, string> processDic;
        private static MessageQueue queue1 = null;
        private static MessageQueue queue2 = null;
        private static string queueName1 = null;
        private static string queueName2 = null;
        private static int localV;

        public class GetMessageEventArgs : EventArgs
        {
            public readonly object MessageData;
            public GetMessageEventArgs(object data)
            {
                MessageData = data;
            }
        }

        public event EventHandler<GetMessageEventArgs> GetMessageReceived;

        private void ReceiveNow()
        {
            Thread t = new Thread(GetMessage);
            t.Start();
        }

        private void GetMessage()
        {
            while (true)
            {
                int pid = Process.GetCurrentProcess().Id;
                if (processDic.ContainsKey(pid))
                {
                    foreach (object o in processDic)
                    {
                        if (Process.GetCurrentProcess().Id == ((KeyValuePair<int, string>)o).Key)
                        //找到自己的消息队列
                        {
                            queueName1 = ((KeyValuePair<int, string>)o).Value;
                            queue1 = QueueOperate(queue1, queueName1);
                            Message msg = queue1.Receive();
                            if (GetMessageReceived != null)
                            {
                                GetMessageReceived(this, new GetMessageEventArgs(msg.Body.ToString()));
                            }
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 用户发送消息
        /// </summary>
        /// <param name="msg">要发送的消息</param>
        public void SendMessage(object msg)
        {
            lock (processDic)
            {
                int pid = Process.GetCurrentProcess().Id;
                //用于判断用户是否在于Dic中，若用户注销，则不能发送消息
                if (processDic.ContainsKey(pid))
                {
                    if (sm.Version == localV)
                    //本地缓存数据已是最新，直接向本地缓存数据中发送
                    {
                        foreach (object o in processDic)
                        {
                            queueName2 = ((KeyValuePair<int, string>)o).Value;
                            queue2 = QueueOperate(queue2, queueName2);
                            try
                            {
                                queue2.Send(msg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        MyLocker.Lock("sm");
                        //更新本地缓存数据
                        processDic = (Dictionary<int, string>)sm.Data;
                        localV = sm.Version;
                        foreach (object o in processDic)
                        {
                            queueName2 = ((KeyValuePair<int, string>)o).Value;
                            queue2 = QueueOperate(queue2, queueName2);
                            try
                            {
                                queue2.Send(msg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        MyLocker.Unlock("sm");
                    }
                    Console.WriteLine(Process.GetCurrentProcess().Id + " has sent message  : " + msg);
                }
            }
        }

        /// <summary>
        /// 用户向消息总线注册
        /// </summary>
        public void Register()
        {
            //向共享内存中写入数据，进行注册
            int pid = Process.GetCurrentProcess().Id;
            string address = ".\\Private$\\myQueue" + pid;
            MyLocker.Lock("sm");
            try
            {
                processDic = (Dictionary<int, string>)sm.Data;
            }
            catch (Exception ex)
            {
                processDic = new Dictionary<int, string>();
            }
            if (!processDic.ContainsKey(pid))
            {
                processDic.Add(pid, address);
                sm.Data = processDic;
            }
            processDic = (Dictionary<int, string>)sm.Data;
            localV = sm.Version;
            MyLocker.Unlock("sm");
            Console.WriteLine("客户端数量：" + processDic.Count());
            ReceiveNow();
        }

        /// <summary>
        /// 用户注销，注销后无法收发消息
        /// </summary>
        public void Logout()
        {
            int pid = Process.GetCurrentProcess().Id;
            string address = ".\\Private$\\myQueue" + pid;
            if (processDic.ContainsKey(pid))
            {
                processDic.Remove(pid);
            }
            MyLocker.Lock("sm");
            sm.Data = processDic;
            MyLocker.Unlock("sm");
            Console.WriteLine("用户已注销");
        }
        
        private static MessageQueue QueueOperate(MessageQueue queue, string queueName)
        {
            object o = new object();
            lock (o)
            {
                if (MessageQueue.Exists(queueName))
                {
                    queue = new MessageQueue(queueName);
                    queue.Formatter = new BinaryMessageFormatter();
                }
                else
                {
                    queue = MessageQueue.Create(queueName);
                    queue = new MessageQueue(queueName);
                    queue.Formatter = new BinaryMessageFormatter();
                }
                return queue;
            }
        }
    }
}
