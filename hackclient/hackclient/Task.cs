using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackclient
{
    class Task
    {
        int id;
        int j_id;
        String link;
        int start;
        int end;
        public Task(int id,int j, String l, int s, int e) 
        {
            this.id = id;
            this.j_id = j;
            this.link = l;
            this.start = s;
            this.end = e;
        }

        public int getID() 
        {
            return id;
        }

        public int getJobID() 
        {
            return j_id;
        }
    }
}
