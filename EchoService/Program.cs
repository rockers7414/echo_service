using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoService
{
    class Program
    {
        static void Main(string[] args)
        {
            EchoServer.Server.RegRecvHandler(dataRecvHandler);

            Thread t = new Thread(EchoServer.Server.Start);
            t.Start();

            //Thread.Sleep(5000);
            //server.Shutdown();

            t.Join();
        }

        private static void dataRecvHandler(Socket client, String data)
        {
            Console.WriteLine(data);
            EchoServer.Server.Send(client, data.ToUpper());
        }
    }
}
