using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PartialDeployer
{
    using log4net;

    public class Deploy
    {
        private static readonly ILog log = LogManager.GetLogger("Deploy");

        private configMan _config;

        public Deploy(configMan configMan)
        {
            _config = configMan;
        }

        public void CheckAndCreateNewFolders(string[] Srcfolders)
        {
            log.Debug("CheckAndCreateNewFolders");

            foreach (string folder in Srcfolders)
            {
                string SrcFolderPath = _config.DIR_Source;
                string PrdFolderPath = folder.Replace(SrcFolderPath, _config.DIR_Product);
                string DepFolderPath = folder.Replace(SrcFolderPath, _config.DIR_Deploy);

                if (File.Exists(PrdFolderPath))
                {
                    if (!(File.Exists(DepFolderPath)))
                    {
                        Directory.CreateDirectory(DepFolderPath);
                    }
                }
            }
        }

        public void CleanUpDeployFolderAndReleaseFolder()
        {
            log.Debug("CleanUpDeployFolderAndReleaseFolder");

            if (Directory.Exists(_config.DIR_Deploy))
            {
                Directory.Delete(_config.DIR_Deploy, true);
            }
            Directory.CreateDirectory(_config.DIR_Deploy);
        }

        public void CopyAllDeployToProduction()
        {
            log.Debug("CopyAllDeployToProduction");

            folderMan fman = new folderMan();

            IEnumerable<DirEntry> filesToProduction = fman.DirGetFolderContents(_config.DIR_Deploy);

            foreach (DirEntry f in filesToProduction.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(_config.DIR_Deploy + f.EntryPath, f.EntryName, _config.DIR_Product + f.EntryPath, f.EntryName);
            }
        }


        public void CopyNewAndChangedFiles()
        {
            log.Debug("CopyNewAndChangedFiles");

            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(_config.ReleaseNameTemplate);

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(_config.DIR_Source);
            IEnumerable<DirEntry> fldProudContent = fman.DirGetFolderContents(_config.DIR_Product);

            DirEntryEqualityComparer dirEntryEqualityComparer = new DirEntryEqualityComparer();
            IEnumerable<DirEntry> filesToDeploy = fldSourceContent.Except(fldProudContent, dirEntryEqualityComparer).ToList();

            /*
            StreamWriter w = new StreamWriter("c:\\temp\\te.txt");
            foreach (DirEntry f in filesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                string fromFile = cr.FTP_Server + f.EntryPath + f.EntryName;
                string toFile = cr.DIR_Backup + f.EntryPath + f.EntryName;
                w.WriteLine(fromFile + "|" + toFile);
            }
            w.Close();
            */

            foreach (DirEntry f in filesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(_config.DIR_Source + f.EntryPath, f.EntryName, _config.DIR_Deploy + f.EntryPath, f.EntryName);
            }
        }
    }
}
