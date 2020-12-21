using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace demo
{
    public class Test
    {
        public static string constr = "server=192.168.3.6; user=root; database=test; port=3306; pwd=angel070711";
        public int tag = 0;
        public static Thread upload;
        public static Thread monitor;
        
        /*-----------------测试数据库连接函数--------------------*/
        public void conDB()
        {
            MySqlConnection con = new MySqlConnection(constr);
            try
            {
                con.Open();
                Console.WriteLine("connection successful");
                con.Close();
            }
            catch(Exception)
                {
                    Console.WriteLine("connection error");
                }
        }

        /*-----------------读取AGV顺序表中的station序列--------------------*/
        public void read_sequence()
        {
            if (tag == 0)
            {
                MySqlConnection con = new MySqlConnection(constr);
                con.Open();
                string SQLstr = "Select station from agv_sequence";
                MySqlCommand commission = new MySqlCommand(SQLstr, con);
                MySqlDataAdapter adapter = new MySqlDataAdapter(commission);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                int[] job_se = new int[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    job_se[i] = Convert.ToInt16(dt.Rows[i]["station"]);
                    Console.WriteLine("第" + i + "个目标station为" + job_se[i] + "\n");
                }
                string ChangeTag = "Update upload_monitor set processed=1 where line=0";
                MySqlCommand update = new MySqlCommand(ChangeTag, con);
                update.ExecuteNonQuery();
                con.Close();
            }            
        }

        /*-----------------监测顺序表中是否有新数据上传--------------------*/
        public void online_monitor()
        {
            MySqlConnection con = new MySqlConnection(constr);
            con.Open();
            string SQLstr = "Select processed from upload_monitor";
            MySqlCommand commission = new MySqlCommand(SQLstr, con);
            MySqlDataAdapter adapter = new MySqlDataAdapter(commission);
            DataTable dt = new DataTable();
            adapter.Fill(dt);
            tag = Convert.ToInt16(dt.Rows[0]["processed"]);
            if (tag == 0) Console.WriteLine("new sequence to be processed!");
            con.Close();
        }

        /*-----------------为monitor和upload创建线程--------------------*/
        public void uploadnewthread()
        {
            upload = new Thread(delegate()
                {                    
                    read_sequence();                    
                });
            upload.Start();
            System.Threading.Thread.Sleep(200);
            upload.Abort();
        }

        public void monitornewthread()
        {
            monitor = new Thread(delegate()
            {
                online_monitor();                
            });
            monitor.Start();
            System.Threading.Thread.Sleep(200);
            monitor.Abort();
        }
    }
}




namespace AGV2database_test
{
    class Program
    {
        static void Main(string[] args)
        {
            demo.Test dbtest = new demo.Test();
            dbtest.conDB();
            DateTime startTime = DateTime.Now;
            while (true)
            {
                dbtest.uploadnewthread();
                System.Threading.Thread.Sleep(100);
                dbtest.monitornewthread();
                System.Threading.Thread.Sleep(100);
                Application.DoEvents();
                if (DateTime.Now - startTime > TimeSpan.FromMinutes(3)) break;
            }
                Console.ReadLine();
        }
    }
}
