using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tenshinhan.DataClass
{
    class ErgSave: Erg
    {
        public DateTime lastUpdateTime { get; set; }
        public string oneDriveFileId { get; set; }
        public string saveDataPath { get; set; }
        public enum SaveDataPathKind
        {
            DocumentsFolder,AppDataFolder, AppExistFolder 
        }
        public SaveDataPathKind saveDataPathKind { get; set; }
        
    }

    class LocalErgSave: ErgSave
    {
        public string appPath { get; set; }
    }
}
