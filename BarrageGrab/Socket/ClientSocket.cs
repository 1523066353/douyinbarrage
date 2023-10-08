using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCPServer
{
    public class ClientSocket
    {
        //定义一个静态的变量用于赋予客户端ID
        public static int CLIENT_BEGIN_ID = 1;
        //客户端ID
        public int clientID;
        //客户端连入返回的socket
        public Socket socket;
        //封装好的服务端的socket，调用其封装好的转发消息功能（后面再封装这个ServerSocket类）
        private ServerSocket serverSocket;

        //构造函数中传入客户端连入返回的socket和服务端的ServerSocket对象
        public ClientSocket(Socket clientSocket, ServerSocket serverSocket)
        {
            //记录一下socket
            socket = clientSocket;
            this.serverSocket = serverSocket;
            //记录ID
            clientID = CLIENT_BEGIN_ID;
            ++CLIENT_BEGIN_ID;
        }

        //发送消息，这里的发送消息是指服务端给客户端发送消息
        public void SendMsg(string msg)
        {
            //将string类型消息序列化成字节数组并发送
            if (socket != null)
                socket.Send(Encoding.UTF8.GetBytes(msg));
        }

        //接收消息
        public void ReceiveClientMsg()
        {
            if (socket == null)
                return;

            //判断一下客户端即将收到消息的数量，如果没有消息，就不需要接收处理
            if (socket.Available > 0)
            {
                //定义一个字节数组来装载收到的消息
                byte[] msgBytes = new byte[1024];
                //接收消息，返回的是消息长度，反序列化时需要用到
                int msgLength = socket.Receive(msgBytes);
                //BroadcastMsg这个是服务端socket封装的广播消息方法，将消息和客户端ID传进去进行消息转发
                serverSocket.BroadcastMsg(Encoding.UTF8.GetString(msgBytes, 0, msgLength), clientID);
            }
        }

        //public void OnPoll() {
        //    if (socket!=null)
        //    {
        //         socket.Poll(1000, SelectMode.SelectRead);
        //    }
        //}

        //释放连接
        public void Close()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
    }
}
