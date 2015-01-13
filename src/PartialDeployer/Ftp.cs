using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Security.Cryptography;
using System.IO;

namespace PartialDeployer
{
    using System.Text.RegularExpressions;
    using log4net;

    public class ftp
    {
        private static readonly ILog log = LogManager.GetLogger("Ftp");

        private string ftp_server;
        private string ftp_username;
        private string ftp_password;
        private string ftp_folder;

        //http://stackoverflow.com/questions/1013486/parsing-ftpwebrequests-listdirectorydetails-line
        //Regex regex = new Regex(@"^([d-])([rwxt-]{3}){3}\s+\d{1,}\s+.*?(\d{1,})\s+(\w+\s+\d{1,2}\s+(?:\d{4})?)(\d{1,2}:\d{2})?\s+(.+?)\s?$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static Regex FtpListDirectoryDetailsRegex = new Regex(@".*(?<month>(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec))\s*(?<day>[0-9]*)\s*(?<yearTime>([0-9]|:)*)\s*(?<fileName>.*)", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        public static string fileNamesWithoutDot = "|error_log|";


        public ftp(configMan _configMan)
        {
            ftp_username = _configMan.FTP_UserName;
            ftp_password = _configMan.FTP_PassWord;
            ftp_server = _configMan.FTP_Server;
            ftp_folder = _configMan.FTP_Folder;
        }

        #region Privates
        private NetworkCredential getNewNetworkCredential()
        {
            return new NetworkCredential(ftp_username, ftp_password);
        }

        private FtpWebRequest getRequestObject(string fullpath)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fullpath);
            request.Credentials = getNewNetworkCredential();
            request.KeepAlive = true;
            request.UsePassive = true;
            request.Timeout = 300000;
            request.ReadWriteTimeout = 1200000;
            request.ConnectionGroupName = getSecureGroupName();
            return request;
        }

        private string getSecureGroupName()
        {
            SHA1Managed Sha1 = new SHA1Managed();
            Byte[] updHash = Sha1.ComputeHash(Encoding.UTF8.GetBytes(ftp_username + ftp_password + ftp_server));
            return Encoding.Default.GetString(updHash);
        }
        #endregion

        public void FTPdownload(string fileToDownload, string savePath, string saveAs)
        {
            log.Debug("FTPdownload");

            try
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
            }
            catch (Exception ex)
            {
                log.InfoFormat("Error : {0} - {1}", savePath, ex.Message);
            }


            try
            {

                FtpWebRequest requestFileDownload = getRequestObject(fileToDownload);
                requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();


                Stream responseStream = responseFileDownload.GetResponseStream();
                FileStream writeStream = new FileStream(saveAs, FileMode.Create);

                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);

                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }

                responseStream.Close();
                writeStream.Close();

                requestFileDownload = null;
                responseFileDownload = null;


            }
            catch (Exception ex)
            {
                log.InfoFormat("Error Server connection : {0} - {1}", savePath, ex.Message);
            }
        }

        //http://msdn.microsoft.com/en-us/library/ms229715%28v=vs.110%29.aspx

        public bool FTPMakeFolder(string folderToMake)
        {
            log.Debug("FTPMakeFolder");

            folderToMake = folderToMake.Substring(0, folderToMake.Length - 1);

            string[] pathToMake = folderToMake.Substring(ftp_server.Length + ftp_folder.Length, folderToMake.Length - ftp_server.Length - ftp_folder.Length).Split('/');

            for (int i = 1; i <= pathToMake.Length - 1; i++)
            {
                pathToMake[i] = string.Format("{0}/{1}", pathToMake[i - 1], pathToMake[i]);
            }

            foreach (string path in pathToMake)
            {
                string tryPath = String.Format("{0}{1}{2}", ftp_server, ftp_folder, path);
                FtpWebRequest request = getRequestObject(tryPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                try
                {
                    WebResponse response = request.GetResponse();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat(ex.Message);
                }
            }

            return true;
        }

        public bool FTPUpload(string fromFile, string fileToUpload)
        {
            log.Debug("FTPUpload");

            if (!File.Exists(fromFile))
            {
                log.InfoFormat("FromFile not exists : {0}", fromFile);
                return false;
            }

            FtpWebRequest requestFTPUploader = getRequestObject(fileToUpload);
            log.InfoFormat("Upload file : {0}", fileToUpload);
            requestFTPUploader.Method = WebRequestMethods.Ftp.UploadFile;

            FileInfo fileInfo = new FileInfo(fromFile);
            FileStream fileStream = fileInfo.OpenRead();

            int bufferLength = 2048;
            byte[] buffer = new byte[bufferLength];

            try
            {
                Stream uploadStream = requestFTPUploader.GetRequestStream();
                int contentLength = fileStream.Read(buffer, 0, bufferLength);

                while (contentLength != 0)
                {
                    uploadStream.Write(buffer, 0, contentLength);
                    contentLength = fileStream.Read(buffer, 0, bufferLength);
                }
                uploadStream.Close();
                fileStream.Close();

            }
            catch (Exception e)
            {
                log.ErrorFormat(e.Message);
                return false;
            }
            finally
            {
                requestFTPUploader = null;
            }

            return true;
        }


        private DirEntry getFtpEntryFromDataLine(string dataline, string sFolder)
        {
            log.Debug("getFtpEntryFromDataLine");

            Match match = FtpListDirectoryDetailsRegex.Match(dataline);
            DirEntry ftpET = new DirEntry() { EntryName = match.Groups["fileName"].Value, EntryPath = sFolder, EntryType = FtpEntryType.Unknown };
            log.Info(dataline);
            if (dataline.EndsWith(" .") || dataline.EndsWith(" .."))
            {
                ftpET.EntryType = FtpEntryType.Unknown;
            }
            else if (dataline.StartsWith("d"))
            {
                ftpET.EntryType = FtpEntryType.Folder;
                ftpET.EntryName = ftpET.EntryName + "/";
            }
            else
            {
                ftpET.EntryType = FtpEntryType.File;
            }
            return ftpET;
        }

        private List<DirEntry> getDirectoryEntries(string sServer, string sFolder, bool details = true)
        {
            log.Debug("getDirectoryEntries");

            List<DirEntry> DirEntries = new List<DirEntry>();
            FtpWebRequest request = getRequestObject(sServer + sFolder);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            FtpWebResponse responseDetails = (FtpWebResponse)request.GetResponse();
            Stream responseDetailsStream = responseDetails.GetResponseStream();
            StreamReader readerDetails = new StreamReader(responseDetailsStream);

            string line = readerDetails.ReadLine();
            while (line != null)
            {
                DirEntries.Add(new DirEntry() { EntryName = line, EntryPath = sFolder, EntryType = FtpEntryType.Unknown });
                line = readerDetails.ReadLine();
            }
            readerDetails.Close();
            responseDetails.Close();

            FtpWebRequest request1 = getRequestObject(sServer + sFolder);
            request1.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            FtpWebResponse responseDetails1 = (FtpWebResponse)request1.GetResponse();
            Stream responseDetailsStream1 = responseDetails1.GetResponseStream();
            StreamReader readerDetails1 = new StreamReader(responseDetailsStream1);

            string line1;
            foreach (DirEntry f in DirEntries)
            {
                line1 = readerDetails1.ReadLine();
                if (line1.StartsWith("-"))
                {
                    f.EntryType = FtpEntryType.File;
                }
                else
                {
                    if (f.EntryName != "." && f.EntryName != "..")
                    {
                        f.EntryType = FtpEntryType.Folder;
                        f.EntryName = f.EntryName + "/";
                    }
                }
            }
            readerDetails1.Close();
            responseDetails1.Close();

            return DirEntries;
        }

        private List<DirEntry> getDirectoryEntries2(string sServer, string sFolder, bool details = true)
        {
            log.Debug("getDirectoryEntries2");

            List<DirEntry> DirEntries = new List<DirEntry>();
            FtpWebRequest request = getRequestObject(sServer + sFolder);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            FtpWebResponse responseDetails = (FtpWebResponse)request.GetResponse();
            Stream responseDetailsStream = responseDetails.GetResponseStream();
            StreamReader readerDetails = new StreamReader(responseDetailsStream);

            string line = readerDetails.ReadLine();
            while (line != null)
            {
                if (line.Length > 2)
                {
                    DirEntries.Add(getFtpEntryFromDataLine(line, sFolder));
                }
                line = readerDetails.ReadLine();
            }

            readerDetails.Close();
            responseDetails.Close();

            return DirEntries;
        }

        // http://msdn.microsoft.com/en-us/library/ms229716%28v=vs.110%29.aspx
        public List<DirEntry> FTPGetFolderContents(string sServer, string sFolder, bool regressive = true)
        {
            log.Debug("FTPGetFolderContents"); 
            log.InfoFormat("Reading Folder : {0}", sFolder);

            List<DirEntry> DirEntries = new List<DirEntry>();

            DirEntries = getDirectoryEntries2(sServer, sFolder);

            if (regressive)
            {
                List<DirEntry> ftpFolders = DirEntries.Where(x => (x.EntryType == FtpEntryType.Folder)).ToList();

                foreach (DirEntry f in ftpFolders)
                {
                    DirEntries.AddRange(FTPGetFolderContents(sServer, sFolder + f.EntryName));
                }
            }
            return DirEntries;
        }
    }
}
