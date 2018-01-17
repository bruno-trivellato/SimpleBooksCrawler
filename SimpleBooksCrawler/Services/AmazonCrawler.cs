using HtmlAgilityPack;
using SimpleBooksCrawler.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public class AmazonCrawler : Crawler, ICrawler
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

            if (bookNode != null)
            {
                String detailsUrl = FindDetailsPageUrl(bookNode);

                if (detailsUrl != null)
                {
                    book.DetailsUrl = detailsUrl;
                    Boolean crawlOutput = await CrawlBooksMetadata(book, detailsUrl);

                    if (crawlOutput == true)
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
                response = await this.httpClient.GetAsync(String.Format("https://www.amazon.com/gp/aw/s/ref=is_s?n=283155&n=283155&k={0}", book.ISBN));
            }
            catch (HttpRequestException ex)
            {
                Log(String.Format("[Error] Failed to retrieve search page html. {2}. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException, book.ISBN));
                
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
            if (resultsListNode.ChildNodes.Count == 1)
            {
                return 0;
            }

            return resultsListNode.SelectNodes("//li[contains(@class, 'sx-table-item')]").Count;
            
        }

        /// <summary>
        /// Gets the result's list node from a search page.
        /// </summary>
        /// <param name="searchPageHtml"></param>
        /// <returns></returns>
        private HtmlNode GetResultsListNode(HtmlDocument searchPageHtml)
        {
            HtmlNode resultsListNode = searchPageHtml.GetElementbyId("resultItems");

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
            if(searchPageHtml != null)
            {
                HtmlNode resultsListNode = GetResultsListNode(searchPageHtml);

                if(resultsListNode != null)
                {
                    Int32 searchResults = CountSearchResults(resultsListNode);

                    if (searchResults == 1)
                    {
                        return resultsListNode.SelectSingleNode("//li[contains(@class, 'sx-table-item')][1]");
                    }
                    else if (searchResults > 1)
                    {
                        // TODO: handle if more than one book was found.
                        Log(String.Format("[ISBN: {0}] [Warning] More than one result list was found.", book.ISBN));
                        SaveFailedCrawl(searchPageHtml, book);

                    }
                    else if (searchResults == 0)
                    {
                        Log(String.Format("[ISBN: {0}] [Warning] No result was found.", book.ISBN));
                        SaveFailedCrawl(searchPageHtml, book);
                    }
                } else
                {
                    Log(String.Format("[ISBN: {0}] [Error] Results List Node wasn't found on page.", book.ISBN));
                    SaveFailedCrawl(searchPageHtml, book);
                }

            }
            

            return null;
        }

        private String FindDetailsPageUrl(HtmlNode bookNode)
        {
            HtmlNode detailsUrlNode = bookNode.SelectSingleNode(bookNode.XPath + "/a[1]");
            String hrefAttribute = detailsUrlNode.Attributes.Where(attr => attr.Name == "href").First().Value;

            return String.Format("https://www.amazon.com{0}", hrefAttribute);
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

                Log(String.Format("Failed to crawl metadata on book {2}. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException, book.ISBN));
                return false;
            }

            String responseAsString = await response.Content.ReadAsStringAsync();

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseAsString);

            CrawlBooksTitle(html, book);
            CrawlBooksAuthor(html, book);
            CrawlBooksPublisher(html, book);
            CrawlBooksYear(html, book);
            CrawlBooksDescription(html, book);

            return true;
        }

        private Boolean CrawlBooksTitle(HtmlDocument booksHtmlPage, Book book)
        {
            HtmlNode titleNode = booksHtmlPage?.GetElementbyId("title");
            if(titleNode != null)
            {
                book.Title = HttpUtility.HtmlDecode(titleNode.InnerText).Trim();
                return true;
            }

            book.Title = "NOT FOUND";
            return false;
            
        }

        private Boolean CrawlBooksAuthor(HtmlDocument booksHtmlPage, Book book)
        {
            HtmlNode authorsNode = booksHtmlPage.GetElementbyId("byline");
            String booksAuthor = "";
            if (authorsNode != null)
            {
                if(authorsNode.Descendants().Count() == 4) // One author scenario
                {
                    booksAuthor = authorsNode.ChildNodes[1].InnerText.Trim();
                    

                } else if(authorsNode.Descendants().Count() > 4) // Two authors or more scenario
                {
                    // count == 16
                    booksAuthor = authorsNode.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText.Trim();
                }

                book.Author = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(booksAuthor.ToLower());
                return true;

            }

            booksAuthor = "NOT FOUND";
            return false;
        }

        private Boolean CrawlBooksPublisher(HtmlDocument booksHtmlPage, Book book)
        {
            HtmlNode booksProductInformationTableNode = booksHtmlPage.GetElementbyId("productDetails_techSpec_section_1");

            if(booksProductInformationTableNode != null)
            {
                HtmlNode publisherHeaderNode = booksProductInformationTableNode.SelectSingleNode(booksProductInformationTableNode.XPath + "//tr/th/text()[contains(.,'Publisher')]");

                if(publisherHeaderNode != null)
                {
                    HtmlNode publisherValueNode = publisherHeaderNode.ParentNode.ParentNode.ChildNodes[3];

                    book.Publisher = publisherValueNode.InnerText.Trim();
                    return true;
                }
                
            }

            book.Publisher = "NOT FOUND";
            return false;    
        }

        private Boolean CrawlBooksYear(HtmlDocument booksHtmlPage, Book book)
        {
            HtmlNode booksProductInformationTableNode = booksHtmlPage.GetElementbyId("productDetails_techSpec_section_1");

            if(booksProductInformationTableNode != null)
            {
                HtmlNode yearHeaderNode = booksProductInformationTableNode.SelectSingleNode(booksProductInformationTableNode.XPath + "//tr/th/text()[contains(.,'Publication date')]");

                if(yearHeaderNode != null)
                {
                    HtmlNode yearValueNode = yearHeaderNode.ParentNode.ParentNode.ChildNodes[3];

                    String yearValueText = yearValueNode.InnerText.Trim();

                    DateTime dateValue;
                    if (DateTime.TryParse(yearValueText, out dateValue))
                    {
                        book.Year = dateValue.Year;
                        return true;
                    }
                    else
                    {

                        Log(String.Format("[Warning] Not a date on book of ISBN: {0}. Crawled the following: '{1}'", book.ISBN, yearValueText));
                    }
                }
            }

            // Year not available on Amazon.com
            return false;
            
        }

        private Boolean CrawlBooksDescription(HtmlDocument booksHtmlPage, Book book)
        {
            HtmlNode booksDescriptionNode = booksHtmlPage.GetElementbyId("productDescription_fullView");
            if(booksDescriptionNode != null)
            {
                book.Description = HttpUtility.HtmlDecode( booksDescriptionNode.ChildNodes[1].InnerText.Trim() );
                return true;
            }


            book.Description = "NOT FOUND";
            return false;
            
        }
        
    }
}
