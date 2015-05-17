using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackserver
{
    class Task
    {
        static int id = 1;
        int t_id;
        int j_id;
        int c_id;
        int status;
        int start;
        int end;

        public Task(int j_id,int c_id, int start, int end) 
        {
            t_id = id++;
            this.j_id = j_id;
            this.c_id = c_id;
            this.status = 1;   //downloading
            this.start = start;
            this.end = end;
        }

        public int getID() 
        {
            return t_id;
        }

        public int getJobID() 
        {
            return j_id;
        }

        public int getStatus() 
        {
            return this.status;
        }

        public void taskStopped(int id) 
        {
            if (c_id == id)
                status = 0;             //stopped
        }

        public void setStatus(int s) 
        {
            this.status = s;
        }

        public void setClientID(int id) 
        {
            this.c_id = id;
        }

        public int getStart() 
        {
            return this.start;
        }

        public int getEnd()
        {
            return this.end;
        }
    }
}
