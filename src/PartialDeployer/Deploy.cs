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

        private configMan config;

        public Deploy(configMan configMan)
        {
            config = configMan;
        }

        public void CheckAndCreateNewFolders(string[] Srcfolders)
        {
            log.Debug("CheckAndCreateNewFolders");

            foreach (string folder in Srcfolders)
            {
                string SrcFolderPath = config.DIR_Source;
                string PrdFolderPath = folder.Replace(SrcFolderPath, config.DIR_Product);
                string DepFolderPath = folder.Replace(SrcFolderPath, config.DIR_Deploy);

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

            ResetFolder(config.DIR_Deploy);
        }

        private void ResetFolder(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
            Directory.CreateDirectory(folderName);
        }

        public void CopyAllDeployToProduction()
        {
            log.Debug("CopyAllDeployToProduction");

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> filesToProduction = fman.DirGetFolderContents(config.DIR_Deploy);

            foreach (DirEntry f in filesToProduction.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Deploy + f.EntryPath, f.EntryName, config.DIR_Product + f.EntryPath, f.EntryName);
            }
        }

        public void CopySourceToReleaseFiltered()
        {
            log.Debug("CopySourceToReleaseFiltered");

            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(config.ReleaseNameTemplate);

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(config.DIR_Source);
            foreach (DirEntry f in fldSourceContent.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Source + f.EntryPath, f.EntryName, config.DIR_Release + f.EntryPath, f.EntryName);
            }
        }

        public void CopySourceToDeployFiltered()
        {
            log.Debug("CopySourceToDeployFiltered");

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(config.DIR_Source);
            foreach (DirEntry f in fldSourceContent.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Source + f.EntryPath, f.EntryName, config.DIR_Deploy + f.EntryPath, f.EntryName);
            }
        }

        public void CopyTopperToDeployFiltered()
        {
            log.Debug("CopyTopperToDeployFiltered");

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> fldTopperContent = fman.DirGetFolderContents(config.DIR_Topper);
            foreach (DirEntry f in fldTopperContent.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Topper + f.EntryPath, f.EntryName, config.DIR_Deploy + f.EntryPath, f.EntryName);
            }
        }

        public void CopyNewAndChangedReleaseFilesToDeploy()
        {
            log.Debug("CopyNewAndChangedReleaseFilesToDeploy");

            DirEntryEqualityComparer dirEntryEqualityComparer = new DirEntryEqualityComparer();
            folderMan fman = new folderMan();

            IEnumerable<DirEntry> fldProductContent = fman.DirGetFolderContents(config.DIR_Product);
            IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(config.DIR_Source);
            IEnumerable<DirEntry> fldTopperContent = fman.DirGetFolderContents(config.DIR_Topper);

            IEnumerable<DirEntry> sourFilesToDeploy = fldSourceContent.Except(fldProductContent, dirEntryEqualityComparer).ToList();
            IEnumerable<DirEntry> toppFilesToDeploy = fldTopperContent.Except(fldProductContent, dirEntryEqualityComparer).ToList();

            foreach (DirEntry f in sourFilesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Source + f.EntryPath, f.EntryName, config.DIR_Deploy + f.EntryPath, f.EntryName);
            }

            foreach (DirEntry f in toppFilesToDeploy.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(config.DIR_Topper + f.EntryPath, f.EntryName, config.DIR_Deploy + f.EntryPath, f.EntryName);
            }
        }

        public void CopyTopperFilesToRelease()
        {
            if (!string.IsNullOrEmpty(config.DIR_Topper))
            {
                log.Debug("CopyDeployTopperFiles");

                folderMan fman = new folderMan();
                IEnumerable<DirEntry> filesTopper = fman.DirGetFolderContents(config.DIR_Topper);

                foreach (DirEntry f in filesTopper.Where(x => x.EntryType == FtpEntryType.File).ToList())
                {
                    fman.ForceCopy(config.DIR_Topper + f.EntryPath, f.EntryName, config.DIR_Release + f.EntryPath, f.EntryName);
                }
            }
        }
    }
}
