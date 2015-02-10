using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartialDeployer
{
    class Laravel
    {
        public static string[] GetFilteredFileNamesWithPath(string[] fldSource)
        {
            string[] allowedList = (".png|.jpg|.jpeg|.gif|.html|.htm|.css|.php|.js|.json|.sql|.readme|.htaccess|.txt|.yml|.xml|.xsd|.csv|.pipe|.pdf").Split('|');
            string[] deniedList = (".env|.git|.gitignore|.gitattributes|.svn-base|sess_|.idea|log-|.less|\\package\\|\\storage\\sessions\\|\\bundle\\|\\app\\storage\\").Split('|');
            string[] s = fldSource.ToList().Where(x => allowedList.Any(al => x.EndsWith(al))).ToArray();
            string[] s1 = s.ToList().Where(x => !deniedList.Any(al => x.EndsWith(al))).ToArray();
            return s1;
        }
    }
}
