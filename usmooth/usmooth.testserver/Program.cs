using NNanomsg;
using NNanomsg.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace usmooth.testserver
{
    class Program
    {
        const string addr = "tcp://127.0.0.1:5088";

        static void Main(string[] args)
        {
            using (var s = new PairSocket())
            {
                s.Bind(addr);
                Console.WriteLine("Start listening at: {0}", addr);
                //NanomsgSocketOptions.SetTimespan(s.SocketID, SocketOptionLevel.Default, SocketOption.RCVTIMEO, TimeSpan.FromMilliseconds(100));

                while (true)
                {
                    Console.WriteLine("Listening...");
                    var data = s.ReceiveImmediate();
                    if (data != null)
                    {
                        Console.WriteLine("RECEIVED: '" + Encoding.UTF8.GetString(data) + "'");
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    s.SendImmediate(Encoding.UTF8.GetBytes("the message is " + DateTime.Now.ToLongTimeString()));
                }
            }
        }
    }
}
