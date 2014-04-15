using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using HtmlAgilityPack;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class SutronDataDownloadHelper
    {
        private static readonly ILog log = LogManager.GetLogger("Application");

        public static IEnumerable<SutronDataFile> FetchDownloadableDataFileList(string rootUrl)
        {
            if (!rootUrl.EndsWith("/"))
                rootUrl += "/";

            string rootHtml = string.Empty;
            // -- download the file list
            using (LongTimeoutWebClient webClient = new LongTimeoutWebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                log.InfoFormat("Downloading file list from '{0}' ...", rootUrl);
                rootHtml = webClient.DownloadString(rootUrl);
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
