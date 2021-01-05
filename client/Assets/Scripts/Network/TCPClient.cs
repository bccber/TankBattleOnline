using Google.Protobuf;
using Protocol;
using System;
using System.Net.Sockets;

namespace Network
{
    public class TCPClient
    {
        private static TCPClient instance = null;
        public static TCPClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TCPClient();
                }
                return instance;
            }
        }

        private TCPClient()
        {
        }

        private Socket client;

        public void Connect(string ip, int port)
        {
            try
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.SendTimeout = 2000;
                client.ReceiveTimeout = 2000;
                client.NoDelay = true;
                client.Connect(ip, port);

                Receive();//开始读取数据
            }
            catch (Exception e)
            {
                Console.WriteLine("连接失败，断开连接");
                Close();
            }
        }

        private void Receive()
        {
            try
            {
                StateObject state = new StateObject();
                state.WorkSocket = client;

                client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallBack, state);
            }
            catch (Exception e)
            {
                Close();
            }
        }
        private void ReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                StateObject state = ar.AsyncState as StateObject;
                int count = state.WorkSocket.EndReceive(ar);
                if (count == 0)
                {
                    Close();
                    return;
                }

                //处理数据
                ProtocolDecoder.Instance.HandleMessage(state.Buffer, count);

                client.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallBack, state);
            }
            catch (Exception e)
            {
                Close();
            }
        }

        /// <summary>
        /// 发送protobuf对象
        /// </summary>
        /// <param name="cmd">命令号</param>
        /// <param name="sendMessage">protobuf对象</param>
        /// <returns>已发送长度</returns>
        public int SendMessage<T>(short cmd, IMessage message)
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    Close();
                    return 0;
                }

                // 组装发送数据 2个字节id
                int protobufLen = message.CalculateSize();
                var sendAllBuffer = new byte[protobufLen + 4];

                // 2个字节的包长
                var headBuffer = BitConverter.GetBytes(protobufLen + 2);
                Buffer.BlockCopy(headBuffer, 0, sendAllBuffer, 0, headBuffer.Length);

                // 2个字节的id
                headBuffer = BitConverter.GetBytes(cmd);
                Buffer.BlockCopy(headBuffer, 0, sendAllBuffer, 2, headBuffer.Length);

                // 数据体
                var buffer = new byte[protobufLen];
                using (CodedOutputStream cos = new CodedOutputStream(buffer))
                {
                    message.WriteTo(cos);
                }
                Buffer.BlockCopy(buffer, 0, sendAllBuffer, 4, protobufLen);

                // 发包
                int sendOKLen = client.Send(sendAllBuffer);

                return sendOKLen;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
    }
}