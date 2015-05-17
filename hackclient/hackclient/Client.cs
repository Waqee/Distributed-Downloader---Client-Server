using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace hackclient
{
    class Client
    {
        TcpClient client;
        private Thread readThread;
        List<Task> taskList;
        public Client()
        {
            client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
            client.Connect(serverEndPoint);
            taskList = new List<Task>();
        }

        public void sendMsg(String msg)
        {
            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(msg);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        public void submitJob(String link)
        {
            String str = "1," + link;
            sendMsg(str);
        }

        public void authenticate(String email, String pass)
        {
            String str = "0," + email + "," + pass;
            sendMsg(str);
            this.readThread = new Thread(new ThreadStart(rcvMsg));
            this.readThread.Start();
        }

        public void taskCompleted(Task task) 
        {
            sendMsg("2,"+task.getJobID()+","+task.getID()+","+"1");
            taskList.Remove(task);
        }

        private void rcvMsg()
        {
            NetworkStream clientStream = client.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;
            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a server sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    Console.WriteLine("Socket error occurred");
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client Disconnected");
                    break;
                }

                ASCIIEncoding encoder = new ASCIIEncoding();
                String[] tokens = encoder.GetString(message, 0, bytesRead).Split(',');
                switch (int.Parse(tokens[0])) 
                {
                    case 0:
                        int res = int.Parse(tokens[1]);
                        if (res == 1)
                            Console.WriteLine("login successfull");
                        else if(res==0)
                            Console.WriteLine("retry");
                        break;
                    case 2:
                        //Task accepted
                        Console.WriteLine("Download from + " + tokens[2]);
                        Console.WriteLine("Byte no. " + tokens[3] + " to " + tokens[4]);
                        Task task = new Task(int.Parse(tokens[5]),int.Parse(tokens[1]),tokens[2]
                            ,int.Parse(tokens[3]),int.Parse(tokens[4]));
                        taskList.Add(task);
                        Download(tokens[5] + ".bat", tokens[2], long.Parse(tokens[3]), long.Parse(tokens[4]));
                        taskCompleted(task);
                        break;

                    default:
                        Console.WriteLine("invalid code");
                        break;
                }
            }
        }

	private async void Download(String saveas, String url, long start, long end)
        {
            File.Create(saveas).Dispose();

            using (var httpClient = new HttpClient())
            using (var fileStream = new FileStream(saveas, FileMode.Open, FileAccess.Write, FileShare.Write))
            {
                var message = new HttpRequestMessage(HttpMethod.Get, url);
                message.Headers.Add("Range", string.Format("bytes={0}-{1}", start, end));

                fileStream.Position = start;
                await httpClient.SendAsync(message).Result.Content.CopyToAsync(fileStream);
            }
        }
    }
}
