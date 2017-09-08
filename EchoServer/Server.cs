using RGiesecke.DllExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
    public class ClientState
    {
        public Socket socket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class Server
    {
        public delegate void DataRecvHandler(Socket client, String data);

        private const int port = 5000;
        private static bool isBusy = false;
        private static IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, port);
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static DataRecvHandler dataRecvHandler = null;

        [DllExport("start", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static void Start()
        {
            Console.WriteLine("Server is starting...");

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                listener.Bind(endPoint);
                listener.Listen(100);

                Server.isBusy = true;
                Console.WriteLine("Waiting connection...");

                while (Server.isBusy) {
                    allDone.Reset();

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    allDone.WaitOne(1000);
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }

        [DllExport("shutdown", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static void Shutdown()
        {
            Console.WriteLine("Servier is shutting down...");
            Server.isBusy = false;
        }

        [DllExport("reg_recv_handler", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static void RegRecvHandler(DataRecvHandler handler)
        {
            dataRecvHandler = handler;
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            ClientState client = new ClientState();
            client.socket = handler;
            handler.BeginReceive(client.buffer, 0, ClientState.BufferSize, SocketFlags.None,
                new AsyncCallback(ReadCallback), client);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            ClientState client = (ClientState)ar.AsyncState;
            Socket handler = client.socket;
            String data = string.Empty;

            int read = handler.EndReceive(ar);
            if (read > 0) {
                client.sb.Append(Encoding.UTF8.GetString(client.buffer, 0, read));
                data = client.sb.ToString();

                if (data.IndexOf("\n") > -1) {
                    Console.WriteLine("Read {0} bytes from socket.", read);
                    Console.WriteLine("Data: {0}", data);

                    dataRecvHandler?.Invoke(handler, data);
                } else {
                    handler.BeginReceive(client.buffer, 0, ClientState.BufferSize, SocketFlags.None,
                        new AsyncCallback(ReadCallback), client);
                }
            }
        }

        [DllExport("send_data", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        public static void Send(Socket handler, String data)
        {
            byte[] bData = Encoding.UTF8.GetBytes(data);
            handler.BeginSend(bData, 0, bData.Length, SocketFlags.None, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try {
                Socket handler = (Socket)ar.AsyncState;
                int send = handler.EndSend(ar);
                Console.WriteLine("Send {0} bytes to client.", send);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
        
    }
}
