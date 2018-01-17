using SimpleBooksCrawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Services
{
    public interface ICrawler
    {
        Task<Boolean> CrawlBookAsync(Book book);


    }
}
