using log4net;
using log4net.Repository.Hierarchy;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace RabbitMQConsumer
{
    public class Consumer
    {
      static  log4net.ILog Loginfo = log4net.LogManager.GetLogger("loginfo");
        public static void Receive()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("");
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    //var consumer = new QueueingBasicConsumer(channel);
                    //channel.BasicConsume("task_queue", false, consumer);
                    //while (true)
                    //{
                    //    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                    //    var body = ea.Body;
                    //    var message = Encoding.UTF8.GetString(body);

                    //    int dots = message.Split('.').Length - 1;
                    //    //Thread.Sleep(dots * 1000);

                    //    channel.BasicAck(ea.DeliveryTag, false);
                    //    witelog(message);
                    //}
                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume("task_queue", false, consumer);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        witelog(message);
                        channel.BasicAck(ea.DeliveryTag, false);
                    };
                }
            }
        }
        static async void witelog(string message)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                //System.IO.File.AppendAllText("d:\\test.log", string.Format("Received {0}", message));
                Console.WriteLine("{0} Received", message);
                Console.WriteLine("Done");
                Loginfo.Info(string.Format("Received {0}", message));
            });
        }
    }
}