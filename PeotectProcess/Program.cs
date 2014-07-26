using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using 消息总线程序;

namespace PeotectProcess
{
    public class Program
    {
        private static SharedMemory sm = new SharedMemory("test", 100000);
        private static Dictionary<int, string> processDic = null;
        private static Dictionary<int, string> tempDic = null;
        private static ArrayList ts = new ArrayList();
        private static int localV = 0;
        static void Main(string[] args)
        {
            #region
            //while (true)
            //{
            //    Guard();
            //}

            //Process protectProcess = new Process();
            //protectProcess.StartInfo.FileName = "消息总线程序.exe";
            //protectProcess.Start();
            //try
            //{
            //    processDic = (Dictionary<int, string>)sm.Data;
            //}
            //catch (Exception ex)
            //{
            //    processDic = new Dictionary<int, string>();
            //}
            //p = Process.GetProcessById(protectProcess.Id);
            //p.WaitForExit();
            //processDic.Remove(protectProcess.Id);
            //MyLocker.Lock("sm");
            //sm.Data = processDic;
            //MyLocker.Unlock("sm");
            //Console.WriteLine("user count : " + ((Dictionary<int, string>)sm.Data).Count);
            //Thread.Sleep(2000);

            //p = Process.GetProcessById(Convert.ToInt32(args[0]));
            //Console.WriteLine("正在监听程序：" + Convert.ToInt32(args[0]) + " 当前用户数量：" + ((Dictionary<int, string>)sm.Data).Count);
            //p.WaitForExit();         
            //MyLocker.Lock("sm");
            //processDic = (Dictionary<int, string>)sm.Data;
            //processDic.Remove(Convert.ToInt32(args[0]));
            //sm.Data = processDic;
            //Console.WriteLine("主程序：" + Convert.ToInt32(args[0]) + "已退出。当前用户数量：" + ((Dictionary<int, string>)sm.Data).Count);
            //MyLocker.Unlock("sm");
            //Thread.Sleep(2000);
            #endregion
            for (int i = 0; i < 10000; i++)
            {
                ts.Add(0);
            }
            MyLocker.Lock("sm");
            //初始化processDic
            try
            {
                processDic = (Dictionary<int, string>)sm.Data;
                localV = sm.Version;
            }
            catch (Exception ex)
            {
                processDic = new Dictionary<int, string>();
            }
            MyLocker.Unlock("sm");
            while (true)
            {
                if (localV != sm.Version)
                {
                    MyLocker.Lock("sm");
                    try
                    {
                        tempDic = (Dictionary<int, string>)sm.Data;
                        localV = sm.Version;
                    }
                    catch (Exception ex)
                    {
                        tempDic = new Dictionary<int, string>();
                    }
                    MyLocker.Unlock("sm");
                    //分配新线程监视新进程
                    foreach (var temp in tempDic)
                    {
                        if (!processDic.ContainsKey(temp.Key))
                        {
                            ts.RemoveAt(temp.Key);
                            ts.Insert(temp.Key, new Thread(PExit));
                            ((Thread)ts[temp.Key]).Start(temp.Key);
                            MyLocker.Lock("sm");
                            processDic = (Dictionary<int, string>)sm.Data;
                            MyLocker.Unlock("sm");
                        }
                    }
                    //去除无效监视线程
                    foreach (var temp in processDic)
                    {
                        if (!tempDic.ContainsKey(temp.Key))
                        {
                            try
                            {
                                ((Thread)ts[temp.Key]).Abort();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            MyLocker.Lock("sm");
                            processDic = (Dictionary<int, string>)sm.Data;
                            MyLocker.Unlock("sm");
                            Console.WriteLine("当前用户数量：" + processDic.Count);
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        static void PExit(object o)
        {
            //从共享内存中去除无效的进程信息
            Process p = new Process();
            p = Process.GetProcessById((int)o);
            p.WaitForExit();
            MyLocker.Lock("sm");
            processDic = (Dictionary<int, string>)sm.Data;
            processDic.Remove(p.Id);
            //更新本地及共享内存中数据
            sm.Data = processDic;
            processDic = (Dictionary<int, string>)sm.Data;
            MyLocker.Unlock("sm");
            Console.WriteLine("当前用户数量：" + processDic.Count);
        }

        //public static void Guard()
        //{
        //    MyLocker.Lock("sm");
        //    try
        //    {
        //        processDic = (Dictionary<int, string>)sm.Data;
        //    }
        //    catch (Exception ex)
        //    {
        //        processDic = new Dictionary<int, string>();
        //    }
        //    foreach (var item in processDic.Keys)
        //    {
        //        try
        //        {
        //            Process.GetProcessById(item);
        //        }
        //        catch (Exception ex)
        //        {
        //            temp.Add(item);
        //        }
        //    }
        //    foreach (var item in temp)
        //    {
        //        processDic.Remove(item);
        //    }
        //    sm.Data = processDic;
        //    Console.WriteLine("user count : " + processDic.Count);
        //    Thread.Sleep(1000);
        //    MyLocker.Unlock("sm");
        //}
    }
}
