using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.FtpClient;
using Hatfield.AquariusDataImporter.Core.Models.Goes;
using log4net;

namespace Hatfield.AquariusDataImporter.Core.Helpers
{
    public static class GoesDataHelper
    {
        public static string ExtractDataSetName(string rawDataLine)
        {
            var startIndex = rawDataLine.IndexOf('_');
            var endIndex = rawDataLine.LastIndexOf("_");

            var dataSetName = rawDataLine.Substring(startIndex + 1, endIndex - startIndex - 1);
            return dataSetName;
        }

        public static GoesValueData ExtractDataValue(string rawDataLine)
        {
            var cells = rawDataLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            Console.WriteLine(cells[0]);
            var dateTime = DateTime.ParseExact(cells[0], "yyyyMMddHHmmss", null);
            var value = double.Parse(cells[1]);

            return new GoesValueData
            {
                DateTime = dateTime,
                Value = value
            };
        }

        public static FtpClient ConfigureFTPClient()
        {
            var client = new FtpClient();
            client.Host = Constants.GOESDataFTPURL;
            client.Credentials = new System.Net.NetworkCredential(Constants.GOESDataUserName, Constants.GOESDataPassword);
            client.ConnectTimeout = Int32.MaxValue;//Default: 15000

            return client;
        }

        public static IEnumerable<FtpListItem> GetAllFilesFromFTP()
        {
            IEnumerable<FtpListItem> ftpListItem = null;
            using (var ftpClient = ConfigureFTPClient())
            {
                ftpListItem = ftpClient.GetListing(ftpClient.GetWorkingDirectory(), FtpListOption.Modify | FtpListOption.Size).Where(x => x.Type == FtpFileSystemObjectType.File);

                ftpClient.Disconnect();
            }

            return ftpListItem;
        }

        public static Dictionary<string, IEnumerable<GoesValueData>> DownloadAndTransformDataForSingleFile(FtpListItem fileListItem, ILog log)
        {
            log.Info("Starting to download and process file '" + fileListItem.FullName + "'");
            MemoryStream ms = new MemoryStream();
            using (var ftpClient = GoesDataHelper.ConfigureFTPClient())
            {
                using (var stream = ftpClient.OpenRead(fileListItem.FullName, FtpDataType.Binary))
                {
                    try
                    {
                        // istream.Position is incremented accordingly to the reads you perform
                        // istream.Length == file size if the server supports getting the file size
                        // also note that file size for the same file can vary between ASCII and Binary
                        // modes and some servers won't even give a file size for ASCII files! It is
                        // recommended that you stick with Binary and worry about character encodings
                        // on your end of the connection.

                        byte[] buf = new byte[8192];
                        int read = 0;

                        while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                        {
                            ms.Write(buf, 0, read);

                            log.DebugFormat("{0}/{1} {2:p} {3}    ", stream.Position, stream.Length, ((double)stream.Position / (double)stream.Length), fileListItem.FullName);
                        }
                    }
                    finally
                    {
                        stream.Close();
                    }
                }//end of using stream

                ftpClient.Disconnect();
            }//end of ftpClient

            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);
            string fileContents = sr.ReadToEnd();

            string[] lines = fileContents.Split(new string[] { "\n", "\r", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            log.Info("File '" + fileListItem.FullName + "' downloaded. Length: " + fileContents.Length.ToString() + "; # lines: " + lines.Length.ToString());

            var transformedValue = TransformRawData(lines);

            return transformedValue;
        }

        private static Dictionary<string, IEnumerable<GoesValueData>> TransformRawData(string[] lines)
        {
            var data = new Dictionary<string, IEnumerable<GoesValueData>>();

            var tempDataName = string.Empty;
            var tempDataValue = new List<GoesValueData>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    if (!string.IsNullOrEmpty(tempDataName) && tempDataValue.Any())
                    {
                        data.Add(tempDataName, tempDataValue);
                        tempDataName = string.Empty;
                        tempDataValue = new List<GoesValueData>();
                    }
                    else
                    {
                        tempDataName = string.Empty;
                        tempDataValue = new List<GoesValueData>();
                    }

                    tempDataName = GoesDataHelper.ExtractDataSetName(line);
                }
                else
                {
                    var extractedValue = GoesDataHelper.ExtractDataValue(line);
                    tempDataValue.Add(extractedValue);
                }
            }

            if (!string.IsNullOrEmpty(tempDataName) && tempDataValue.Any() && !data.ContainsKey(tempDataName))
            {
                data.Add(tempDataName, tempDataValue);
                tempDataName = string.Empty;
                tempDataValue = new List<GoesValueData>();
            }

            return data;
        }

        public static void MoveProcessedFile(bool moveToSuccess, ILog log)
        {
            string destDir = Constants.GOESProcessedWithSuccessFolder;
            if (!moveToSuccess)
                destDir = Constants.GOESProcessedWithProblemFolder;

            var allFiles = GetAllFilesFromFTP();
            using (FtpClient client = ConfigureFTPClient())
            {
                foreach (var file in allFiles)
                {
                    if (!client.DirectoryExists(destDir))
                    {
                        log.Warn("Creating FTP site directory that does not exist " + destDir);
                        client.CreateDirectory(destDir, true);
                    }
                    if (!destDir.EndsWith("/"))
                        destDir += "/";

                    string destPath = destDir + Path.GetFileName(file.FullName);
                    log.Info("Moving '" + file.FullName + "' to '" + destPath + "'");
                    client.Rename(file.FullName, destPath);
                    if (!client.FileExists(file.FullName) && client.FileExists(destPath))
                        log.Info("Successfully moved file");
                    else
                        log.Error("Could not move '" + file.FullName + "' to '" + destPath + "' on the FTP site.");
                }

                client.Disconnect();
            } // using
        }
    }
}