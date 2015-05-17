using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hackserver
{
    class Job
    {
        static int id = 1;
        int job_id;
        String link;
        int size;
        int index;
        int net_id;
        int part;

        public Job(String l, int s, int n) 
        {
            job_id = id++;
            link = l;
            size = s;
            index = 0;
            net_id = n;
            part = 0;
        }

        public int getNetID()
        {
            return net_id;
        }

        public int getIndex() 
        {
            return index;
        }

        public int getSize() 
        {
            return size;
        }

        public int getParNo() 
        {
            return part;
        }

        public string getLink() 
        {
            return link;
        }

        public int getID() 
        {
            return job_id;
        }

        public void setIndex(int ind) 
        {
            this.index = ind;
        }
    }
}
