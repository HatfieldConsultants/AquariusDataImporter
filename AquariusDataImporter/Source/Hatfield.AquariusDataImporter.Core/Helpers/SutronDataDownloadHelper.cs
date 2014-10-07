using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class SutronDataDownloadHelper
    {
        private static readonly ILog log = LogManager.GetLogger("Application");
        private static string SutronUserName = System.Configuration.ConfigurationManager.AppSettings["SutronUserName"];
        private static string SutronPassword = System.Configuration.ConfigurationManager.AppSettings["SutronPassword"];
            
        public static IEnumerable<SutronDataFile> FetchDownloadableDataFileList(string rootUrl)
        {
            if (!rootUrl.EndsWith("/"))
                rootUrl += "/";

            string rootHtml = string.Empty;
            // -- download the file list
            using (LongTimeoutWebClient webClient = new LongTimeoutWebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Credentials = new NetworkCredential(SutronUserName, SutronPassword);
                log.InfoFormat("Downloading file list from '{0}' ...", rootUrl);
                rootHtml = webClient.DownloadString(rootUrl);
                //var filePath = Path.Combine(@"C:\Users\zlu\Documents\Code\WRD\AquariusDataImporter\Source\Hatfield.AquariusDataImporter\Html", DateTime.Now.ToString("yyyy-MMM-dd-mm-ss") + DateTime.Now.Ticks +".html");
                //File.WriteAllText(filePath, rootHtml);
                
            }

            // -- parse the file list
            log.InfoFormat("Parsing HTML for file list... ({0} characters)", rootHtml.Length);
            var dataFiles = ParseDownloadableDataFilesFromRootHtml(rootHtml);
            log.InfoFormat("{0} data files found.", dataFiles.Count());

            return dataFiles;
        }

        public static string DownloadSingleDataFile(string rootUrl, SutronDataFile dataFile)
        {
            try
            {
                using (LongTimeoutWebClient webClient = new LongTimeoutWebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Credentials = new NetworkCredential(SutronUserName, SutronPassword);
                    string fileUrl = rootUrl + dataFile.FileName;
                    log.InfoFormat("Downloading file list from '{0}' ...", fileUrl);
                    return webClient.DownloadString(fileUrl);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static IEnumerable<SutronDataFile> ParseDownloadableDataFilesFromRootHtml(string rootHtml)
        {
            List<SutronDataFile> ret = new List<SutronDataFile>();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(rootHtml);
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables == null || tables.Count == 0)
            {
                log.Error("No tables found in downloaded HTML. Nothing to parse.");
                return ret.ToArray();
            }

            var table = tables[0];
            var rows = table.SelectNodes("tr");
            if (rows == null || rows.Count == 0)
            {
                log.Error("No rows found in table. Nothing to parse.");
                return ret.ToArray();
            }

            foreach (HtmlNode row in rows)
            {
                var rowCells = row.SelectNodes("td");
                if (rowCells != null && rowCells.Count >= 4)
                {
                    // 0: icon
                    // 1: fileName
                    // 2: date
                    // 3: size
                    // 4: description
                    string fileName = rowCells[1].InnerText.Trim();
                    string createdDate = rowCells[2].InnerText.Trim();
                    string size = rowCells[3].InnerText.Trim();

                    if (size.Contains("K"))
                    {
                        var tempValue = double.MinValue;
                        var sizeString = size.Substring(0, size.LastIndexOf("K"));
                        tempValue = double.Parse(sizeString);
                        size = ((int)(tempValue * 1024)).ToString();

                    }

                    long sz = long.MinValue;
                    if (long.TryParse(size, out sz) && fileName.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SutronDataFile file = new SutronDataFile()
                        {
                            FileName = fileName,
                            LastModified = DateTime.Parse(createdDate),
                            Size = sz
                        };
                        ret.Add(file);
                    }
                }
            }

            return ret;
        }

    }

    public class SutronDataFile
    {
        public string FileName { get; set; }

        public DateTime LastModified { get; set; }

        public long Size { get; set; }
    }
}
