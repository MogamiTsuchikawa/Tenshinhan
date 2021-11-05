using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MicrosoftGraph;
using Tenshinhan.DataClass;
using System.Diagnostics;
using Microsoft.Win32;

namespace Tenshinhan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ErgFileManager ergFileManager;
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            /*var res = await App.MicrosoftGraph.Auth(this);
            AuthBtn.Content = res ? "認証成功" : "認証失敗";
            var client = await App.MicrosoftGraph.GetClient();
            var items = await client.Me.Drive.Root.Children.Request().GetAsync();
            foreach(var item in items)
            {
                if(item.Folder == null)
                {
                    ResultBox.Text += $"{item.Name} \n";
                }
                else
                {
                    ResultBox.Text += $"{item.Name} (フォルダ) \n";
                }
            }*/
        }

        private void AddNewGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ergFileManager == null) 
            {
                RequestLoginMessage();
                return; 
            }
            AddErgWindow addErgWindow = new(AddErgWindow.Kind.AddNewGame, App.MicrosoftGraph);
            addErgWindow.ShowDialog();
            if (addErgWindow.appPath == null) return;

            ergFileManager.AddNewErg(
                addErgWindow.gameName, 
                addErgWindow.gameMaker, 
                addErgWindow.appPath, 
                addErgWindow.saveFolderPath);

        }

        private void AddGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ergFileManager == null)
            {
                RequestLoginMessage();
                return;
            }
            AddErgWindow addErgWindow = new(AddErgWindow.Kind.AddGame, App.MicrosoftGraph);
            addErgWindow.ShowDialog();
        }

        private async void OneDriveLoginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //MicrosoftLogin
            var res = await App.MicrosoftGraph.Auth(this);
            if (res)
            {
                OneDriveLoginMenuItem.Header = "ログイン中";
                OneDriveLoginMenuItem.IsEnabled = false;
                OneDriveLogoutMenuItem.IsEnabled = true;
                MainWindowWindow.Title = "チャーハン天津飯(Microsoftログイン済)";
            }
            ergFileManager = new ErgFileManager(App.MicrosoftGraph);
        }

        private async void OneDriveLogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //MicrosoftLogout
            var res = await App.MicrosoftGraph.Logout();
            if (res)
            {
                OneDriveLoginMenuItem.Header = "ログイン";
                OneDriveLoginMenuItem.IsEnabled = true;
                OneDriveLogoutMenuItem.IsEnabled = false;
                MainWindowWindow.Title = "天津飯";
            }
            ergFileManager = null;
        }
        private async void LanuchErg(LocalErgSave erg)
        {
            if (!ergFileManager.isReady)
            {
                MessageBox.Show("起動直後の処理中です。少々お待ちください。");
                return;
            } 
                
            if (ergFileManager.CheckErgSaveDataUpdate(erg))
            {
                var upResult = MessageBox.Show(
                    "OneDriveに最新のセーブデータを確認しました。\nダウンロードしますか？",
                    "更新確認",
                    MessageBoxButton.YesNo);
                if(upResult == MessageBoxResult.Yes)
                {
                    await ergFileManager.DownloadLatestSaveData(erg);
                }
            }
            Process p = Process.Start(erg.appPath);
            p.WaitForExit();
            var result = MessageBox.Show(
                "セーブデータをOneDriveにアップロードしますか？",
                "終了時確認",
                MessageBoxButton.YesNo);
            if(result == MessageBoxResult.Yes)
            {
                ergFileManager.UpdateOneDriveSaveData(erg);
            }
        }
        private void RequestLoginMessage()
        {
            MessageBox.Show(
                "先にMicrosoftアカウントへのログインが必要です",
                "ログインエラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error
                );
        }
        private void SetErgListBox(List<LocalErgSave> list)
        {
            ErgList.Items.Clear();
            list.ForEach(erg =>
            {
                ErgListItem ergListItem = new(erg, LanuchErg);
                ErgList.Items.Add(ergListItem);
            });
            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await App.MicrosoftGraph.SilentAuth();
            if (res)
            {
                OneDriveLoginMenuItem.Header = "ログイン中";
                OneDriveLoginMenuItem.IsEnabled = false;
                MainWindowWindow.Title = "チャーハン天津飯(Microsoftログイン済)";
                ergFileManager = new ErgFileManager(App.MicrosoftGraph);
                ergFileManager.windowUpdateAction = SetErgListBox;
                SetErgListBox(ergFileManager.ergList);
            }
            else
            {
                OneDriveLogoutMenuItem.IsEnabled = false;
                MainWindowWindow.Title = "天津飯";
            }
        }

        private void UpdateErgListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ergFileManager == null)
            {
                RequestLoginMessage();
                return;
            }
            SetErgListBox(ergFileManager.ergList);
        }
    }
}
