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

namespace Tenshinhan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            AddErgWindow addErgWindow = new(AddErgWindow.Kind.AddNewGame);
            addErgWindow.ShowDialog();
        }

        private void AddGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddErgWindow addErgWindow = new(AddErgWindow.Kind.AddGame);
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
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await App.MicrosoftGraph.SilentAuth();
            if (res)
            {
                OneDriveLoginMenuItem.Header = "ログイン中";
                OneDriveLoginMenuItem.IsEnabled = false;
                MainWindowWindow.Title = "チャーハン天津飯(Microsoftログイン済)";
            }
            else
            {
                OneDriveLogoutMenuItem.IsEnabled = false;
                MainWindowWindow.Title = "天津飯";
            }
        }
    }
}
