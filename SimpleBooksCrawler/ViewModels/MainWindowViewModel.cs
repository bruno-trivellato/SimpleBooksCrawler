using SimpleBooksCrawler.Common;
using SimpleBooksCrawler.Models;
using SimpleBooksCrawler.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace SimpleBooksCrawler.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private ObservableCollection<Book> _Books;
        public ObservableCollection<Book> Books
        {
            get { return _Books; }
            set
            {
                SetProperty(ref _Books, value);
            }
        }

        private string _BooksCSVPath;

        public string BooksCSVPath
        {
            get { return _BooksCSVPath; }
            set
            {
                SetProperty(ref _BooksCSVPath, value);
            }
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        private String _LastTraceMessage;
        public String LastTraceMessage
        {
            get { return _LastTraceMessage; }
            set
            {
                // Forcing event rise to fix a bug where not all messages were displayed.
                SetProperty(ref _LastTraceMessage, value, true);
            }
        }


        private Boolean _CanExecuteMetadataCrawling;
        public Boolean CanExecuteMetadataCrawling
        {
            get { return _CanExecuteMetadataCrawling; }
            set
            {
                SetProperty(ref _CanExecuteMetadataCrawling, value);
            }
        }


        private RelayCommand _StopCrawlingCommand;

        public RelayCommand StopCrawlingCommand
        {
            get
            {
                if (_StopCrawlingCommand == null)
                {
                    _StopCrawlingCommand = new RelayCommand(
                      () =>
                      {
                          this.CancellationTokenSource.Cancel();
                          Trace.WriteLine("[Info] Crawl operation canceled.");
                      },
                      () =>
                      {
                          return !this.CanExecuteMetadataCrawling;
                      });
                    this.PropertyChanged += (s, e) => _StopCrawlingCommand.RaiseCanExecuteChanged();
                }
                return _StopCrawlingCommand;
            }
        }



        private RelayCommand _CrawlMetadataCommand;

        public RelayCommand CrawlMetadataCommand
        {
            get
            {
                if (_CrawlMetadataCommand == null)
                {
                    _CrawlMetadataCommand = new RelayCommand(
                      async () =>
                      {
                          this.CancellationTokenSource = new CancellationTokenSource();
                          await Task.Factory.StartNew( () => BooksHandler.Instance.CrawlMetadataAsync(this.CancellationTokenSource.Token) );
                          
                          
                      },
                      () =>
                      {
                          return this.CanExecuteMetadataCrawling;
                      });
                    this.PropertyChanged += (s, e) => _CrawlMetadataCommand.RaiseCanExecuteChanged();
                }
                return _CrawlMetadataCommand;
            }
        }


        private RelayCommand _LoadBooksCSVCommand;

        public RelayCommand LoadBooksCSVCommand
        {
            get
            {
                if (_LoadBooksCSVCommand == null)
                {
                    _LoadBooksCSVCommand = new RelayCommand(
                      () =>
                      {
                          var dialog = new CommonOpenFileDialog();
                          dialog.Filters.Add(
                                new CommonFileDialogFilter("CSV File", ".csv")
                              );

                          var result = dialog.ShowDialog();

                          if (result == CommonFileDialogResult.Ok)
                          {
                              BooksHandler.Instance.LoadBooksFromCSV(dialog.FileName);

                              SettingsHandler.Instance.ChangeCSVBooksPath(dialog.FileName);

                          }
                      },
                      () =>
                      {
                          return this.CanExecuteMetadataCrawling;
                      });
                    this.PropertyChanged += (s, e) => _LoadBooksCSVCommand.RaiseCanExecuteChanged();
                }
                return _LoadBooksCSVCommand;
            }
        }

        public MainWindowViewModel()
        {
            this.Books = BooksHandler.Instance.Books;
            this.CanExecuteMetadataCrawling = BooksHandler.Instance.CanExecuteMetadataCrawling;

            TraceHandler.Instance.ServicePropertyChanged += Instance_ServicePropertyChanged; ;
            SettingsHandler.Instance.ServicePropertyChanged += Instance_ServicePropertyChanged;
            BooksHandler.Instance.ServicePropertyChanged += Instance_ServicePropertyChanged;
        }

        //public int Counter { get; set; }
        private void Instance_ServicePropertyChanged(object sender, BaseService.ServicePropertyChangedEventArgs e)
        {
            
            if (e.ServicePropertyName == nameof(TraceHandler.Instance.TraceListener))
            {
                var message = (String)e.ServicePropertyValue;
                this.LastTraceMessage = message;
                //Console.WriteLine("Hit " + Counter++ + "Msg: " + message);
            }


            if (e.ServicePropertyName == nameof(AppSettings.BooksCSVPath))
            {
                this.BooksCSVPath = (String)e.ServicePropertyValue;
            }

            if(e.ServicePropertyName == nameof(BooksHandler.Instance.CanExecuteMetadataCrawling))
            {
                this.CanExecuteMetadataCrawling = BooksHandler.Instance.CanExecuteMetadataCrawling;
            }
            
        }

    }
}
