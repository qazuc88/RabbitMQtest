using RabbitMQConsumer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumerConsole
{
    class Program
    {
        static log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");
        static void Main(string[] args)
        {
            for (var i = 0; i < 100; i++)
            {
                //System.Threading.ThreadPool.QueueUserWorkItem(delegate
                //{
                //    Consumer.Receive();
                //});
            }
            MutilConsumeService2();

            showcount();
            while ("q" != Console.ReadLine())
            {
                lock (locker)
                {
                    keycount++;
                }

                //   Console.WriteLine("{0}个任务,上次成功运行了{1}次,总运行{2}次", keycount, tempcount, totalcount);
                run();
            }
            ;
        }
        static BlockingCollection<int> ls = new BlockingCollection<int>(128);
        private static void run()
        {
            Console.WriteLine("run()");
            System.Threading.Thread th = new System.Threading.Thread(new ThreadStart(delegate
            {
                new Action(() =>
                {
                    try
                    {

                        Console.WriteLine(string.Format("Action(() =>，IsBackground{0}", Thread.CurrentThread.IsBackground));
                        HttpClient wclient = new HttpClient();
                        wclient.Timeout = new TimeSpan(0, 0, 0, 1);
                        var task = wclient.GetStringAsync("https://www.telerik.com/fiddler");
                       // Thread.Sleep(10000);
                        Loginfo.Info("action end");

                        string str = task.Result;
                        Console.WriteLine(str);
                        Console.WriteLine("action end");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        Loginfo.Error(ex);
                    }
                }).BeginInvoke(null, null);
                Thread.CurrentThread.Abort();
                for (var i = 0; i < 100; i++)
                {
                    try
                    {
                        ls.Add(i);
                    }
                    catch (Exception ex)
                    {
                        Loginfo.Error(string.Format("endid={0}", ex.ToString()), ex);
                    }
                    //System.Threading.ThreadPool.QueueUserWorkItem(delegate
                    //{
                    //    Consumer.Receive();
                    //});
                }
            }));
            th.IsBackground = false;
            th.Start();
        }
        private static void showcount()
        {
            Task.Factory.StartNew(() =>
               {
                   while (true)
                   {
                       int tempcount = 0;
                       // Loginfo.Info(string.Format("{0}", count));
                       lock (locker)
                       {
                           tempcount = count;
                           // totalcount += tempcount;
                           count = 0;
                       }
                       Console.WriteLine("{0}个任务,上次成功运行了{1}次,总运行{2}次", keycount, tempcount, totalcount);
                       System.Threading.Thread.Sleep(1000);
                   }
               });

        }
        private static void MutilConsumeService()
        {
            Task.Factory.StartNew(() =>
            {
                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 128
                };
                foreach (var item in ls)
                {

                }
                int i = 0;
                Parallel.ForEach(ls.GetConsumingEnumerable(), options, row =>
                {
                    i++;
                    try
                    {
                        print(row);
                    }
                    catch (Exception ex)
                    {

                        Loginfo.Error(string.Format("{0},{1}", row, ex.Message));
                    }
                }
                );


            });
        }

        private static void MutilConsumeService2()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var row in ls.GetConsumingEnumerable())
                {
                    Task.Factory.StartNew(() =>
                        {

                            try
                            {
                                print(row);
                            }
                            catch (Exception ex)
                            {

                                Loginfo.Error(string.Format("{0},{1}", row, ex.Message));
                            }
                        }
                        );
                }
            });
        }
        static int totalcount = 0;
        static int count = 0;
        static int keycount = 0;
        static object locker = new object();
        private static void print(int id)
        {
            // Loginfo.Info(string.Format("{0}", id, Thread.CurrentThread.Name, Thread.CurrentThread.IsBackground,ls.Count));

            Loginfo.Info(string.Format("{0},t={1},IsBackground={2},ls.count()={3}", id, Thread.CurrentThread.Name, Thread.CurrentThread.IsBackground, ls.Count));
            System.Threading.Thread.Sleep(10);
            lock (locker)
            {
                count++;
                totalcount++;
            }
            if (id == 15)
            {
                //  throw new Exception("搞点事");
            }
            // Loginfo.Info(string.Format("{0}", id));
        }
    }
}
