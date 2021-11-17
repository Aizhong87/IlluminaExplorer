using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IlluminaExplorer
{
    public class UserPickInfo
    {
        public string SelectedPath { get; set; }


        private string _delimiter;
        public string Delimiter
        {

            get
            {
                string del = "";
                switch (_delimiter)
                {
                    case "Comma":
                        del = ",";
                        break;
                    case "Pipe":
                        del = "|";
                        break;
                    default:
                        del = ",";
                        break;
                }
                return del;
            } set
            {
                _delimiter = value;
            }
        }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FullPath
        {

            get { return SelectedPath.ToLower() + "\\" + FileName.ToLower(); }

        }

        private bool _readFile = false;
        public bool ReadFile
        {

            get { return _readFile; }

            set { _readFile = value; }

        }

    }
}
