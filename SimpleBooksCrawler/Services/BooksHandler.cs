﻿using SimpleBooksCrawler.Models;
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
using CsvHelper;
using System.IO;

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
            this.CanExecuteMetadataCrawling = true;

            this.Books.CollectionChanged += Books_CollectionChanged;
        }


        private Boolean _CanExecuteMetadataCrawling;
        public Boolean CanExecuteMetadataCrawling
        {
            get { return _CanExecuteMetadataCrawling; }
            set
            {
                SetServiceProperty(ref _CanExecuteMetadataCrawling, value);
            }
        }


        private void Books_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var hehe = 1;
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
                    book.BookState = BookState.WaitingToBeCrawled;

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

        public void SaveBooks()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            var fileFullPath = String.Format("{0}{1}", currentDir, "saved.csv");
            using (StreamWriter outFile = new StreamWriter(fileFullPath))
            {
                var csv = new CsvWriter(outFile);
                csv.WriteRecords(this.Books);
            }
        }

        public async void CrawlMetadataAsync()
        {
            this.CanExecuteMetadataCrawling = false;


            foreach (var book in this.Books)
            {
                book.BookState = BookState.OnCrawling;

                
                AmazonCrawler amazonCrawler = new AmazonCrawler();
                await amazonCrawler.CrawlBookAsync(book);
            }

            this.CanExecuteMetadataCrawling = true;

        }


    }
}
