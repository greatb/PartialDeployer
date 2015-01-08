using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialDeployer
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the profile name");
                Environment.Exit(1);
            }*/

            ftp ftp1 = new ftp();

            configMan confiMan = loadConfig(args);
            if (confiMan == null)
            {
                Environment.Exit(1);
            }


            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(confiMan.ReleaseNameTemplate);

            Deploy _deploy = new Deploy(confiMan);


            folderMan fman = new folderMan();

            /*
            int iCounter = 0;
            while (!CheckRemoteAccess(ftp1, cr) && iCounter < 5)
            {
                iCounter++;
            }

            if (!CheckRemoteAccess(ftp1, cr))
            {
                Console.WriteLine("Unable to connect to FTP server");
                Environment.Exit(1);
            }
             * */


            _deploy.CleanUpDeployFolderAndReleaseFolder();
            _deploy.CopyNewAndChangedFiles();
            IEnumerable<DirEntry> filesToDeploy = fman.DirGetFolderContents(confiMan.DIR_Deploy);

            foreach (DirEntry f in filesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                string toPath = confiMan.FTP_Folder + f.EntryPath;
                toPath = confiMan.FTP_Server + toPath.Replace("\\", "/").Replace("//", "/");
                string toFile = toPath + f.EntryName;
                string fromFile = confiMan.DIR_Deploy + f.EntryPath + f.EntryName;
                if (ftp1.FTPUpload(fromFile, toFile) == false)
                {
                    ftp1.FTPMakeFolder(toPath);
                    ftp1.FTPUpload(fromFile, toFile);
                }
            }
            _deploy.CopyAllDeployToProduction();
        }

        private configMan loadConfig(string[] args)
        {
            configMan confiMan;
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

                confiMan = new configMan(appName);
                Console.WriteLine(confiMan.GetString("sdfsfsfdsf"));
                return confiMan;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
                return null;
            }
        }

        private static bool CheckRemoteAccess(ftp ftp1, configMan confiMan)
        {
            try
            {
                Console.WriteLine(confiMan.FTP_Server);
                ftp1.FTPGetFolderContents(confiMan.FTP_Server, "/", false);
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
