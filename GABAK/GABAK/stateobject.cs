//MIT License
//Copyright(c) 2018 Sabahattin Gokhan Ozden
using System.Net.Sockets;
using System.Text;

namespace GABAK
{
    internal class stateobject
    {
        // Client socket.
        public Socket workSocket = null;

        // Size of receive buffer.
        public const int BufferSize = options.bufferSize;

        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}