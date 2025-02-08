using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
    {
        [RelayCommand]
        private void DebugFunc()
        {
#if DEBUG
            if (jdMachMon.GetProgState(ref status))
            {
                ShowMessageEvent("Info: program status: " + status);
                try
                {
                    ShowMessageEvent(ProgStateDic[status]);
                }
                catch { }
            }
            else
                ShowMessageEvent("Error: 获取程序状态失败");
            if (jdMachMon.GetProgCtrlState(ref status))
            {
                ShowMessageEvent("Info: program control status: " + status);
                try
                {
                    ShowMessageEvent(ProgStateDic[status]);
                }
                catch { }
            }
            else
                ShowMessageEvent("Error: 获取程序控制状态失败");
            if (jdMachMon.GetOprationMode(ref status))
            {
                ShowMessageEvent("Info: opration mode status: " + status);
                try { ShowMessageEvent(OperationModeDic[status]); }
                catch { }
            }

            else
                ShowMessageEvent("Error: 获取工作模式失败");
#endif
        }

        [RelayCommand]
        private void PingIp()
        {
            ShowMessageEvent("Info: try to ping " + Ip);
            PingReply reply = new Ping().Send(Ip, 120);

            if (reply.Status == IPStatus.Success)
            {
                ShowMessageEvent("Info: ping " + Ip + " success");
                MessageBox.Show($"{Ip} 可Ping通", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ShowMessageEvent("Error: ping " + Ip + " fail");
                MessageBox.Show($"{Ip} 未连接，请检查网线和IP地址", "失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                
        }
    }
}
