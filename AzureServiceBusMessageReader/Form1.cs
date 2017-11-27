using System;
using System.Windows.Forms;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using System.Threading;
using System.IO;

namespace AzureServiceBusMessageReader
{
    public partial class Form1 : Form
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            ParameterizedThreadStart paraThread = new ParameterizedThreadStart(Run);
            int threadCount = Convert.ToInt32(textBox4.Text);
            for (int i = 0; i < threadCount; i++)
            {
                Thread th = new Thread(paraThread);
                th.Name = "Thread #" + i.ToString();
                th.Start(th.Name);
            }

            button1.Enabled = true;
        }

        private void Run(object obj)
        {
            string threadName = obj.ToString();
            var connectionString = textBox1.Text; //System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var topicName = textBox2.Text;  //System.Configuration.ConfigurationManager.AppSettings["TopicName"];
            var subscriptionName = textBox3.Text;  //System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);

            int i = 0;
            subscriptionClient.OnMessage(msg =>
            {
                Console.WriteLine(threadName + "_" + i++);

                label5.BeginInvoke((Action)delegate ()
                {
                    label5.Text = threadName + "_" + i++;
                    Application.DoEvents();
                });

                //Console.WriteLine(msg.Properties["eventData"].ToString());
                string text = threadName + "_" + i++ + "\n";
                foreach (var prop in msg.Properties)
                {
                    text += string.Format("{0}\t{1}\n", prop.Key, prop.Value);
                }
                WriteToFileThreadSafe(text + "\n", "QueueMessages.txt");
                msg.Complete();
            });

            label5.BeginInvoke((Action)delegate ()
            {
                label5.Text = threadName + "_" + i++ + " ... Done.";
            });
        }

        private void Run1(object obj)
        {
            string threadName = obj.ToString();
            var connectionString = textBox1.Text; //System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var topicName = textBox2.Text;  //System.Configuration.ConfigurationManager.AppSettings["TopicName"];
            var subscriptionName = textBox3.Text;  //System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);

            // Accept a message session and handle any timeout exceptions.
            MessageSession orderSession = null;
            while (orderSession == null)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Accepting message session...");

                    // Accept the message session
                    orderSession = subscriptionClient.AcceptMessageSession();

                    Console.WriteLine("Session accepted.");
                }
                catch (TimeoutException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            int i = 0;
            // Receive the order batch tag read messages.               
            while (true)
            {
                //BrokeredMessage receivedTagRead = queueClient.Receive(TimeSpan.FromSeconds(5));

                // Receive the tag read messages from the message session.
                BrokeredMessage receivedTagRead = orderSession.Receive(TimeSpan.FromSeconds(5));

                if (receivedTagRead != null)
                {
                    //Console.WriteLine(msg.Properties["eventData"].ToString());
                    string text = threadName + "_" + i++ + "\n";
                    foreach (var prop in receivedTagRead.Properties)
                    {
                        text += string.Format("{0}\t{1}\n", prop.Key, prop.Value);
                    }

                    // Mark the message as complete
                    receivedTagRead.Complete();
                }
                else
                {
                    break;
                }
            }

            //int i = 0;
            //subscriptionClient.OnMessage(msg =>
            //{
            //    Console.WriteLine(threadName + "_" + i++);

            //    label5.BeginInvoke((Action)delegate ()
            //    {
            //        label5.Text = threadName + "_" + i++;
            //        Application.DoEvents();
            //    });

            //    //Console.WriteLine(msg.Properties["eventData"].ToString());
            //    string text = threadName + "_" + i++ + "\n";
            //    foreach (var prop in msg.Properties)
            //    {
            //        text += string.Format("{0}\t{1}\n", prop.Key, prop.Value);
            //    }
            //    WriteToFileThreadSafe(text + "\n", "QueueMessages.txt");
            //    msg.Complete();
            //});

            //label5.BeginInvoke((Action)delegate ()
            //{
            //    label5.Text = threadName + "_" + i++ + " ... Done.";
            //});
        }

        public void WriteToFileThreadSafe(string text, string path)
        {
            // Set Status to Locked
            _readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                File.AppendAllText(path, text);
            }
            finally
            {
                // Release lock
                _readWriteLock.ExitWriteLock();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            textBox2.Text = System.Configuration.ConfigurationManager.AppSettings["TopicName"];
            textBox3.Text = System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];
            textBox4.Text = System.Configuration.ConfigurationManager.AppSettings["ThreadCount"];
            textBox5.Text = System.Configuration.ConfigurationManager.AppSettings["SQLFilterExpression"];
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var connectionString = textBox1.Text; //System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var topicName = textBox2.Text;  //System.Configuration.ConfigurationManager.AppSettings["TopicName"];
            var subscriptionName = textBox3.Text;  //System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];

            RuleDescription rule = new RuleDescription();
            SqlFilter sqlFilter = new SqlFilter(textBox5.Text); //textBox5.Text = "eventDestination LIKE '%ALCO%'";            
            rule.Filter = sqlFilter;
            rule.Name = "EventDestination";

            if (checkBox1.Checked == false)
            {
                var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);
                try
                {
                    subscriptionClient.RemoveRule("$Default");
                    subscriptionClient.RemoveRule("EventDestination");
                }
                catch
                {
                    //Ignore
                }
                subscriptionClient.AddRule("EventDestination", sqlFilter);
            }
            else
            {
                var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
                namespaceManager.CreateSubscription(topicName, subscriptionName, rule); //401. Unathorized Error.
                //MessageBox.Show("Under Construction!");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var connectionString = textBox1.Text; //System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            var topicName = textBox2.Text;  //System.Configuration.ConfigurationManager.AppSettings["TopicName"];
            var subscriptionName = textBox3.Text;  //System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];

            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.TopicExists(topicName))
            {
                // Configure Topic Settings.
                var td = new TopicDescription(topicName);
                td.MaxSizeInMegabytes = 1024;
                td.DefaultMessageTimeToLive = TimeSpan.FromMinutes(5);

                namespaceManager.CreateTopic(td);
            }

            if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
            {
                namespaceManager.CreateSubscription(topicName, subscriptionName);
            }

            var sub1Name = "ALCO";
            var sub2Name = "ALCS";

            //Create Subscription without filter
            //----------------------------------------------------------------
            if (!namespaceManager.SubscriptionExists(topicName, sub1Name))
            {
                namespaceManager.CreateSubscription(topicName, sub1Name);
            }

            if (!namespaceManager.SubscriptionExists(topicName, sub2Name))
            {
                namespaceManager.CreateSubscription(topicName, sub2Name);
            }
            //----------------------------------------------------------------

            //Create Subscription with filter
            //----------------------------------------------------------------
            //if (namespaceManager.SubscriptionExists(topicName, sub1Name))
            //{
            //    Console.WriteLine("Deleting subscription {0}", sub1Name);
            //    namespaceManager.DeleteSubscription(topicName, sub1Name);
            //}
            //Console.WriteLine("Creating subscription {0}", sub1Name);
            //namespaceManager.CreateSubscription(topicName, sub1Name, new SqlFilter("eventDestination LIKE '%ALCO%'"));

            //if (namespaceManager.SubscriptionExists(topicName, sub2Name))
            //{
            //    Console.WriteLine("Deleting subscription {0}", sub2Name);
            //    namespaceManager.DeleteSubscription(topicName, sub2Name);
            //}
            //Console.WriteLine("Creating subscription {0}", sub2Name);
            //namespaceManager.CreateSubscription(topicName, sub2Name, new SqlFilter("eventDestination LIKE '%ALCS%'"));
            //----------------------------------------------------------------
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CreateSubscriptions createSubscriptions = new CreateSubscriptions();
            createSubscriptions.TopicName = textBox2.Text;
            createSubscriptions.ConnectionString = textBox1.Text;
            createSubscriptions.Show();
        }
    }
}

//**** WRITE MESSAGE****//
//<add key = "Microsoft.ServiceBus.ConnectionString" value="Endpoint=sb://servicebus-namespace-2.servicebus.windows.net/;SharedAccessKeyName=servicebus-env-002-servicebus-namespace-2-pankil;SharedAccessKey=856mIAhotFSpkSXA0uts2uXmn4Ud87k9w21Vb5ZsBew="/>
//var client = TopicClient.CreateFromConnectionString(connectionString, topicName);
//var message = new BrokeredMessage("this is a test message!");

////MessageBox.Show(string.Format("message id: {0}", message.MessageId));

//client.Send(message);

//MessageBox.Show("message successfully sent!");
//****END WRITE MESSAGE****//