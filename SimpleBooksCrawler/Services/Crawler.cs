using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Services
{
    public abstract class Crawler
    {
        protected virtual void Log(String message)
        {
            Trace.WriteLine(String.Format("[{0}] {1}", this.GetType().Name, message));
        }
    }
}
