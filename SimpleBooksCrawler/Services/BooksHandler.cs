using SimpleBooksCrawler.Models;
using HtmlAgilityPack;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace SimpleBooksCrawler.Services
{
    public class BooksHandler : BaseService
    {

        private static BooksHandler _instance;

        public static BooksHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BooksHandler();
                }

                return _instance;
            }
            set { _instance = value; }
        }

        private BooksHandler()
        {
            this.Books = new ObservableCollection<Book>();
        }


        private ObservableCollection<Book> _Books;
        public ObservableCollection<Book> Books
        {
            get { return _Books; }
            set
            {
                SetServiceProperty(ref _Books, value);
            }
        }

        public void LoadBooksFromCSV(String csvPath)
        {

            using (TextFieldParser csvParser = new TextFieldParser(csvPath))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    Book book = new Book();
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();

                    try
                    {
                        book.ISBN = long.Parse(fields[0]);
                    }
                    catch (FormatException ex)
                    {
                        this.Books.Clear();
                        MessageBox.Show("Failed to read ISBN at line " + ( csvParser.LineNumber - 1 ) + ". Exception: " + ex.Message, "Failed to read CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    catch (OverflowException ex)
                    {
                        this.Books.Clear();
                        MessageBox.Show("Failed to read ISBN at line " + (csvParser.LineNumber - 1) + ". Exception: " + ex.Message, "Failed to read CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }

                    //string Address = fields[1];

                    this.Books.Add(book);
                }
            }
        }

        public async void CrawlMetadataAsync()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");

            foreach (var book in this.Books)
            {
                HttpResponseMessage response = null;
                try
                {
                    response = await client.GetAsync(String.Format("https://www.amazon.com/gp/search/ref=sr_adv_b/?search-alias=stripbooks&unfiltered=1&field-keywords=&field-author=&field-title=&field-isbn={0}&field-publisher=&node=&field-p_n_condition-type=&p_n_feature_browse-bin=&field-age_range=&field-language=&field-dateop=During&field-datemod=&field-dateyear=&sort=relevanceexprank&Adv-Srch-Books-Submit.x=52&Adv-Srch-Books-Submit.y=6", book.ISBN));
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(String.Format("Failed to crawl metadata. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException), "Failed to crawl metadata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                String responseAsString = await response.Content.ReadAsStringAsync();

                var html = new HtmlDocument();
                html.LoadHtml(responseAsString);

                // Do we have a result?
                HtmlNode resultsListNode = html.GetElementbyId("s-results-list-atf");
                if(resultsListNode != null)
                {
                    // Do we have more than one?
                    if(resultsListNode.ChildNodes.Count == 1)
                    {
                        HtmlNode bookNode = resultsListNode.ChildNodes.First();

                        // Details Url
                        HtmlNode detailsUrlNode = bookNode.SelectSingleNode("//a[contains(@class, 's-access-detail-page')]");
                        book.DetailsUrl = detailsUrlNode.Attributes.Where(attr => attr.Name == "href").First().Value;

                        // Lets get all info available
                        try
                        {
                            response = await client.GetAsync(String.Format(book.DetailsUrl) );
                        }
                        catch (HttpRequestException ex)
                        {
                            MessageBox.Show(String.Format("Failed to crawl metadata. Exception: {0}. InnerException: {1}", ex.Message, ex.InnerException), "Failed to crawl metadata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        responseAsString = await response.Content.ReadAsStringAsync();

                        html = new HtmlDocument();
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

                    } else
                    {
                        // we should warn the user and let him choose the right one
                        book.Title = "More than one result found. Decide.";
                    }

                } else
                {
                    book.Title = "Not found at Amazon";
                }
                
            }

            
        }


    }
}
