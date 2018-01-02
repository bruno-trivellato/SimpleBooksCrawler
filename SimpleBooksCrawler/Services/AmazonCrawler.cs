using HtmlAgilityPack;
using SimpleBooksCrawler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SimpleBooksCrawler.Services
{
    /// <summary>
    /// Represents a crawler for the Amazon.com website that will crawl for a specific book at a time.
    /// </summary>
    public class AmazonCrawler : ICrawler
    {
        private HttpClient httpClient;

        public AmazonCrawler()
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; U; Android 4.0.3; ko-kr; LG-L160L Build/IML74K) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30");

        }

        /// <summary>
        /// /// Basic method for crawling a book on Amazon.com.
        /// This method consists of 3 steps:
        /// (1) Find the book on the search page.
        /// (2) Get details page url on the search page.
        /// (3) Get book's metadata on the details page.
        /// </summary>
        /// <param name="book"></param>
        /// <returns>True if successful, false otherwise.</returns>
        public async Task<Boolean> CrawlBookAsync(Book book)
        {
            HtmlNode bookNode = await FindBookNodeOnSearchPageAsync(book);

            if(bookNode != null)
            {
                String detailsUrl = FindDetailsPageUrl(bookNode);

                if (detailsUrl != null)
                {
                    book.DetailsUrl = detailsUrl;
                    Boolean crawlOutput = await CrawlBooksMetadata(book, detailsUrl);

                    if(crawlOutput == true)
                    {
                        book.BookState = BookState.Crawled;
                        return true;
                    }
                }
            }
            
            book.BookState = BookState.CrawlFailed;
            return false;
        }
        

        private async Task<HtmlDocument> RetrieveSearchPageHtmlAsync(Book book)
        {
            HttpResponseMessage response = null;
            try
            {
                response = await this.httpClient.GetAsync(String.Format("https://www.amazon.com/gp/search/ref=sr_adv_b/?search-alias=stripbooks&unfiltered=1&field-keywords=&field-author=&field-title=&field-isbn={0}&field-publisher=&node=&field-p_n_condition-type=&p_n_feature_browse-bin=&field-age_range=&field-language=&field-dateop=During&field-datemod=&field-dateyear=&sort=relevanceexprank&Adv-Srch-Books-Submit.x=52&Adv-Srch-Books-Submit.y=6", book.ISBN));
            }
            catch (HttpRequestException ex)
            {
                Trace.WriteLine(String.Format("Failed to crawl metadata. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException));
                return null;
            }

            String responseAsString = await response.Content.ReadAsStringAsync();

            var searchPageHtml = new HtmlDocument();
            searchPageHtml.LoadHtml(responseAsString);

            return searchPageHtml;

        }

        /// <summary>
        /// Returns the number of results in a search page.
        /// </summary>
        /// <param name="resultsListNode">Node containing the result's list</param>
        /// <returns></returns>
        private Int32 CountSearchResults(HtmlNode resultsListNode)
        {
            if (resultsListNode != null)
            {
                return resultsListNode.ChildNodes.Count;
            }

            return 0;
        }

        /// <summary>
        /// Gets the result's list node from a search page.
        /// </summary>
        /// <param name="searchPageHtml"></param>
        /// <returns></returns>
        private HtmlNode GetResultsListNode(HtmlDocument searchPageHtml)
        {
            HtmlNode resultsListNode = searchPageHtml.GetElementbyId("s-results-list-atf");

            return resultsListNode;
        }

        /// <summary>
        /// Finds the book node on the search page, if not found returns null.
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        private async Task<HtmlNode> FindBookNodeOnSearchPageAsync(Book book)
        {
            HtmlDocument searchPageHtml = await RetrieveSearchPageHtmlAsync(book);
            HtmlNode resultsListNode = GetResultsListNode(searchPageHtml);

            if (CountSearchResults(resultsListNode) == 1)
            {
                return resultsListNode.ChildNodes.First();
            }

            return null;
        }

        private String FindDetailsPageUrl(HtmlNode bookNode)
        {
            HtmlNode detailsUrlNode = bookNode.SelectSingleNode("//a[contains(@class, 's-access-detail-page')]");

            return detailsUrlNode.Attributes.Where(attr => attr.Name == "href").First().Value;
        }

        private async Task<Boolean> CrawlBooksMetadata(Book book, String detailsUrl)
        {
            HttpResponseMessage response = null;
            // Lets get all info available
            try
            {
                response = await this.httpClient.GetAsync(String.Format(book.DetailsUrl));
            }
            catch (HttpRequestException ex)
            {
                Trace.WriteLine(String.Format("Failed to crawl metadata. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException) );
                return false;
            }

            String responseAsString = await response.Content.ReadAsStringAsync();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseAsString);

            // Get Title
            HtmlNode titleNode = html.GetElementbyId("productTitle");
            book.Title = HttpUtility.HtmlDecode(titleNode.InnerText);

            // Get Author
            HtmlNode authorNode = html.DocumentNode.SelectSingleNode("//span[contains(@class, 'author')][1]/span[1]/a[1]");
            book.Author = authorNode.InnerText;

            // Get Publisher
            HtmlNode publisherNode = html.DocumentNode.SelectSingleNode("//li[b='Publisher:']/text()");
            book.Publisher = publisherNode.InnerText.TrimStart();

            // Get Description

            HtmlNode descriptionNode = html.DocumentNode.SelectSingleNode("//div[@id='productDescription']//p");
            book.Description = descriptionNode.InnerText;


            return true;
        }
        
    }
}
