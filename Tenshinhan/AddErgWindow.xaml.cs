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
        public AddErgWindow(Kind kind)
        {
            this.kind = kind;
            InitializeComponent();
            if (kind == Kind.AddGame) ;
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
                SaveFolderSelectBox.IsEnabled = false;
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}