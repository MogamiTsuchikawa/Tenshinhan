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
        public bool isReady { get; private set; } = false;
        public List<LocalErgSave> ergList { get; private set; }
        private string jsonPath = "ergs.json";
        private DriveItem targetOneDriveFolder;
        private DriveItem serverJson;
        private string localServerJsonPath = "server.json";
        public Action<List<LocalErgSave>> windowUpdateAction;
        public List<ErgSave> serverErgList { get; private set; }
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
        public void SaveJson()
        {
            string jsonStr = JsonSerializer.Serialize(ergList);
            System.IO.File.WriteAllText(jsonPath, jsonStr);
        }
        public async void SaveServerJson()
        {
            var client = await graph.GetClient();
            string jsonStr = JsonSerializer.Serialize(serverErgList);
            System.IO.File.WriteAllText(localServerJsonPath, jsonStr);
            serverJson = await client.Me.Drive.Items[targetOneDriveFolder.Id]
                    .ItemWithPath(localServerJsonPath).Content.Request()
                    .PutAsync<DriveItem>(new StreamReader(localServerJsonPath).BaseStream);
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
                serverJson = await client.Me.Drive.Items[targetOneDriveFolder.Id]
                    .ItemWithPath(localServerJsonPath).Content.Request()
                    .PutAsync<DriveItem>(streamReader.BaseStream);
                serverErgList = new();
            }
            else
            {
                targetOneDriveFolder = tenshinFolder[0];
                //server.jsonを探す&読み出してserverErgListに入れる
                var tenshinFolderItems = await client.Me.Drive.Items[targetOneDriveFolder.Id].Children.Request().GetAsync();
                var serverJson = tenshinFolderItems.Where(i => i.Name == "server.json").ToList()[0];
                await DownloadOneDriveFile(serverJson.Id, serverJson.Name);
                string jsonStr = System.IO.File.ReadAllText(localServerJsonPath);
                serverErgList = JsonSerializer.Deserialize<List<ErgSave>>(jsonStr);
            }
            isReady = true;
        }
        private async Task<bool> DownloadOneDriveFile(string targetId, string fileName)
        {
            var client = await graph.GetClient();
            Stream stream = await client.Me.Drive.Items[targetId].Content.Request().GetAsync();
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer);
            stream.Close();
            FileStream fs = new(fileName, FileMode.Create);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            return true;
        }
        /// <summary>
        /// Server.jsonの内容と比較してServer側が最新であるかを確認する
        /// </summary>
        /// <returns>Updateがあるか</returns>
        public bool CheckErgSaveDataUpdate(LocalErgSave target)
        {
            //あとでサーバー側にデータがなかったときに対処を書く
            var serverver = serverErgList.Where(e => e.uuid == target.uuid).ToList()[0];
            return serverver.lastUpdateTime > target.lastUpdateTime;
        }
        public async Task<bool> DownloadLatestSaveData(LocalErgSave target)
        {
            var serverver = serverErgList.Where(e => e.uuid == target.uuid).ToList()[0];
            await DownloadOneDriveFile(target.oneDriveFileId, "downloadtemp.zip");
            //まずは既存のセーブデータの削除
            var di = new DirectoryInfo(target.saveDataPath);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectory in di.EnumerateDirectories())
            {
                subDirectory.Delete(true);
            }
            foreach (DirectoryInfo subDirectory in di.EnumerateDirectories())
            {
                subDirectory.Delete(true);
            }
            ZipFile.ExtractToDirectory("downloadtemp.zip", target.saveDataPath);
            System.IO.File.Delete("downloadtemp.zip");
            return true;
        } 
        public async void UpdateOneDriveSaveData(LocalErgSave target)
        {
            var client = await graph.GetClient();
            target.lastUpdateTime = DateTime.Now;
            string zipPath = GenerateSaveDataZip(target);
            StreamReader streamReader = new(zipPath);
            var uploadSaveData = await client.Me.Drive.Items[targetOneDriveFolder.Id]
                .ItemWithPath(zipPath).Content.Request()
                .PutAsync<DriveItem>(streamReader.BaseStream);
            System.IO.File.Delete(zipPath);
            target.oneDriveFileId = uploadSaveData.Id;
            var serverver = serverErgList.Where(e => e.uuid == target.uuid).ToList()[0];
            serverver.lastUpdateTime = target.lastUpdateTime;
            serverver.oneDriveFileId = target.oneDriveFileId;
            SaveServerJson();
            SaveJson();
            windowUpdateAction?.Invoke(ergList);
        }
        public async void AddNewErg(string ergName, string makerName, string appPath, string saveFolderPath)
        {
            var client = await graph.GetClient();
            LocalErgSave newErg = new();
            newErg.title = ergName;
            newErg.maker = makerName;
            newErg.appPath = appPath;
            newErg.saveDataPath = saveFolderPath;
            newErg.uuid = Guid.NewGuid().ToString("D");
            newErg.lastUpdateTime = DateTime.Now;
            //saveデータをzipにする（初回）
            string zipPath = GenerateSaveDataZip(newErg);
            //OneDriveにアップロードしてFileIDを得てnewErgに入れる
            StreamReader streamReader = new(zipPath);
            var uploadSaveData = await client.Me.Drive.Items[targetOneDriveFolder.Id]
                .ItemWithPath(zipPath).Content.Request()
                .PutAsync<DriveItem>(streamReader.BaseStream);
            System.IO.File.Delete(zipPath);
            newErg.oneDriveFileId = uploadSaveData.Id;
            //OneDriveのjsonに追記する
            serverErgList.Add(newErg);
            SaveServerJson();
            ergList.Add(newErg);
            SaveJson();
            windowUpdateAction?.Invoke(ergList);
            
        }
        public string GenerateSaveDataZip(LocalErgSave targetErg)
        {
            System.IO.File.Delete($"{targetErg.uuid}.zip");
            try
            {
                ZipFile.CreateFromDirectory(targetErg.saveDataPath, $"{targetErg.uuid}.zip");
            }
            catch(Exception ex)
            {
                return "";//あとでなんとかする
            }
            return $"{targetErg.uuid}.zip";
        }

    }
}
