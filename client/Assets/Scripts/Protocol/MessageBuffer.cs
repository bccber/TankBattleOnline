namespace Protocol
{
    public class MessageBuffer
    {
        // 命令ID
        public short CMD { get; set; }

        // 数据包
        public byte[] Buffer { get; set; }

        public MessageBuffer(short cmd, byte[] buffer)
        {
            CMD = cmd;
            Buffer = buffer;
        }
    }
}