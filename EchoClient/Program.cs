using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {
                socket.Connect(server);

                Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());

                byte[] msg = Encoding.Unicode.GetBytes("{\"method\": \"echo\", \"params\": {\"content\": \"hi\"}, \"id\": \"1234\", \"jsonrpc\": \"2.0\"}\n");
                int bytesSent = socket.Send(msg);

                byte[] bytes = new byte[1024];
                int bytesRec = socket.Receive(bytes);

                Console.WriteLine("Echoed test = {0}", Encoding.Unicode.GetString(bytes, 0, bytesRec));

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
