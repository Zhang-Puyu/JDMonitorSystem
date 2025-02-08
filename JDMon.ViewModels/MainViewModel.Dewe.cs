using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using System.IO;
using System.Runtime.CompilerServices;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
    {
        private DEWESoft.AppClass DeweApp = null;

        #region 存储路径
        [ObservableProperty]
        private string deweFileName = string.Empty;
        [ObservableProperty]
        private string deweFileAvailableSpace = string.Empty;
        [RelayCommand]
        private void ChooseDeweFileName()
        {
            // open a file dialog to choose save file name and path
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "请选择Dewesoft数据保存路径",
                Filter = "Dewesoft数据文件|*.dxd",
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // check if the file exists
                if (System.IO.File.Exists(saveFileDialog.FileName))
                    // if the file exists, ask if overwrite
                    if (MessageBox.Show("文件已存在，是否覆盖？", "文件已存在", MessageBoxButtons.YesNo) == DialogResult.No) 
                        return;
                
                DeweFileName = saveFileDialog.FileName;
                string driveName = DeweFileName.Substring(0, 2);
                DriveInfo driveInfo = new DriveInfo(driveName);
                DeweFileAvailableSpace = (driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024)).ToString("0.00") + " MB";
            }
        }
        [RelayCommand]
        private void OpenFileDir()
        {
            if (!string.IsNullOrEmpty(DeweFileName))
            {
                // open the dir of fileName and select the file
                Process.Start("explorer.exe", "/select," + DeweFileName);
            }
        }
        #endregion

        #region 初始化
        [ObservableProperty]
        private bool deweIsInit = false;
        [RelayCommand]
        public void InitDewe()
        {
            ShowMessageEvent("Info: Initializing DeweControl... ");

            try
            {
                DeweApp = new DEWESoft.AppClass();
                DeweApp.OnStartStoring += () => DeweIsStoring = true;
                DeweApp.OnStopStoring += () => DeweIsStoring = false;

                DeweApp.Init();
                DeweApp.Enabled = true;

                DeweIsInit = true;
            }
            catch (Exception ex)
            {
                ShowMessageEvent("Error: " + ex.Message);
                DeweIsInit = false;
            }

            ShowMessageEvent("Info: down. ");
        }
        #endregion

        #region 测量
        [ObservableProperty]
        private bool deweIsMeasuring = false;
        [RelayCommand]
        public void StartMeasureDewe()
        {
            if (DeweIsInit)
            {
                DeweApp.Start();
                DeweIsMeasuring = true;
                ShowMessageEvent("Info: Dewesoft Start measuring");
            }
            else
            {
                ShowMessageEvent("Error； Please init Dewesoft first");
            }
        }
        [RelayCommand]
        public void StopMeasureDewe()
        {
            if (DeweIsMeasuring)
            {
                DeweApp.Stop();
                ShowMessageEvent("Info: Dewesoft Stop measuring");
                DeweIsMeasuring = false;
            }
            else
            {
                ShowMessageEvent("Warn: Dewesoft is not measuring");
            }
        }
        #endregion

        #region 采集
        [ObservableProperty]
        private bool deweIsStoring = false;
        [RelayCommand]
        public void StartStoreDewe()
        {
            if (DeweIsInit)
            {
                if (string.IsNullOrEmpty(DeweFileName))
                {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    DeweFileName = Path.Combine(desktopPath, $"{DateTime.Now}.dxd");   
                }
                StartMeasureDewe();
                DeweApp.StartStoring(DeweFileName);
                ShowMessageEvent($"Info: Dewesoft Start storing to {DeweFileName}");
                DeweIsStoring = true;
            }
        }
        [RelayCommand]
        public void StopStoreDewe()
        {
            if (DeweIsStoring) 
            {
                DeweApp.StopStoring();
                ShowMessageEvent("Info: Dewesoft Stop storing");
                DeweIsStoring = false;
            } 
        }
        #endregion
    }
}
