//using Microsoft.ServiceBus.Messaging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ThreadPoolExample
//{
//    public class AsyncMessageReceiver
//    {
//        public static void Test(string[] args)
//        {
//            var connectionString = "Endpoint=sb://servicebus-namespace-2.servicebus.windows.net/;SharedAccessKeyName=servicebus-env-002-servicebus-namespace-2-pankil;SharedAccessKey=856mIAhotFSpkSXA0uts2uXmn4Ud87k9w21Vb5ZsBew=";
//            var topicName = "main-topic-env-002-servicebus-namespace-2";
//            var subscriptionName = "AllocationScheduling/$DeadLetterQueue";

//            OnMessageOptions options = new OnMessageOptions
//            {
//                MaxConcurrentCalls = 10,
//                AutoComplete = false
//            };

//            SubscriptionClient client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);

//            options.ExceptionReceived += OnMessageError;
//            client.OnMessageAsync(OnMessageReceived, options);

//        }

//        private static void OnMessageError(object sender, ExceptionReceivedEventArgs e)
//        {
//            if (e != null && e.Exception != null)
//            {
//                Console.WriteLine("Hey, there's an error!" + e.Exception.Message + "\r\n\r\n");
//            }
//        }

//        private static async Task OnMessageReceived(BrokeredMessage arg)
//        {
//            System.Threading.Thread.Sleep(4000);
//            Console.WriteLine(string.Format("Message processing done! {0}. {1}", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff"), arg.CorrelationId));
//            await arg.CompleteAsync();
//        }


//        private ManualResetEvent pauseProcessingEvent;
//        public void ReceiveMessages(Func<BrokeredMessage, Task> processMessageTask)
//        {
//            var connectionString = "Endpoint=sb://servicebus-namespace-2.servicebus.windows.net/;SharedAccessKeyName=servicebus-env-002-servicebus-namespace-2-pankil;SharedAccessKey=856mIAhotFSpkSXA0uts2uXmn4Ud87k9w21Vb5ZsBew=";
//            var topicName = "main-topic-env-002-servicebus-namespace-2";
//            var subscriptionName = "AllocationScheduling/$DeadLetterQueue";

//            // Set up the options for the message pump.
//            var options = new OnMessageOptions();

//            // When AutoComplete is disabled it's necessary to manually
//            // complete or abandon the messages and handle any errors.
//            options.AutoComplete = false;
//            options.MaxConcurrentCalls = 10;
//            options.ExceptionReceived += this.OptionsOnExceptionReceived;
//            SubscriptionClient client = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);
//            // Use of the Service Bus OnMessage message pump.
//            // The OnMessage method must be called once, otherwise an exception will occur.
//            client.OnMessageAsync(
//              async (msg) =>
//              {
//                  // Will block the current thread if Stop is called.
//                  this.pauseProcessingEvent.WaitOne();

//                  // Execute processing task here.
//                  await processMessageTask(msg);
//              },
//              options);
//        }

//        private void OptionsOnExceptionReceived(object sender,
//          ExceptionReceivedEventArgs exceptionReceivedEventArgs)
//        {
//            //Test
//        }

//    }
//}


using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
class Program
{
    static int i = 0;
    static void Main(string[] args)
    {        
        // Build the messaging options.
        var eventDrivenMessagingOptions = new OnMessageOptions();
        eventDrivenMessagingOptions.AutoComplete = false;
        eventDrivenMessagingOptions.ExceptionReceived += OnExceptionReceived;
        eventDrivenMessagingOptions.MaxConcurrentCalls = 10;

        var connectionString = "Endpoint=sb://servicebus-namespace-2.servicebus.windows.net/;SharedAccessKeyName=servicebus-env-002-servicebus-namespace-2-pankil;SharedAccessKey=856mIAhotFSpkSXA0uts2uXmn4Ud87k9w21Vb5ZsBew=";
        var topicName = "main-topic-env-002-servicebus-namespace-2";
        var subscriptionName = "AllocationOrchestration_Pankil/$DeadLetterQueue";
        SubscriptionClient subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);
        // Subscribe for messages.
        subscriptionClient.OnMessage(OnMessageArrived, eventDrivenMessagingOptions);

        // Wait.
        Console.ReadLine();
    }

    /// <summary>
    /// This event will be called each time a message arrives.
    /// </summary>
    /// <param name="message"></param>
    private static void OnMessageArrived(BrokeredMessage message)
    {
        i++;
        //var approval = message.GetBody<object>;

        Console.WriteLine(" > {0} - Received approval message for customer: {1} (Thread: {2})",
            DateTime.Now, message.LockToken, Thread.CurrentThread.ManagedThreadId);

        System.Threading.Thread.Sleep(10000);

        if (i%2==0)
        {
            message.Complete();
        }
        else
        {
            throw new InvalidOperationException("Error Occured");
        }
        Console.WriteLine(" > {0} - Completed message : {1} (Thread: {2})",
            DateTime.Now, message.LockToken, Thread.CurrentThread.ManagedThreadId);
    }

    /// <summary>
    /// Event handler for each time an error occurs.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void OnExceptionReceived(object sender, ExceptionReceivedEventArgs e)
    {
        if (e != null && e.Exception != null)
        {
            Console.WriteLine(" > Exception received: {0}", e.Exception.Message);
        }
    }
}