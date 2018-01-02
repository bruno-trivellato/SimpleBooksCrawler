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

        private String _LastTraceMessage;
        public String LastTraceMessage
        {
            get { return _LastTraceMessage; }
            set
            {
                SetProperty(ref _LastTraceMessage, value);
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


        private RelayCommand _CrawlMetadataCommand;

        public RelayCommand CrawlMetadataCommand
        {
            get
            {
                if (_CrawlMetadataCommand == null)
                {
                    _CrawlMetadataCommand = new RelayCommand(
                      () =>
                      {

                          BooksHandler.Instance.CrawlMetadataAsync();
                          
                          
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

        private void Instance_ServicePropertyChanged(object sender, BaseService.ServicePropertyChangedEventArgs e)
        {
            
            if (e.ServicePropertyName == nameof(TraceHandler.Instance.TraceListener))
            {
                var message = (String)e.ServicePropertyValue;
                this.LastTraceMessage = message;
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
