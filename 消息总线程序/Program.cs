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
    public class Program
    {
        static void Main(string[] args)
        {
            MessageManage msgManage = new MessageManage();
            msgManage.Register();
            msgManage.GetMessageReceived += msgManage_GetMessageReceived;
            int count = 0;
            while (count < 5)
            {
                msgManage.SendMessage(new Random().Next(0, 99));
                count++;
                Thread.Sleep(1000);
            }
            msgManage.Logout();
        }

        static void msgManage_GetMessageReceived(object sender, MessageManage.GetMessageEventArgs e)
        {
            Console.WriteLine("message received : " + e.MessageData);
        }
    }
}
