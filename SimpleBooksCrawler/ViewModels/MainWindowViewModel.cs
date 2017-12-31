﻿using SimpleBooksCrawler.Common;
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
                          return true;
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

                              //if (SettingsHandler.Instance.IsElfbotPath(dialog.FileName))
                              //{
                              //    SettingsHandler.Instance.ChangeElfbotPath(dialog.FileName);

                              //}
                              //else
                              //{
                              //    MessageBox.Show("Folder doesn't contain Elfbot's executable (loader.exe).", "Error",
                              //    MessageBoxButton.OK, MessageBoxImage.Error);
                              //}
                          }
                      },
                      () =>
                      {
                          return true;
                      });
                    this.PropertyChanged += (s, e) => _LoadBooksCSVCommand.RaiseCanExecuteChanged();
                }
                return _LoadBooksCSVCommand;
            }
        }

        public MainWindowViewModel()
        {
            this.Books = BooksHandler.Instance.Books;
        }

    }
}