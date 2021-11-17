using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlluminaExplorer
{
   public static class FolderHelper
    {
        public static DirectoryInfo GetDirectoryInfo(string path) {

            DirectoryInfo info = new DirectoryInfo(path);
            return info;
        }
       
    }
}
