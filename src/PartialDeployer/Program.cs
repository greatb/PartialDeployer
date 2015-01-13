using System;
using System.Collections.Generic;
using System.Linq;

using log4net;
using log4net.Config;

namespace PartialDeployer
{
    class Program
    {
        private static readonly ILog log = LogManager.GetLogger("PDip");

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            //BasicConfigurator.Configure();
            log.Debug("Test");

            configMan _config = loadConfig(args);

            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(_config.ReleaseNameTemplate);

            folderMan fman = new folderMan();
            ftp ftp1 = new ftp(_config);
            Deploy _deploy = new Deploy(_config);
            
            CheckConnection(ftp1, _config);

            _deploy.CleanUpDeployFolderAndReleaseFolder();
            _deploy.CopyNewAndChangedFiles();
            IEnumerable<DirEntry> filesToDeploy = fman.DirGetFolderContents(_config.DIR_Deploy);

            foreach (DirEntry f in filesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                string toPath = _config.FTP_Folder + f.EntryPath;
                toPath = _config.FTP_Server + toPath.Replace("\\", "/").Replace("//", "/");
                string toFile = toPath + f.EntryName;
                string fromFile = _config.DIR_Deploy + f.EntryPath + f.EntryName;
                if (ftp1.FTPUpload(fromFile, toFile) == false)
                {
                    ftp1.FTPMakeFolder(toPath);
                    ftp1.FTPUpload(fromFile, toFile);
                }
            }
            _deploy.CopyAllDeployToProduction();
        }

        private static void CheckConnection(ftp ftp1, configMan _config)
        {
            log.Debug("CheckConnection");

            int iCounter = 0;
            while (!CheckRemoteAccess(ftp1, _config) && iCounter < 5)
            {
                iCounter++;
            }

            if (!CheckRemoteAccess(ftp1, _config))
            {
                Console.WriteLine("Unable to connect to FTP server");
                Environment.Exit(1);
            }
        }

        private static configMan loadConfig(string[] args)
        {
            string appName;
            try
            {
                if (args.Length == 0)
                {
                    appName = "App";
                }
                else
                {
                    appName = args[0];
                }

                appName = string.Format("{0}.config", appName);

                configMan confiMan = new configMan(appName);
                Console.WriteLine(confiMan.GetString("sdfsfsfdsf"));
                return confiMan;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return null;
            }
        }

        private static bool CheckRemoteAccess(ftp ftp1, configMan _config)
        {
            try
            {
                Console.WriteLine(_config.FTP_Server);
                ftp1.FTPGetFolderContents(_config.FTP_Server, "/", false);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}
