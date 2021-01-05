using System.Net.Sockets;

namespace Network
{
    public class StateObject
    {
        public Socket WorkSocket = null;
        public byte[] Buffer = new byte[1024];
    }
}
