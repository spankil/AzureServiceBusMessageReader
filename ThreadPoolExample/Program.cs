using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPoolExample
{
    class Program
    {
        static void Test(string[] args)
        {
            Console.WriteLine("Do work started.");
            DoWork(true);
            Console.ReadLine();
        }

        public static void DoWork()
        {
            System.Threading.ThreadPool.SetMaxThreads(5, 5);

            for (int i = 0; i < 20; i++)
            {
                // Queue a task.  
                System.Threading.ThreadPool.QueueUserWorkItem(
                    new System.Threading.WaitCallback(SomeLongTask), i.ToString());
                //// Queue another task.  
                //System.Threading.ThreadPool.QueueUserWorkItem(
                //    new System.Threading.WaitCallback(AnotherLongTask), i.ToString());
            }
            Console.WriteLine("Do work Completed." + DateTime.Now.ToString());
        }

        private static void SomeLongTask(Object state)
        {
            Console.WriteLine(string.Format("Some Long Tast started. {0}. Thread: {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), state));
            System.Threading.Thread.Sleep(30000);
            Console.WriteLine(string.Format("Some Long Tast completed. {0}. Thread: {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), state));
            // Insert code to perform a long task.  
        }

        private static void AnotherLongTask(Object state)
        {
            BrokeredMessage msg = ((BrokeredMessage)state);
            string text = "";
            foreach (var prop in msg.Properties)
            {
                text += string.Format("{0}\t{1}\n", prop.Key, prop.Value);
            }
            Console.WriteLine(string.Format("{0}: {1} - Thread: {2}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), text, msg.CorrelationId));
            //msg.Complete();
            System.Threading.Thread.Sleep(60000);
        }

        public static void DoWork(bool SB)
        {
            System.Threading.ThreadPool.SetMaxThreads(10, 10);
            var connectionString = "Endpoint=sb://servicebus-namespace-2.servicebus.windows.net/;SharedAccessKeyName=servicebus-env-002-servicebus-namespace-2-pankil;SharedAccessKey=856mIAhotFSpkSXA0uts2uXmn4Ud87k9w21Vb5ZsBew=";
            var topicName = "main-topic-env-002-servicebus-namespace-2";
            var subscriptionName = "AllocationOrchestration_Pankil/$DeadLetterQueue";
            SubscriptionClient subscriptionClient = null;
            //int i = 0;
            while (true)
            {
                try
                {
                    OnMessageOptions options = new OnMessageOptions();
                    options.AutoComplete = false;
                    options.MaxConcurrentCalls = 10;

                    subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);
                    
                    subscriptionClient.OnMessage(msg =>
                    {
                        Console.WriteLine(string.Format("{0}: Thread Started: {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), msg.CorrelationId));
                        System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(AnotherLongTask), msg);
                    }, options);
                }
                catch (Exception ex)
                {
                    //Log Error
                }
                finally
                {
                    subscriptionClient = null;
                }
            }
        }
    }
}
