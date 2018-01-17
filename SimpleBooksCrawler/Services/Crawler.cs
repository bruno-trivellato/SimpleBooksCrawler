using HtmlAgilityPack;
using SimpleBooksCrawler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        

        protected virtual void SaveFailedCrawl(HtmlDocument htmlDocument, Book book)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string failedCrawlsDirectory = String.Format("{0}Failed Crawls\\", currentDir);
            string fileName = String.Format("{0} - ISBN {1}", DateTime.Now.ToString("yyyyMMddHHmmssfff"), book.ISBN);
            string fileFullPath = String.Format("{0}{1}", failedCrawlsDirectory, fileName);

            try
            {
                
                Directory.CreateDirectory(failedCrawlsDirectory);
                htmlDocument.Save(fileFullPath);
                
                Log("[Info] Failed Crawl HTML page saved successfully.");
            }
            catch (IOException ex)
            {
                Log(String.Format("[Error] Failed to save Failed Crawl HTML page. Message: {0}", ex.Message));
            }
        }
    }
}
