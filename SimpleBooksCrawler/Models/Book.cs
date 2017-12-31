using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Models
{
    public class Book
    {
        public long ISBN { get; set; }
        public String Title { get; set; }
        public String Author { get; set; }
        public int Edition { get; set; }
        public int Year { get; set; }
        public String Editor { get; set; }
        public String Publisher { get; set; }
        public String DetailsUrl { get; set; }
        public String Description { get; set; }
    }
}
