using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;

namespace hackserver
{
    class DBHandler
    {
        OleDbConnection conn;
        OleDbDataAdapter da;
        DataSet ds;
        String table_network = "tab_network";
        String table_device = "tab_device";
        String table_job = "tab_job";
        public DBHandler()
        {
            String connectionString =
                  @"Provider=Microsoft.ACE.OLEDB.12.0;Data"
                + @" Source=C:\Users\Uzair\Desktop\hack.accdb";
            conn = new OleDbConnection(connectionString);
            try
            {
                conn.Open();
            }
            catch (OleDbException exp) { }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            ds = new DataSet();
        }
        public bool login(string email,string pass) 
        {

            String query = String.Format("select * from [{0}]", table_network);
            da = new OleDbDataAdapter(query, conn);
            da.Fill(ds, table_network);
            for (int i = 0; i < ds.Tables[table_network].Rows.Count; i++)
            {
                //Console.WriteLine(i);
                if (email.CompareTo(ds.Tables[table_network].Rows[i]["email"].ToString()) == 0
                    && pass.CompareTo(ds.Tables[table_network].Rows[i]["pass"].ToString()) == 0)
                    return true;
            }
            return false;
        }

        public int getNetID(string email)
        {

            String query = String.Format("select * from [{0}]", table_network);
            da = new OleDbDataAdapter(query, conn);
            da.Fill(ds, table_network);
            for (int i = 0; i < ds.Tables[table_network].Rows.Count; i++)
            {
                //Console.WriteLine(i);
                if (email.CompareTo(ds.Tables[table_network].Rows[i]["email"].ToString()) == 0)
                    return int.Parse(ds.Tables[table_network].Rows[i]["id"].ToString());
            }
            return -1;
        }

        public int signup(string email, string pass)
        {
            String query = String.Format("select * from [{0}]", table_network);
            da = new OleDbDataAdapter(query, conn);
            OleDbCommandBuilder cmdB = new OleDbCommandBuilder(da);
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, table_network);
            for (int i = 0; i < ds.Tables[table_network].Rows.Count; i++)
            {
                if (email.CompareTo(ds.Tables[table_network].Rows[i]["email"].ToString()) == 0)
                    return 2;
                //ds.Tables[table_network].Rows[i]["email"] = email;
            }
            try
            {
                DataRow row = ds.Tables[table_network].NewRow();
                row["email"] = email;
                row["pass"] = pass;
                ds.Tables[table_network].Rows.Add(row);
                da.Update(ds, table_network);
            }
            catch (OleDbException exp) { }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return 0;
        }
        public int adddevice(int nid, int rate) 
        {
            String query = String.Format("select * from [{0}]", table_device);
            da = new OleDbDataAdapter(query, conn);
            OleDbCommandBuilder cmdB = new OleDbCommandBuilder(da);
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, table_device);
            try
            {
                DataRow row = ds.Tables[table_device].NewRow();
                row["net_id"] = nid;
                row["rate"] = rate;
                row["state"] = 0;
                ds.Tables[table_device].Rows.Add(row);
                da.Update(ds, table_device);
            }
            catch (OleDbException exp) { }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            query = String.Format("select * from [{0}]", table_device);
            da = new OleDbDataAdapter(query, conn);
            da.Fill(ds, table_network);
            int last = ds.Tables[table_device].Rows.Count - 1;
            return int.Parse(ds.Tables[table_device].Rows[last]["id"].ToString());
        }

        public int submitJob(int nid, string link, int size) 
        {
            String query = String.Format("select * from [{0}]", table_job);
            da = new OleDbDataAdapter(query, conn);
            OleDbCommandBuilder cmdB = new OleDbCommandBuilder(da);
            da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            da.Fill(ds, table_job);
            try
            {
                DataRow row = ds.Tables[table_job].NewRow();
                row["net_id"] = nid;
                row["link"] = link;
                row["size"] = size;
                row["index"] = 0;
                ds.Tables[table_job].Rows.Add(row);
                da.Update(ds, table_job);
            }
            catch (OleDbException exp) { Console.Write("Exception\n"); }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return 0;
        }

        ~DBHandler() 
        {
            conn.Close();
        }
    }
}
