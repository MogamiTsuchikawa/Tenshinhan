﻿using System;
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
using System.Windows.Shapes;
using MicrosoftGraph;

namespace Tenshinhan
{
    /// <summary>
    /// AddErgWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class AddErgWindow : Window
    {
        public enum Kind
        {
            AddNewGame, AddGame
        }
        private Kind kind;
        private MicrosoftGraph.MicrosoftGraph microsoftGraph;
        public AddErgWindow(Kind kind, MicrosoftGraph.MicrosoftGraph microsoftGraph)
        {
            this.microsoftGraph = microsoftGraph;
            this.kind = kind;
            InitializeComponent();
            if (kind == Kind.AddGame)
            {
                AddErgToList();
            }
        }
        List<DataClass.ErgSave> notExistErgList = new();
        private async void AddErgToList()
        {
            notExistErgList = ErgFileManager.instanse.serverErgList.Where(
                erg => ErgFileManager.instanse.ergList.Where(e => e.title == erg.title).Count() == 0
                ).ToList();
            notExistErgList.ForEach(e =>
            {
                GameSelectComboBox.Items.Add(e.title);
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(kind == Kind.AddNewGame)
            {
                //既存のゲーム系のコントロールを無効化する
                GameSelectComboBox.IsEnabled = false;
            }
            else
            {
                //新規ゲームの追加のコントロールを無効化する
                GameNameTextBox.IsEnabled = false;
                GameMakerTextBox.IsEnabled = false;
                //SaveFolderSelectBox.IsEnabled = false;
            }
            if (kind == Kind.AddGame && notExistErgList.Count() == 0)
            {
                MessageBox.Show("追加できるゲームがありません");
                this.Close();
            }
        }
        public string gameName { get; private set; }
        public string gameMaker { get; private set; }
        public string appPath { get; private set; }
        public string saveFolderPath { get; private set; }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if(kind == Kind.AddNewGame)
            {
                //新規ゲーム
                gameName = GameNameTextBox.Text;
                gameMaker = GameMakerTextBox.Text;

            }
            else
            {
                //既存ゲーム
                //OneDriveから取得したリストを利用する
                var targetErg = notExistErgList[GameSelectComboBox.SelectedIndex];
                gameName = targetErg.title;
                gameMaker = targetErg.maker;
            }
            appPath = AppPathSelectBox.SelectPath;
            saveFolderPath = SaveFolderSelectBox.SelectPath;
            this.Close();
        }
    }
}
