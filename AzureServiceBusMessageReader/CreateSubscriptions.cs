using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzureServiceBusMessageReader
{
    public partial class CreateSubscriptions : Form
    {
        public CreateSubscriptions()
        {
            InitializeComponent();
        }

        [DefaultValue("T2")]
        public string TopicName { get; set; }

        private string _connectionString = "Endpoint=sb://pazuresb.servicebus.windows.net/;SharedAccessKeyName=AccessKey;SharedAccessKey=j70qC/Z2gK6UMOXj4LbhARip5HfbrV7+bAHjXXwUhEc=";
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var connectionString = ConnectionString;
            var topicName = TopicName;

            var client = TopicClient.CreateFromConnectionString(connectionString, topicName);

            for (int i = 0; i < 5; i++)
            {
                var message = new BrokeredMessage();
                message.Properties["eventID"] = DateTime.Now.Ticks;
                message.Properties["eventSource"] = "ALCS";
                message.Properties["eventDestination"] = "ALCO";
                message.Properties["eventType"] = "ABC";
                message.Properties["eventData"] = "Test" + DateTime.Now.Ticks;

                client.Send(message);
            }

            for (int i = 0; i < 5; i++)
            {
                var message = new BrokeredMessage();
                message.Properties["eventID"] = DateTime.Now.Ticks;
                message.Properties["eventSource"] = "ALCO";
                message.Properties["eventDestination"] = "ALCS";
                message.Properties["eventType"] = "DEF";
                message.Properties["eventData"] = "Test" + DateTime.Now.Ticks;
                client.Send(message);
            }
        }
    }

    [Serializable]
    public class MyClass
    {
        public int P1 { get; set; }
        public long P2 { get; set; }
        public string eventSource { get; set; }
        public string eventDestination { get; set; }
        public string message { get; set; }
    }
}

