using SimpleBooksCrawler.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Models
{
    public class Book : BindableBase
    {

        private BookState _BookState;
        public BookState BookState
        {
            get { return _BookState; }
            set
            {
                SetProperty(ref _BookState, value);
            }
        }


        private long _ISBN;
        public long ISBN
        {
            get { return _ISBN; }
            set
            {
                SetProperty(ref _ISBN, value);
            }
        }


        private String _Title;
        public String Title
        {
            get { return _Title; }
            set
            {
                SetProperty(ref _Title, value);
            }
        }

        private String _Author;
        public String Author
        {
            get { return _Author; }
            set
            {
                SetProperty(ref _Author, value);
            }
        }

        private int _Edition;
        public int Edition
        {
            get { return _Edition; }
            set
            {
                SetProperty(ref _Edition, value);
            }
        }


        private int _Year;
        public int Year
        {
            get { return _Year; }
            set
            {
                SetProperty(ref _Year, value);
            }
        }


        private String _Editor;
        public String Editor
        {
            get { return _Editor; }
            set
            {
                SetProperty(ref _Editor, value);
            }
        }


        private String _Publisher;
        public String Publisher
        {
            get { return _Publisher; }
            set
            {
                SetProperty(ref _Publisher, value);
            }
        }

        private String _DetailsUrl;
        public String DetailsUrl
        {
            get { return _DetailsUrl; }
            set
            {
                SetProperty(ref _DetailsUrl, value);
            }
        }

        private String _Description;
        public String Description
        {
            get { return _Description; }
            set
            {
                SetProperty(ref _Description, value);
            }
        }

        public String HtmlDetailsPage { get; set; }

    }

    public enum BookState
    {
        WaitingToBeCrawled,
        OnCrawling,
        Crawled,
        CrawlFailed
    }
}
