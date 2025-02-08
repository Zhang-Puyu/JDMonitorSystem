﻿
using System.Runtime.InteropServices;
using System;
using System.Windows;


namespace JDMon.Application
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel(jdHostControl.JDMonHost);
        }
    }
}
