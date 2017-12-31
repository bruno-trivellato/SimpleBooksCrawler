using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace SimpleBooksCrawler.Models
{
    public class StringTraceListener : TraceListener
    {
        public StringBuilder output;
        public String LastMessage { get; set; }

        public event EventHandler<String> TraceUpdated;

        private void OnTraceUpdated(object sender, string message)
        {
            var handler = TraceUpdated;
            if(handler != null)
            {
                TraceUpdated(sender, message);
            }
        }

        public StringTraceListener(StringBuilder output)
        {
            this.Name = "Trace";
            this.output = output;
        }

        public override void Write(string message)
        {
            
            Action append = delegate () {
                
                var messageWithTime = String.Format("[{0}] {1}", DateTime.Now.ToString(), message);
                output.Append(messageWithTime);

                this.LastMessage = messageWithTime;
                OnTraceUpdated(this, messageWithTime);
                
            };
            append();
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}
