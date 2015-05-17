using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace hackclient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();
            client.authenticate("12beseughauri@seecs.edu.pk","seecs@123");
            //Console.ReadKey();
            //client.submitJob("http://www.lob.de/pdf/helloworld.pdf");
            //Console.ReadKey();
        }
    }
}
