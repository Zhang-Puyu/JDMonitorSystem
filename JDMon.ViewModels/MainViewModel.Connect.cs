using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
    {
        #region 连接
        private const int RPCTimeOut = 1000;
        private const int ConnectTimeOut = 1000;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
        private string ip = "169.254.144.62";

        [ObservableProperty]
        private string connectButtonIcon = ".\\Icons\\连接.png";
        [ObservableProperty]
        private string connectButtonText = "连接";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CollectCommand))]
        private bool isConnected = false;

        private bool CanConnect => !string.IsNullOrWhiteSpace(Ip);
        [RelayCommand(CanExecute = nameof(CanConnect))]
        private void Connect()
        {
            if (jdMachMon.IsConnect())
            {
                ShowMessageEvent($"Info: Disconnect ...");

                StopUpdate();

                if (jdMachMon.DisconnectJDMach())
                {
                    ConnectButtonIcon = ".\\Icons\\连接.png";
                    ConnectButtonText = "连接";

                    IsConnected = false;
                    ShowMessageEvent($"Info: Disconnected");
                }
                else
                {
                    ShowMessageEvent("Error: Disconnect FAIL");
                }

            }
            else
            {
                ShowMessageEvent($"Info: Connect to {Ip} ...");
                jdMachMon.SetRPCTimeout(RPCTimeOut);
                jdMachMon.SetConnectionTimeout(ConnectTimeOut);
                if (jdMachMon.ConnectJDMach(Ip))
                {
                    ConnectButtonIcon = ".\\Icons\\删除线.png";
                    ConnectButtonText = "断开";

                    Stopwatch.Start();
                    StartUpdate();

                    UpdateTaskInfo();

                    IsConnected = true;

                    ShowMessageEvent($"Info: Connection SUCCESS!");
                }
                else
                {
                    ShowMessageEvent("Error: Connection FAIL");
                    PingIp();
                }
            }
        }
        #endregion

    }
}
