using System;
using System.Windows.Forms;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using System.Threading;
using System.IO;

namespace AzureServiceBusMessageReader
{
    public partial class ApplyRules : Form
    {
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public ApplyRules()
        {
            InitializeComponent();
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
            rule.Name = "$Default";

            //NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            //var rules = namespaceManager.GetRules(topicName, subscriptionName);

            var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, topicName, subscriptionName, ReceiveMode.PeekLock);
            try
            {
                subscriptionClient.RemoveRule("$Default");
                //subscriptionClient.RemoveRule("$EventDestination");
            }
            catch
            {
                //Ignore
            }
            subscriptionClient.AddRule("$Default", sqlFilter);
        }

        //private void button4_Click(object sender, EventArgs e)
        //{
        //    var connectionString = textBox1.Text; //System.Configuration.ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
        //    var topicName = textBox2.Text;  //System.Configuration.ConfigurationManager.AppSettings["TopicName"];
        //    var subscriptionName = textBox3.Text;  //System.Configuration.ConfigurationManager.AppSettings["SubscriptionName"];

        //    NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
        //    if (!namespaceManager.TopicExists(topicName))
        //    {
        //        // Configure Topic Settings.
        //        var td = new TopicDescription(topicName);
        //        td.MaxSizeInMegabytes = 1024;
        //        td.DefaultMessageTimeToLive = TimeSpan.FromMinutes(5);

        //        namespaceManager.CreateTopic(td);
        //    }

        //    if (!namespaceManager.SubscriptionExists(topicName, subscriptionName))
        //    {
        //        namespaceManager.CreateSubscription(topicName, subscriptionName);
        //    }

        //    var sub1Name = "ALCO";
        //    var sub2Name = "ALCS";

        //    //Create Subscription without filter
        //    //----------------------------------------------------------------
        //    if (!namespaceManager.SubscriptionExists(topicName, sub1Name))
        //    {
        //        namespaceManager.CreateSubscription(topicName, sub1Name);
        //    }

        //    if (!namespaceManager.SubscriptionExists(topicName, sub2Name))
        //    {
        //        namespaceManager.CreateSubscription(topicName, sub2Name);
        //    }
        //    //----------------------------------------------------------------

        //    //Create Subscription with filter
        //    //----------------------------------------------------------------
        //    //if (namespaceManager.SubscriptionExists(topicName, sub1Name))
        //    //{
        //    //    Console.WriteLine("Deleting subscription {0}", sub1Name);
        //    //    namespaceManager.DeleteSubscription(topicName, sub1Name);
        //    //}
        //    //Console.WriteLine("Creating subscription {0}", sub1Name);
        //    //namespaceManager.CreateSubscription(topicName, sub1Name, new SqlFilter("eventDestination LIKE '%ALCO%'"));

        //    //if (namespaceManager.SubscriptionExists(topicName, sub2Name))
        //    //{
        //    //    Console.WriteLine("Deleting subscription {0}", sub2Name);
        //    //    namespaceManager.DeleteSubscription(topicName, sub2Name);
        //    //}
        //    //Console.WriteLine("Creating subscription {0}", sub2Name);
        //    //namespaceManager.CreateSubscription(topicName, sub2Name, new SqlFilter("eventDestination LIKE '%ALCS%'"));
        //    //----------------------------------------------------------------
        //}

        private void button5_Click(object sender, EventArgs e)
        {
            CreateSubscriptions createsubscriptions = new CreateSubscriptions();
            createsubscriptions.TopicName = textBox2.Text;
            createsubscriptions.ConnectionString = textBox1.Text;
            createsubscriptions.Show();
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