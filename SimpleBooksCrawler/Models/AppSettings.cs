using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using SimpleBooksCrawler.ViewModels;
using System.IO;

namespace SimpleBooksCrawler.Models
{
    /// <summary>
    /// This class holds all settings for this application.
    /// </summary>
    [DataContract]
    public class AppSettings
    {
        [DataMember]
        public String BooksCSVPath { get; set; }


        /// <summary>
        /// Full path where the Settings File resides.
        /// </summary>
        public String SettingsFileFullPath { get; set; }

        public AppSettings Clone()
        {
            var newAppSettings = new AppSettings();
            newAppSettings.BooksCSVPath = this.BooksCSVPath;

            return newAppSettings;
        }

        [DataMember]
        public Boolean ShowDebugInformation { get; set; }
        

    }

}
