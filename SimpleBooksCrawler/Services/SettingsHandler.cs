using SimpleBooksCrawler.Common;
using SimpleBooksCrawler.Models;
using SimpleBooksCrawler.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SimpleBooksCrawler.Services
{
    /// <summary>
    /// This class is responsible for reading and writing settings.
    /// </summary>
    public class SettingsHandler : BaseService
    {

        private readonly string SettingsFileName = "settings.json";

        private static SettingsHandler _instance;

        public static SettingsHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsHandler();
                }

                return _instance;
            }
            set { _instance = value; }
        }
        


        private AppSettings _appSettings;

        public AppSettings AppSettings
        {
            get { return _appSettings; }
            set { _appSettings = value; }
        }


        private Boolean _ShowDebugInformation;
        public Boolean ShowDebugInformation
        {
            get { return _ShowDebugInformation; }
            set
            {
                SetServiceProperty(ref _ShowDebugInformation, value);
            }
        }


        private String _BooksCSVPath;
        public String BooksCSVPath
        {
            get { return _BooksCSVPath; }
            set
            {
                SetServiceProperty(ref _BooksCSVPath, value);
            }
        }



        private SettingsHandler()
        {
            this.AppSettings = new AppSettings();
            this.ServicePropertyChanged += SettingsHandler_ServicePropertyChanged1;
        }

        private void SettingsHandler_ServicePropertyChanged1(object sender, ServicePropertyChangedEventArgs e)
        {
            if (e.ServicePropertyName == nameof(AppSettings.BooksCSVPath))
            {
                this.AppSettings.BooksCSVPath = this.BooksCSVPath;
            }

            if(e.ServicePropertyName == nameof(AppSettings.ShowDebugInformation))
            {
                this.AppSettings.ShowDebugInformation = this.ShowDebugInformation;
            }


            // Auto save. Every time a property is changed, save settings.
            SaveSettings(this.AppSettings);
        }
        



        /// <summary>
        /// Initializes settings.
        /// Loads the settings file and does validation.
        /// </summary>
        public void Init()
        {
            
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            this.AppSettings = new AppSettings();

            this.AppSettings.SettingsFileFullPath = String.Format("{0}{1}", currentDir, SettingsFileName);
            LoadSettingsFile(this.AppSettings.SettingsFileFullPath);
            

        }

       

        /// <summary>
        /// Saves settings to the settings.json file.
        /// </summary>
        public void SaveSettings(AppSettings appSettings)
        {
            try
            {
                Serializer.SerializeInJson(appSettings, appSettings.SettingsFileFullPath);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Loads the serialized settings file.
        /// </summary>
        /// <returns>True if file was loaded successfully. False otherwise.</returns>
        private bool LoadSettingsFile(string settingsFullPath)
        {
            try
            {
                var file = Serializer.DeserializeInJson<AppSettings>(settingsFullPath);
                this.ShowDebugInformation = file.ShowDebugInformation;

                ChangeCSVBooksPath(file.BooksCSVPath);

                Trace.WriteLine("Settings file loaded successfully.");
            }
            catch (SerializationException)
            {
                // bad format in file. It is not T.
                Trace.WriteLine("[Warning] Settings file was not loaded successfully.");
                return false;
            }
            catch (FileNotFoundException)
            {
                Trace.WriteLine("[Warning] Settings file was not loaded successfully.");
                return false;
                // No settings file.
                

            } finally
            {
                
            }

            return true;
        }
        

        public Boolean ChangeCSVBooksPath(string newCSVBooksPath)
        {
            this.BooksCSVPath = newCSVBooksPath;

            Trace.WriteLine("CSV Books path changed successfully.");

            return true;
        }
        



    }
}
