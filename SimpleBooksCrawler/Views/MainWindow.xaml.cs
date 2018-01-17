using SimpleBooksCrawler.Common;
using SimpleBooksCrawler.Services;
using SimpleBooksCrawler.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleBooksCrawler.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();


            this.ViewModel = (MainWindowViewModel)this.DataContext;
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = SettingsHandler.Instance;

            Trace.Listeners.Add(TraceHandler.Instance.TraceListener);

            settings.Init();

            if(settings.AppSettings.BooksCSVPath != null)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                        new Action(() => {
                            //BooksHandler.Instance.LoadBooksFromCSV(settings.AppSettings.BooksCSVPath);

                            BooksHandler.Instance.LoadSavedBooks();
                            HttpRequestHelper.Instance.LoadMobileUserAgentStrings();
                        }));
                
            }
            
            this.TraceOutputTextBox.ScrollToEnd();
        }

        
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ViewModel.LastTraceMessage))
            {
                this.TraceOutputTextBox.AppendText(this.ViewModel.LastTraceMessage);
            }
        }

        static readonly Dictionary<TextBox, Capture> _associations = new Dictionary<TextBox, Capture>();

        public static bool GetScrollOnTextChanged(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(ScrollOnTextChangedProperty);
        }

        public static void SetScrollOnTextChanged(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(ScrollOnTextChangedProperty, value);
        }

        public static readonly DependencyProperty ScrollOnTextChangedProperty =
            DependencyProperty.RegisterAttached("ScrollOnTextChanged", typeof(bool), typeof(MainWindow), new UIPropertyMetadata(false, OnScrollOnTextChanged));

        static void OnScrollOnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var textBox = dependencyObject as TextBox;
            if (textBox == null)
            {
                return;
            }
            bool oldValue = (bool)e.OldValue, newValue = (bool)e.NewValue;
            if (newValue == oldValue)
            {
                return;
            }
            if (newValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.Unloaded += TextBoxUnloaded;
            }
            else
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.Unloaded -= TextBoxUnloaded;
                if (_associations.ContainsKey(textBox))
                {
                    _associations[textBox].Dispose();
                }
            }
        }

        static void TextBoxUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            _associations[textBox].Dispose();
            textBox.Unloaded -= TextBoxUnloaded;
        }

        static void TextBoxLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var textBox = (TextBox)sender;
            textBox.Loaded -= TextBoxLoaded;
            _associations[textBox] = new Capture(textBox);
        }

        class Capture : IDisposable
        {
            private TextBox TextBox { get; set; }

            public Capture(TextBox textBox)
            {
                TextBox = textBox;
                TextBox.TextChanged += OnTextBoxOnTextChanged;
            }

            private void OnTextBoxOnTextChanged(object sender, TextChangedEventArgs args)
            {
                TextBox.ScrollToEnd();
            }

            public void Dispose()
            {
                TextBox.TextChanged -= OnTextBoxOnTextChanged;
            }
        }
    }
}
