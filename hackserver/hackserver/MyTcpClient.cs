using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace hackserver
{
    class MyTcpClient
    {
        static int id = 0;
        int c_id;
        TcpClient tcpClient;
        public MyTcpClient(TcpClient tcpClient) 
        {
            this.tcpClient = tcpClient;
            this.c_id = id++;
        }

        public bool Compare(TcpClient client)
        {
            if (client == tcpClient)
                return true;
            return false;
        }

        public int getID() 
        {
            return c_id;
        }
    }
}
