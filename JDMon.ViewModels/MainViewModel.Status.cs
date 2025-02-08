
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
    {
        private int status;

        [ObservableProperty]
        private string progState;
        [ObservableProperty]
        private string progCtrlState;
        [ObservableProperty]
        private string operationMode;
        [ObservableProperty]
        private string appMode;

        private readonly object lockStatus = new object();
        private Task UpdateStatus()
        {
            lock(lockStatus)
            {
                if (jdMachMon.GetProgState(ref status))
                    ProgState = ProgStateDic[status];
                else
                    ShowMessageEvent("Error: 获取程序状态失败");
                if (jdMachMon.GetProgCtrlState(ref status))
                    ProgCtrlState = ProgCtrlStateDic[status];
                else
                    ShowMessageEvent("Error: 获取程序控制状态失败");
                if (jdMachMon.GetOprationMode(ref status))
                    OperationMode = OperationModeDic[status];
                else
                    ShowMessageEvent("Error: 获取工作模式失败");
            }
            return Task.CompletedTask;
        }

        #region 状态字典
        public static readonly Dictionary<int, string> ProgStateDic
            = new Dictionary<int, string>()
            {
                { 0, "停止" },
                { 1, "运行" },
                { 2, "暂停" },
                { 3, "复位" }
            };

        public static readonly Dictionary<int, string> ProgCtrlStateDic 
            = new Dictionary<int, string>()
            {
                { 0, "READY" },
                { 1, "空运行" },
                { 2, "单步运行" },
                { 4, "机床锁住" },
                { 8, "选择停止" },
                { 16, "程序段跳过" },
                { 32, "MST锁住" },
                { 64, "手轮试切" }
            };

        public static readonly Dictionary<int, string> OperationModeDic
            = new Dictionary<int, string>()
            {
                { 0, "无" },
                { 1, "程序编辑" },
                { 2, "程序自动内存运行" },
                { 4, "MDI运行" },
                { 8, "程序自动远程运行" },
                { 16, "点动"},
                { 32, "寸动" },
                { 64, "手轮" },
                { 128, "参考点" },
                { 256, "示教" }
            };

        public static readonly Dictionary<string, int> AppModeDic
            = new Dictionary<string, int>()
            {
                { "手动连续", 2 },
                { "增量进给", 3 },
                { "程序自动内存运行", 4 },
                { "MDI 运行", 5 },
                { "程序编辑", 6 }
            };
        #endregion
    }
}
