using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicrosoftGraph;
using Tenshinhan.DataClass;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using Microsoft.Graph;

namespace Tenshinhan
{
    public class ErgFileManager
    {
        private MicrosoftGraph.MicrosoftGraph graph;
        private bool isReady = false;
        private List<LocalErgSave> ergList;
        private string jsonPath = "ergs.json";
        private DriveItem targetOneDriveFolder;
        private DriveItem serverJson;
        private string localServerJsonPath = "server.json";
        public ErgFileManager(MicrosoftGraph.MicrosoftGraph graph)
        {
            this.graph = graph;
            if (!System.IO.File.Exists(jsonPath))
            {
                System.IO.File.WriteAllText(jsonPath, "[]");
            }
            string jsonStr = System.IO.File.ReadAllText(jsonPath);
            ergList = JsonSerializer.Deserialize<List<LocalErgSave>>(jsonStr);
            //OneDriveに指定のフォルダとjsonファイルがあるかを確認する
            CheckInitSettings();
        }
        private async void CheckInitSettings()
        {
            var client = await graph.GetClient();
            var items = await client.Me.Drive.Root.Children.Request().GetAsync();
            var tenshinFolder = items.Where(item => item.Name == "Tenshinhan" && item.Folder != null).ToList();
            if(tenshinFolder.Count == 0)
            {
                DriveItem driveItem = new();
                driveItem.Name = "Tenshinhan";
                driveItem.Folder = new Folder();
                targetOneDriveFolder = await client.Me.Drive.Root.Children.Request().AddAsync(driveItem);
                System.IO.File.WriteAllText(localServerJsonPath, "[]");
                System.IO.StreamReader streamReader = new(localServerJsonPath);
                serverJson = await client.Me.Drive.Items[targetOneDriveFolder.Id].ItemWithPath(localServerJsonPath).Content.Request().PutAsync<DriveItem>(streamReader.BaseStream);
            }
            else
            {
                targetOneDriveFolder = tenshinFolder[0];
                //server.jsonを探す
            }
        }
        public void AddNewErg(string ergName, string makerName, string appPath, string saveFolderPath)
        {
            LocalErgSave newErg = new();
            newErg.title = ergName;
            newErg.maker = makerName;
            newErg.appPath = appPath;
            newErg.saveDataPath = saveFolderPath;
            newErg.uuid = Guid.NewGuid().ToString("D");
            //saveデータをzipにする（初回）
            string zipPath = GenerateSaveDataZip(newErg);
            //OneDriveにアップロードしてFileIDを得てnewErgに入れる
            //OneDriveのjsonに追記する
            ergList.Add(newErg);
        }
        public string GenerateSaveDataZip(LocalErgSave targetErg)
        {
            try
            {
                ZipFile.CreateFromDirectory(targetErg.saveDataPath, $"{targetErg.uuid}.zip");
            }
            catch(Exception ex)
            {
                return "";
            }
            return $"{targetErg.uuid}.zip";
        }

    }
}
