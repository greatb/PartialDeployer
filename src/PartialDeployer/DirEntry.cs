using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialDeployer
{
    public enum FtpEntryType
    {
        Unknown = 0,
        File = 1,
        Folder = 2
    }

    public class DirEntry
    {
        public FtpEntryType EntryType
        {
            get;
            set;
        }

        public string EntryName
        {
            get;
            set;
        }

        public string EntryPath
        {
            get;
            set;
        }

        public string FullLine
        {
            get;
            set;
        }

        public DateTime DateModified
        {
            get;
            set;
        }

    }

    class DirEntryEqualityComparer : IEqualityComparer<DirEntry>
    {
        public bool Equals(DirEntry x, DirEntry y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) ||
                Object.ReferenceEquals(y, null))
                return false;

            return x.DateModified == y.DateModified &&
                    x.EntryName == y.EntryName &&
                    x.EntryPath == y.EntryPath &&
                    x.EntryType == y.EntryType;
        }

        public int GetHashCode(DirEntry entry)
        {
            if (Object.ReferenceEquals(entry, null)) return 0;

            int hashDateModified = (entry.DateModified == null) ? 0 : entry.DateModified.GetHashCode();
            int hashEntryName = (entry.EntryName == null) ? 0 : entry.EntryName.GetHashCode();
            int hashEntryPath = (entry.EntryPath == null) ? 0 : entry.EntryPath.GetHashCode();
            int hashEntryType = (entry.EntryType == null) ? 0 : entry.EntryType.GetHashCode();

            return hashDateModified ^ hashEntryName ^ hashEntryPath ^ hashEntryType;
        }
    }
}
