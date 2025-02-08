using Newtonsoft.Json;
using System.IO;
using System;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JDMon.ViewModels
{
    internal class Config
    {
        public string Ip { set; get; }
        public uint CollectInterval { set; get; }
        public uint UpdateInterval { set; get; }
        public string WorkPath { set; get; }
    }

    public partial class MainViewModel
    {
        [ObservableProperty]
        private string currConfigFile = ".\\config.json";

        #region 配置

        [RelayCommand]
        private void OpenConfigFile()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "打开配置文件",
                Multiselect = false,
                Filter = "配置文件 | *.json"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CurrConfigFile = dialog.FileName;
                ReadConfig();
            }
        }

        [RelayCommand]
        private void SaveAsConfigFile()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "打开配置文件",
                Filter = "配置文件 | *.json"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                CurrConfigFile = dialog.FileName;
                WriteConfig();
            }
        }

        [RelayCommand]
        private void SaveConfigFile()
        {
            WriteConfig();
        }


        private void ReadConfig()
        {
            ShowMessageEvent($"Info: 读取配置文件{CurrConfigFile}");
            try
            {
                using (StreamReader reader = new StreamReader(CurrConfigFile))
                {
                    string json = reader.ReadToEnd();
                    Config config = JsonConvert.DeserializeObject<Config>(json);

                    if (jdMachMon.IsConnect())
                        jdMachMon.DisconnectJDMach();

                    Ip = config.Ip;
                    CollectInterval = config.CollectInterval;
                    UpdateInterval = config.UpdateInterval;
                    WorkPath = config.WorkPath;
                }
            }
            catch (FileNotFoundException ex)
            {
                ShowMessageEvent($"Error: 文件 {CurrConfigFile} 未找到。{ex.Message}");
            }
            catch (Exception ex)
            {
                ShowMessageEvent($"Error: 读取或解析JSON文件时发生错误。{ex.Message}");
            }
        }

        private void WriteConfig()
        {
            ShowMessageEvent($"Info: 写入配置文件{CurrConfigFile}");
            try
            {
                Config config = new Config
                {
                    Ip = Ip,
                    CollectInterval = CollectInterval,
                    UpdateInterval = UpdateInterval,
                    WorkPath = WorkPath
                };

                string json = JsonConvert.SerializeObject(config);
                if (File.Exists(CurrConfigFile))
                    File.Delete(CurrConfigFile);
                using (StreamWriter writer = new StreamWriter(CurrConfigFile))
                    writer.Write(json);
            }
            catch (Exception ex)
            {
                ShowMessageEvent($"Error: 写入JSON文件时发生错误。{ex.Message}");
                throw;
            }

        }

        #endregion
    }

}