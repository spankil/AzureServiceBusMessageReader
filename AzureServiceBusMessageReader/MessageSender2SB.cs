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
    public partial class MessageSender2SB : Form
    {
        public MessageSender2SB()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendMessage("OSCS", "ALCS");
        }

        public void SendMessage(string source, string dest)
        {
            var connectionString = textBox1.Text;
            var topicName = textBox2.Text;

            var client = TopicClient.CreateFromConnectionString(connectionString, topicName);

            var message = new BrokeredMessage();
            message.Properties["eventID"] = 1;
            message.Properties["eventSource"] = source;
            message.Properties["eventDestination"] = dest;
            message.Properties["eventType"] = "SupplyChainStockUpdate";
            message.Properties["eventData"] = "Test" + DateTime.Now.Ticks;

            client.Send(message);
        }
    }
}
