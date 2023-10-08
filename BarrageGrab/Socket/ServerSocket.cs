using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace TCPServer
{
    public class ServerSocket
    {
        //服务端socket
        private Socket socket;
        //是否已经关闭socket释放连接
        private bool isClose;
        //保存所有连入客户端与其通信的socket
        private List<ClientSocket> clientList = new List<ClientSocket>();

        //心跳时间
        private int heatTime=3;
        //传入IP地址，端口号和最大连入客户端数量
        public void Start(string ip, int port, int clientNum)
        {
            //建立连接开始，改变状态
            isClose = false;

            //创建socket
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定IP地址和端口号
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(iPEndPoint);

            Console.WriteLine("服务器启动成功...IP:{0},端口:{1}", ip, port);
            Console.WriteLine("开始监听客户端连接...");
            //设置监听数量
            socket.Listen(clientNum);

            //开启线程，持续监听客户端连接
            ThreadPool.QueueUserWorkItem(AcceptClientConnect);
            //开启线程，持续监听连入客户端是否发送了消息
            ThreadPool.QueueUserWorkItem(ReceiveMsg);
            System.Timers.Timer timer = new System.Timers.Timer(heatTime*1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed); //定时监听服务器的动态
            timer.Start();
        }
        //每秒服务端向客户端推送
         void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           // Console.WriteLine("开始发送定时心跳");
            if (clientList.Count > 0)
            {
                for (int i = clientList.Count - 1; i >= 0; i--)
                {

                    string sendStr = "heads";
                    byte[] bs = Encoding.ASCII.GetBytes(sendStr);
                    if (clientList[i].socket.Poll(1000, SelectMode.SelectRead)) //SelectMode.SelectRead表示，如果已调用 并且有挂起的连接，true。
                       // -或 - 如果有数据可供读取，则为 true。-或 - 如果连接已关闭、重置或终止，则返回 true（此种情况就表示若客户端断开连接了，则此方法就返回true）； 否则，返回 false。
                     {
                        clientList[i].Close();//关闭socket
                        clientList.RemoveAt(i);//从列表中删除断开的socke
                        continue;

                    }

                    clientList[i].SendMsg(sendStr);

                }
            }
        }
        //等待客户端连接（新线程）
        private void AcceptClientConnect(object obj)
        {
            Console.WriteLine("等待客户端连入...");
            //死循环，持续不断地监听客户端连接
            while (!isClose)
            {
                //这里会阻塞线程，当有客户端连入时，返回一个socket与客户端通信，才会执行接下来的逻辑
                Socket clientSocket = socket.Accept();
                //有客户端连入了，创建一个我们上面封装好了的ClientSocket对象
                ClientSocket client = new ClientSocket(clientSocket, this);
                Console.WriteLine("客户端{0}连入...", clientSocket.RemoteEndPoint.ToString());
                //向客户端发送一条欢迎消息
                client.SendMsg("欢迎连入服务端...");
                //将客户端添加到List，后续和客户端通信可以从这个List里面取
                clientList.Add(client);
            }
        }

        //接收消息（新线程）
        private void ReceiveMsg(object obj)
        {
            int i;
            //持续不断地监听客户端有没有发送消息
            while (!isClose)
            {
                //当连入客户端数量大于0的时候才去接收消息
                if (clientList.Count > 0)
                {
                    //遍历所有连入的客户端
                    for (i = 0; i < clientList.Count; i++)
                    {
                        try
                        {
                            //调用上面代码封装好的接收消息方法
                            clientList[i].ReceiveClientMsg();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
        }

        //广播消息，传入消息，和发送消息的客户端的ID
        public void BroadcastMsg(string msg, int clientID=-1)
        {
            if (isClose)
                return;
            //遍历所有连入的客户端
            for (int i = 0; i < clientList.Count; i++)
            {
                //如果不是发送消息的客户端则转发消息，不用转发给发送消息的客户端
                if (clientList[i].clientID != clientID)
                    clientList[i].SendMsg(msg);
            }
        }

        //释放连接
        public void Close()
        {
            isClose = true;
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Close();
            }

            clientList.Clear();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
    }
}
