using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PartialDeployer
{
    public class Deploy
    {

        private configMan _configMan;

        public Deploy()
        {
            _configMan = new configMan("");
        }

        public void CheckAndCreateNewFolders(string[] Srcfolders)
        {
            foreach (string folder in Srcfolders)
            {
                string SrcFolderPath = _configMan.DIR_Source;
                string PrdFolderPath = folder.Replace(SrcFolderPath, _configMan.DIR_Product);
                string DepFolderPath = folder.Replace(SrcFolderPath, _configMan.DIR_Deploy);

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
            if (Directory.Exists(_configMan.DIR_Deploy))
            {
                Directory.Delete(_configMan.DIR_Deploy, true);
            }
            Directory.CreateDirectory(_configMan.DIR_Deploy);
        }

        public void CopyAllDeployToProduction()
        {
            folderMan fman = new folderMan();

            IEnumerable<DirEntry> filesToProduction = fman.DirGetFolderContents(_configMan.DIR_Deploy);

            foreach (DirEntry f in filesToProduction.Where(x => x.EntryType == FtpEntryType.File).ToList())
            {
                fman.ForceCopy(_configMan.DIR_Deploy + f.EntryPath, f.EntryName, _configMan.DIR_Product + f.EntryPath, f.EntryName);
            }
        }


        public void CopyNewAndChangedFiles()
        {
            DateTime dt = DateTime.Now;
            string releaseName = dt.ToString(_configMan.ReleaseNameTemplate);

            folderMan fman = new folderMan();
            IEnumerable<DirEntry> fldSourceContent = fman.DirGetFolderContents(_configMan.DIR_Source);
            IEnumerable<DirEntry> fldProudContent = fman.DirGetFolderContents(_configMan.DIR_Product);

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
                fman.ForceCopy(_configMan.DIR_Source + f.EntryPath, f.EntryName, _configMan.DIR_Deploy + f.EntryPath, f.EntryName);
            }
        }
    }
}
