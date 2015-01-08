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
            configMan cr = new configMan();
            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(cr.ReleaseNameTemplate);


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


            //IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(cr.DIR_Source);
            //IEnumerable<DirEntry> fldProudContent = fman.DirGetFolderContents(cr.DIR_Product);

            Deploy.CleanUpDeployFolderAndReleaseFolder();
            Deploy.CopyNewAndChangedFiles();
            IEnumerable<DirEntry> filesToDeploy = fman.DirGetFolderContents(cr.DIR_Deploy);

            /*
            IEnumerable<DirEntry> foldersToDeploy = fman.DirGetFolder(cr.DIR_Deploy);
            foreach (DirEntry f in foldersToDeploy)
            {
                ftp1.FTPMakeFolder(cr.FTP_Server + cr.FTP_Folder +  f.EntryPath.Replace("\\","/"));
            }
             */

            foreach (DirEntry f in filesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                string toPath = cr.FTP_Folder + f.EntryPath;
                toPath = cr.FTP_Server + toPath.Replace("\\", "/").Replace("//", "/");
                string toFile = toPath + f.EntryName;
                string fromFile = cr.DIR_Deploy + f.EntryPath + f.EntryName;
                if (ftp1.FTPUpload(fromFile, toFile) == false)
                {
                    ftp1.FTPMakeFolder(toPath);
                    ftp1.FTPUpload(fromFile, toFile);
                }
            }
            Deploy.CopyAllDeployToProduction();
        }

        private static bool CheckRemoteAccess(ftp ftp1, configMan cr)
        {
            try
            {
                Console.WriteLine(cr.FTP_Server);
                ftp1.FTPGetFolderContents(cr.FTP_Server, "/", false);
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
