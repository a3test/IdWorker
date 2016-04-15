using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowflakeTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IdWorker idWorker = new IdWorker(1, 1);
            int len = 200;
            long[] ids = new long[len];

            for (int i = 0; i < len; i++)
            {
                ids[i] = idWorker.NextId();
                //Thread.Sleep(1);
            }
            foreach (long id in ids)
            {
                Console.WriteLine("{0}", Convert.ToString(id));
            }
        }
    }
}