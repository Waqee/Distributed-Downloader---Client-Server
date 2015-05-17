using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace hackserver
{
    class Server
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        List<Job> jobList;
        List<Task> taskList;
        Dictionary<int, List<MyTcpClient>> clientList;

        public Server()
        {
            this.tcpListener = new TcpListener(IPAddress.Any, 3000);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
            this.jobList = new List<Job>();
            this.clientList = new Dictionary<int, List<MyTcpClient>>();
            this.taskList = new List<Task>();
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for client!");
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            int group_id = 0;

            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    Console.WriteLine("Socket error has occurred");
                    if (group_id > 0)
                    {
                        List<MyTcpClient> list = clientList[group_id];
                        foreach (MyTcpClient mtc in list)
                        {
                            if (mtc.Compare(tcpClient))
                            {
                                foreach (Task t in taskList) 
                                {
                                    int c_id = mtc.getID();
                                    t.taskStopped(c_id);
                                }
                                list.Remove(mtc);
                                Console.WriteLine("Client removed from list of connected clients");
                                break;
                            }
                        }

                    }

                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    Console.WriteLine("Client disconnected");
                    if (group_id > 0)
                    {
                        List<MyTcpClient> list = clientList[group_id];
                        foreach (MyTcpClient mtc in list)
                        {
                            if (mtc.Compare(tcpClient))
                            {
                                foreach (Task t in taskList)
                                {
                                    int c_id = mtc.getID();
                                    t.taskStopped(c_id);
                                }
                                list.Remove(mtc);
                                Console.WriteLine("Client removed from list of connected clients");
                                break;
                            }
                        }
                    }
                    break;
                }

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                //System.Diagnostics.Debug.WriteLine(encoder.GetString(message, 0, bytesRead));
                String[] tokens = encoder.GetString(message, 0, bytesRead).Split(',');
                switch (int.Parse(tokens[0]))
                {
                    case 0: Console.WriteLine("Sign in request");
                        group_id = signIn(tokens[1], tokens[2]);
                        if (group_id > 0)
                        {
                            if (clientList.ContainsKey(group_id))
                            {
                                List<MyTcpClient> list = clientList[group_id];
                                list.Add(new MyTcpClient(tcpClient));
                            }
                            else
                            {
                                List<MyTcpClient> list = new List<MyTcpClient>();
                                list.Add(new MyTcpClient(tcpClient));
                                clientList[group_id] = list;
                            }
                            sendMsg("0,1", tcpClient);       //0 means msg related to login and 1 means success
                            assignTask(group_id, tcpClient);
                        }
                        else
                        {
                            sendMsg("0,0", tcpClient);           //0 means msg related to login and 0 means failed
                        }
                        break;
                    case 1: Console.WriteLine("Download file from " + tokens[1]);
                        storeJob(tokens[1], group_id);
                        break;
                    case 2: Console.WriteLine();        //Message related to task
                        if (int.Parse(tokens[3]) == 1)      //Task completed
                        {
                            Console.WriteLine("Job ID: " + tokens[1]);
                            Console.WriteLine("Task ID: " + tokens[2]);
                            int id = int.Parse(tokens[2]);
                            foreach (Task t in taskList)
                            {
                                if (t.getID() == id)
                                {
                                    taskList.Remove(t);
                                    Console.WriteLine("Task removed from the list");
                                    break;
                                }
                            }
                            assignTask(group_id, tcpClient);
                        }
                        break;
                    default: Console.WriteLine("Invalid Message"); break;
                }
            }

            tcpClient.Close();
        }

        private void storeJob(String link, int id)
        {
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(link);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                int ContentLength;
                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    Console.WriteLine("File size : " + ContentLength);
                    Job job = new Job(link, ContentLength, id);
                    jobList.Add(job);
                }
            }
        }

        private int signIn(String email, String pass)
        {
            Console.WriteLine("Email: " + email);
            Console.WriteLine("Pass: " + pass);
            DBHandler handler = new DBHandler();
            if (handler.login(email, pass))
            {
                Console.WriteLine("login successfull");

                return handler.getNetID(email);
            }
            else
                Console.WriteLine("Invalid username or password");
            return -1;
        }

        public void sendMsg(String msg, TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] buffer = encoder.GetBytes(msg);

            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        private void assignTask(int id, TcpClient client)
        {
            Console.WriteLine("Assgn task called");
            foreach (Job job in jobList)
            {
                if (job.getNetID() == id)
                {
                    Console.WriteLine("Job found for this device");
                    int ind = job.getIndex();
                    int size = job.getSize() - 1;
                    int end = ind + 10000;
                    if (end > size)
                        end = size;
                    if (ind < size)
                    {
                        List<MyTcpClient> list = clientList[id];
                        foreach (MyTcpClient mtc in list) 
                        {
                            if (mtc.Compare(client)) 
                            {
                                int c_id = mtc.getID();
                                Task task = new Task(job.getID(),c_id, ind, end);
                                sendMsg("2," + job.getID() + "," + job.getLink() + "," + ind + "," + end + "," + task.getID(), client);
                                job.setIndex(end + 1);
                                taskList.Add(task);
                                break;
                            }
                        }
                    }
                    else
                    {
                        int j_id = job.getID();
                        bool complete = true;
                        foreach (Task t in taskList) 
                        {
                            if (j_id == t.getJobID() && t.getStatus() == 0) 
                            {
                                complete = false;
                                List<MyTcpClient> list = clientList[id];
                                foreach (MyTcpClient mtc in list)
                                {
                                    if (mtc.Compare(client))
                                    {
                                        int c_id = mtc.getID();
                                        t.setClientID(c_id);
                                        sendMsg("2," + job.getID() + "," + job.getLink() + "," 
                                                + t.getStart() + "," + t.getEnd() + "," + t.getID(), client);
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (complete)
                        {
                            jobList.Remove(job);
                            Console.WriteLine("Job completed");
                        }
                    }
                    break;
                }
            }
        }
    }
}
