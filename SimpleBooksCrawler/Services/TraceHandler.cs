using SimpleBooksCrawler.Common;
using SimpleBooksCrawler.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SimpleBooksCrawler.Services
{
    /// <summary>
    /// Singleton class.
    /// </summary>
    public class TraceHandler : BaseService
    {
        private static TraceHandler _instance;

        public static TraceHandler Instance
        {
            get {
                if (_instance == null)
                {
                    _instance = new TraceHandler();
                }

                return _instance;
            }
            set { _instance = value; }
        }

        


        private StringTraceListener _traceListener;
        

        public StringTraceListener TraceListener
        {
            get { return _traceListener; }
            set { _traceListener = value; }
        }


        private TraceHandler()
        {
            
            this.TraceListener = new StringTraceListener(new StringBuilder());
            this.TraceListener.TraceUpdated += (sender, message) =>
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() => {
                            OnServicePropertyChanged(message, nameof(this.TraceListener));
                        }));
                
            };

            
        }
        
    }
}
