using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Common
{
    public class HttpRequestHelper
    {
        public String DefaultMobileUserAgentString = "Mozilla / 5.0(Linux; U; Android 4.0.3; ko-kr; LG-L160L Build/IML74K) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";

        private static HttpRequestHelper _instance;

        public static HttpRequestHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpRequestHelper();
                }

                return _instance;
            }
            set { _instance = value; }
        }

        public List<String> MobileUserAgentStrings { get; set; }

        private HttpRequestHelper()
        {

        }

        public String GetRandomMobileUserAgentString()
        {
            Int32 randomValue = new Random().Next(0, MobileUserAgentStrings.Count);

            return MobileUserAgentStrings[randomValue];
        }

        public void LoadMobileUserAgentStrings()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string mobileUserAgentFilePath = String.Format("{0}mobile user agent strings.txt", currentDir);
            List<String> mobileUserAgentStrings = new List<string>();

            try
            {
                mobileUserAgentStrings =  File.ReadAllLines(mobileUserAgentFilePath).ToList();
                

                Trace.WriteLine("[Info] Mobile user agent strings loaded successfully.");
            }
            catch (IOException ex)
            {
                Trace.WriteLine(String.Format("[Error] Failed to load mobile user agent strings. Setting a default one. Message: {0}", ex.Message));

                mobileUserAgentStrings.Add(this.DefaultMobileUserAgentString);

            }

            this.MobileUserAgentStrings = mobileUserAgentStrings;
        }

    }
}
