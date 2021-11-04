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
using Microsoft.Win32;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Tenshinhan
{
    /// <summary>
    /// FileFolderSelectBox.xaml の相互作用ロジック
    /// </summary>
    public partial class FileFolderSelectBox : UserControl
    {
        public FileFolderSelectBox()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty FolderSelectModeProperty = DependencyProperty.Register("FolderSelectMode", typeof(bool), typeof(FileFolderSelectBox), new FrameworkPropertyMetadata(false));
        public bool FolderSelectMode
        {
            get { return (bool)GetValue(FolderSelectModeProperty); }
            set { SetValue(FolderSelectModeProperty, value); }
        }

        public static readonly DependencyProperty SelectPathProperty = DependencyProperty.Register("SelectPath", typeof(string), typeof(FileFolderSelectBox), new FrameworkPropertyMetadata(""));
        public string SelectPath
        {
            get { return (string)GetValue(SelectPathProperty); }
            set 
            { 
                SetValue(SelectPathProperty, value);
                PathTextBox.Text = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (FolderSelectMode)
            {
                FolderBrowserDialog folderBrowserDialog = new();
                if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    PathTextBox.Text = folderBrowserDialog.SelectedPath;
                    SetValue(SelectPathProperty, folderBrowserDialog.SelectedPath);
                }
                return;
            }
            OpenFileDialog dialog = new();
            if(dialog.ShowDialog() == true)
            {
                PathTextBox.Text = dialog.FileName;
                SetValue(SelectPathProperty, dialog.FileName);
            }
        }
    }
}
