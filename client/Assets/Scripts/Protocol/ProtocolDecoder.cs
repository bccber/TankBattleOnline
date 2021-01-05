using Google.Protobuf;
using System;
using System.Threading;

namespace Protocol
{
    public class ProtocolDecoder
    {
        // 单例
        public static ProtocolDecoder Instance = new ProtocolDecoder();

        // 消息回调
        public Action<short, IMessage> OnDataArrival;

        private ProtocolDecoder()
        {
        }

        // 数据动态缓存区
        private byte[] _Buffer = new byte[1024 * 1024];
        private int _Offset = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">第二个字节数组</param>
        /// <param name="count">第二个字节数组的截取长度</param>
        /// <returns></returns>
        private void AppendBuffer(byte[] data, int count)
        {
            // 缓冲区剩余空间足够，直接拷贝
            if (_Buffer.Length - _Offset >= count)
            {
                Buffer.BlockCopy(data, 0, _Buffer, _Offset, count);
                _Offset += count;

                return;
            }

            // 缓冲区剩余空间不够，重新创建，再拷贝
            byte[] buffer = new byte[_Buffer.Length + count];

            Buffer.BlockCopy(_Buffer, 0, buffer, 0, _Buffer.Length);
            Buffer.BlockCopy(data, 0, buffer, _Buffer.Length, count);

            _Buffer = buffer;
            _Offset = _Buffer.Length;
        }

        /// <summary>
        /// 从字节数组得到消息类对象
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private MessageBuffer FromBytes()
        {
            if (_Offset < 2)
            {
                return null;
            }

            // 包长
            short len = BitConverter.ToInt16(_Buffer, 0);
            if (_Offset - 2 < len)
            {
                return null;
            }

            // 消息id
            short id = BitConverter.ToInt16(_Buffer, 2);

            // 消息体
            byte[] newBuffer = new byte[len - 2];
            Buffer.BlockCopy(_Buffer, 4, newBuffer, 0, len - 2);

            MessageBuffer protocol = new MessageBuffer(id, newBuffer);

            // 把剩余内容移到头部 BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count)
            _Offset = _Offset - 2 - len;
            Buffer.BlockCopy(_Buffer, 2 + len, _Buffer, 0, _Offset);

            return protocol;
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="count"></param>
        public void HandleMessage(byte[] data, int count)
        {
            AppendBuffer(data, count);
            while (true)
            {
                //消息对象的包头
                MessageBuffer msgBuf = FromBytes();
                if(msgBuf == null)
                {
                    return;
                }

                // 回调
                Decode(msgBuf);
            }
        }

        // 把buffer解析成目标对象,并回调
        private void Decode(MessageBuffer msgBuf)
        {
            IMessage message = null;
            switch (msgBuf.CMD)
            {
                case CMD.MatchCompleteBroadCast:
                    message = MatchCompleteBroadCast.Parser.ParseFrom(msgBuf.Buffer);
                    break;

                case CMD.OfflineBroadCast:
                    message = OfflineBroadCast.Parser.ParseFrom(msgBuf.Buffer);
                    break;

                case CMD.PlayerMoveBroadCast:
                    message = PlayerMoveBroadCast.Parser.ParseFrom(msgBuf.Buffer);
                    break;

                case CMD.AttackBroadCast:
                    message = AttackBroadCast.Parser.ParseFrom(msgBuf.Buffer);
                    break;
            }

            if (message != null && OnDataArrival != null)
            {
                OnDataArrival(msgBuf.CMD, message);
            }
        }
    }
}