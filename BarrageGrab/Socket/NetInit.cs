using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer
{
    public static class NetInit
    {
        private static ServerSocket serverSocket;
        public static void Init() {

            serverSocket = new ServerSocket();
            serverSocket.Start("127.0.0.1", 8999, 1024);
            Console.WriteLine("启动服务器");
            //while (true)
            //{
            //    string inputStr = Console.ReadLine();
            //    if (inputStr == "Quit")
            //    {
            //        serverSocket.Close();
            //        break;
            //    }
            //    else if (inputStr.Substring(0, 2) == "B:")
            //    {
            //        serverSocket.BroadcastMsg(inputStr.Substring(2), -1);
            //    }
            //}
        }
        public static void OnSendMsg(string mgs)
        {
            if (serverSocket != null)
            {
                serverSocket.BroadcastMsg(mgs);
            }

        }

        public static void OnClose()
        {

            if (serverSocket != null)
            {
                serverSocket.Close();
            }
        }
    }

    public enum EventType
    {
        /// <summary>
        /// //礼物
        /// </summary>
        ShowText,
        /// <summary>
        /// 弹幕
        /// </summary>
        ShowDigio,
        /// <summary>
        /// 人进来
        /// </summary>
        ShowUserJion,
    }

}
