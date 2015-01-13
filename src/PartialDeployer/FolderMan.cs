using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PartialDeployer
{
    using System.IO;
    using log4net;

    public class folderMan
    {
        private static readonly ILog log = LogManager.GetLogger("folderMan");

        string[] deliedList = (".git|.gitignore|.gitattributes|.svn-base|sess_|.idea|log-|.less|package\\|app\\storage\\sessions|bundle\\|storage\\").Split('|');

        public bool ForceCopy(string fromPath, string fromFile, string toPath, string toFile)
        {
            log.Debug("ForceCopy");

            if (!Directory.Exists(toPath))
            {
                Directory.CreateDirectory(toPath);
            }
            File.Copy(fromPath + fromFile, toPath + toFile, true);
            return true;
        }

        private bool AllowedPattern(string path)
        {
            int loc = deliedList.Count(x => path.Contains("\\" + x));
            return !(loc > 0);
        }

        public List<DirEntry> DirGetFolder(string sFolder)
        {
            log.Debug("DirGetFolder");

            List<DirEntry> DirEntries = new List<DirEntry>();
            DirectoryInfo dirInfo = new DirectoryInfo(sFolder);
            DirectoryInfo[] dirFolders = dirInfo.GetDirectories("*.*", SearchOption.AllDirectories);
            for (int i = 0; i < dirFolders.Length; i++)
            {
                if (AllowedPattern(dirFolders[i].FullName))
                {
                    DirEntries.Add(new DirEntry()
                    {
                        EntryPath = dirFolders[i].FullName.Replace(sFolder, String.Empty),
                        EntryType = FtpEntryType.Folder
                    });
                }
            }
            return DirEntries;
        }

        public List<DirEntry> DirGetFolderContents(string sFolder)
        {
            log.Debug("DirGetFolderContents");

            log.InfoFormat("Reading Folder : {0}", sFolder);
            List<DirEntry> DirEntries = new List<DirEntry>();
            configMan cr = new configMan("");

            DirectoryInfo dirInfo = new DirectoryInfo(sFolder);
            if (dirInfo.Exists)
            {
                FileInfo[] dirFiles = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
                for (int i = 0; i < dirFiles.Length; i++)
                {
                    if (AllowedPattern(dirFiles[i].FullName))
                    {
                        DirEntry de = new DirEntry()
                        {
                            EntryPath = dirFiles[i].DirectoryName.Replace(sFolder, String.Empty) + "\\",
                            EntryName = dirFiles[i].Name,
                            EntryType = FtpEntryType.File,
                            DateModified = dirFiles[i].LastWriteTimeUtc
                        };
                        DirEntries.Add(de);
                    }
                }
            }
            return DirEntries;
        }
    }
}
