using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System;
using AxNCMONIOEXLib;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace JDMon.ViewModels
{

    public partial class MainViewModel : ObservableObject
    {
        private readonly AxNcMonIOEx jdMachMon = null;

        public event Action<string> ShowMessageEvent;

        public MainViewModel(AxNcMonIOEx JdMachMon)
        {
            this.jdMachMon = JdMachMon;

            coord = new double[6];
            spInfo = new double[3];
            ratesInfo = new int[2];

            AllocConsole();
            ShowMessageEvent += Shell.WriteLine;
        }

        #region 监测对象属性

        // 时间戳
        private readonly List<long> TimeStamps = new List<long>();

        // 程序名
        [ObservableProperty]
        private string currProgName = "";

        // 主轴坐标
        private double[] coord;
        private object objMachCoord;
        private object objAbsCoord;
        private object objRelCoord;

        private readonly List<double[]> Coords = new List<double[]>();

        [ObservableProperty]
        private double x = 0D;
        [ObservableProperty]
        private double y = 0D;
        [ObservableProperty]
        private double z = 0D;
        [ObservableProperty]
        private double a = 0D;
        [ObservableProperty]
        private double c = 0D;

        // 主轴信息-[0]电流，[1]转矩
        private double[] spInfo;
        private object objSpInfo;
        [ObservableProperty]
        private double electricity = 0D;
        [ObservableProperty]
        private double torque = 0D;
        [ObservableProperty]
        private double power = 0D;

        private readonly List<double[]> SpInfos = new List<double[]>();

        [ObservableProperty]
        private float feed = 0.0F;
        [ObservableProperty]
        private int rev = 0;

        // 倍率-[0]进给倍率，[1]转速倍率
        private int[] ratesInfo;
        private object objRates;
        [ObservableProperty]
        private int feedRate = 0;
        [ObservableProperty]
        private int revRate = 0;

        [ObservableProperty]
        private string currNcCode = "--";
        [ObservableProperty]
        private int currNcCodeLineNo = 0;
        [ObservableProperty]
        private bool shallCollectNcCode = true;
        [ObservableProperty]
        private bool shallCollectNcCodeLineNo = false;

        int workpieceCoord = 0;
        [ObservableProperty]
        int toolNo = 0;
        [ObservableProperty]
        float machTime = 0;
        int currProgNo = 0;
        int currMainProgNo = 0;

        private readonly List<string> CodeLines = new List<string>();
        private readonly List<int> CodeLineNos = new List<int>();
        #endregion

        #region 时间参数
        // 设置定时器的间隔为10毫秒
        [ObservableProperty]
        private uint collectInterval = 10;
        [ObservableProperty]
        private uint updateInterval = 500;
        // 设置定时器的分辨率为1毫秒
        [ObservableProperty]
        private uint resolution = 1;
        #endregion

        #region 计时器
        // 定义从系统获取性能计数器的函数
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        // 定义从系统获取性能计数器频率的函数
        [DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceFrequency(out long lpFrequency);

        // C#内置提供
        private Stopwatch Stopwatch = new Stopwatch();
        #endregion

        #region 触发器
        // 声明WinAPI中的Multimedia Timer相关函数
        [DllImport("winmm.dll", EntryPoint = "timeSetEvent")]
        static extern uint TimeSetEvent(uint delay, uint resolution, TimerCallbackDelgate callback, UIntPtr user, uint eventType);
        [DllImport("winmm.dll", EntryPoint = "timeKillEvent")]
        static extern void TimeKillEvent(uint timerId);
        // 定时器指针
        static uint TimerId_Collect = 0;
        static uint TimerId_Update = 0;

        // 回调委托类型
        delegate void TimerCallbackDelgate(uint id, uint msg, UIntPtr user, UIntPtr dw1, UIntPtr dw2);
        // 定时器回调方法
        private event TimerCallbackDelgate TimerCallbackEvent_Collect;
        private event TimerCallbackDelgate TimerCallbackEvent_Update;

        #endregion

        #region 更新
        [ObservableProperty]
        private bool isUpdating = false;

        public void StartUpdate()
        {
            if (!IsUpdating) 
            {
                ShowMessageEvent("Info: Start update-timer");
                // 防止GC回收非委托代码
                TimerCallbackEvent_Update = new TimerCallbackDelgate(TimerCallBackMethod_Update);
                GC.KeepAlive(TimerCallbackEvent_Update);
                // 启动定时器
                TimerId_Update = TimeSetEvent(UpdateInterval, Resolution, TimerCallbackEvent_Update, UIntPtr.Zero, 1);
            }
            if (DeweIsInit)
            {
                StartMeasureDewe();
            }

            IsUpdating = true;
        }

        public void StopUpdate()
        {
            if (IsUpdating) 
            {
                ShowMessageEvent("Info: Stop update-timer");
                if (TimerId_Update != 0)
                {
                    TimeKillEvent(TimerId_Update);
                    TimerId_Update = 0;
                }
            }
            if (DeweIsMeasuring)
            {
                StopMeasureDewe();
            }

            IsUpdating = false;
        }

        private void TimerCallBackMethod_Update(uint id, uint msg, UIntPtr user, UIntPtr dw1, UIntPtr dw2)
        {
            UpdateCoord();
            UpdateSpInfo();
            UpdateProgInfo();
            UpdateModelInfo();
            UpdateRatesInfo();
            UpdateStatus();
        }

        private readonly object lockCoord = new object();
        private Task UpdateCoord()
        {
            lock (lockCoord)
            {
                objMachCoord = new VariantWrapper(coord);
                objAbsCoord = new VariantWrapper(coord);
                objRelCoord = new VariantWrapper(coord);
                jdMachMon.GetMachPosMultiAxisCSH(ref objMachCoord, ref objAbsCoord, ref objRelCoord);
                coord = (double[])objAbsCoord;

                X = Math.Round(coord[0], 4);
                Y = Math.Round(coord[1], 4);
                Z = Math.Round(coord[2], 4);
                A = Math.Round(coord[3], 4);
                C = Math.Round(coord[5], 4);
            }

            return Task.CompletedTask;
        }

        private readonly object lockSpInfo = new object();
        private Task UpdateSpInfo()
        {
            lock (lockSpInfo)
            {
                // 电流，转矩，功率
                objSpInfo = new VariantWrapper(spInfo);
                jdMachMon.GetSpindleInfoCSH(ref objSpInfo);
                spInfo = (double[])objSpInfo;

                Electricity = spInfo[0];
                Torque = spInfo[1];
                Power = spInfo[2];
            }

            return Task.CompletedTask;
        }

        private readonly object lockProgInfo = new object();
        private Task UpdateProgInfo()
        {
            lock (lockProgInfo)
            {
                // 当前执行的G代码
                jdMachMon.GetCurrLineText(ref currNcCode);
                OnPropertyChanged(nameof(CurrNcCode));

                // 当前执行的G代码行号
                //jdMachMon.GetCurLineNo(ref currNcCodeLineNo);
                //OnPropertyChanged(nameof(CurrNcCodeLineNo));
            }

            return Task.CompletedTask;
        }

        private readonly object lockModeInfo = new object();
        private Task UpdateModelInfo()
        {
            lock (lockModeInfo)
            {
                jdMachMon.GetBasicModalInfo(ref workpieceCoord, ref feed, ref rev, ref toolNo, ref machTime, ref currProgNo, ref currMainProgNo);
                OnPropertyChanged(nameof(Feed));
                OnPropertyChanged(nameof(Rev));
                OnPropertyChanged(nameof(ToolNo));
                OnPropertyChanged(nameof(MachTime));
            }

            return Task.CompletedTask;
        }

        private readonly object lockRatesInfo = new object();
        private Task UpdateRatesInfo()
        {
            lock(lockRatesInfo)
            {
                objRates = new VariantWrapper(ratesInfo);
                jdMachMon.GetRateCSH(ref objRates);
                ratesInfo = (int[])objRates;

                RevRate = ratesInfo[0];
                FeedRate = ratesInfo[1];
            }
            return Task.CompletedTask;
        }

        #endregion

        #region 收集
        [ObservableProperty]
        private string collectButtonIcon = ".\\Icons\\开始.png";
        [ObservableProperty]
        private string collectButtonText = "开始";
        [ObservableProperty]
        private bool shallCollectWithDewesoft = false;

        [ObservableProperty]
        private bool isCollecting = false;

        private bool CanCollect => IsConnected && !string.IsNullOrWhiteSpace(WorkPath);
        [RelayCommand(CanExecute = (nameof(CanCollect)))]
        private void Collect()
        {
            if (IsCollecting) 
            {
                StopCollect();
                StartUpdate();
                Save();
                CollectButtonIcon = ".\\Icons\\开始.png";
                CollectButtonText = "开始";
            }
            else
            {
                StopUpdate();
                StartCollect();
                CollectButtonIcon = ".\\Icons\\停止.png";
                CollectButtonText = "停止";
            }
        }

        private void StartCollect()
        {
            TimeStamps.Clear();
            Coords.Clear();
            SpInfos.Clear();

            // 防止GC回收定时触发的非委托代码
            TimerCallbackEvent_Collect = new TimerCallbackDelgate(TimerCallBackMethod_Collect);
            GC.KeepAlive(TimerCallbackEvent_Collect);

            // 启动计时器
            Stopwatch.Start();

            // 启动定时器
            ShowMessageEvent("Info: Start collect-timer");
            TimerId_Collect = TimeSetEvent(CollectInterval, Resolution, TimerCallbackEvent_Collect, UIntPtr.Zero, 1);

            if (DeweIsInit) StartStoreDewe();

            IsCollecting = true;
        }

        private void StopCollect()
        {
            // 停止定时器
            if (TimerId_Collect != 0)
            {
                TimeKillEvent(TimerId_Collect);
                TimerId_Collect = 0;
            }

            // 停止计时器
            ShowMessageEvent("Info: Stop collect-timer");
            Stopwatch.Stop();

            if (DeweIsStoring) StopStoreDewe();

            IsCollecting = false;
        }

        // 回调函数
        private void TimerCallBackMethod_Collect(uint id, uint msg, UIntPtr user, UIntPtr dw1, UIntPtr dw2)
        {
            Task[] tasks = new Task[5]{
                CollectCoord(),
                CollectSpInfo(),
                CollectTimeStamp(),
                ShallCollectNcCode ? CollectNcCode() : Task.CompletedTask,
                ShallCollectNcCodeLineNo ? CollectNcCodeLineNo() : Task.CompletedTask
            };

            Task.WaitAll(tasks);
        }

        private Task CollectCoord()
        {
            objMachCoord = new VariantWrapper(coord);
            objAbsCoord = new VariantWrapper(coord);
            objRelCoord = new VariantWrapper(coord);
            jdMachMon.GetMachPosMultiAxisCSH(ref objMachCoord, ref objAbsCoord, ref objRelCoord);
            Coords.Add((double[])objAbsCoord);

            return Task.CompletedTask;
        }

        private Task CollectSpInfo()
        {
            objSpInfo = new VariantWrapper(spInfo);
            jdMachMon.GetSpindleInfoCSH(ref objSpInfo);
            SpInfos.Add((double[])objSpInfo);

            return Task.CompletedTask;
        }

        private Task CollectTimeStamp()
        {
            TimeStamps.Add(Stopwatch.ElapsedMilliseconds);

            return Task.CompletedTask;
        }

        private Task CollectNcCode()
        {
            jdMachMon.GetCurrLineText(ref currNcCode);
            CodeLines.Add(currNcCode);

            return Task.CompletedTask;
        }

        private Task CollectNcCodeLineNo()
        {
            jdMachMon.GetCurLineNo(ref currNcCodeLineNo);
            CodeLineNos.Add(currNcCodeLineNo);

            return Task.CompletedTask;
        }

        #endregion

        #region 存储
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CollectCommand))]
        [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
        private string workPath = string.Empty;

        [RelayCommand]
        public void ChooseWorkPath()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "请选择工作路径"
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {            
                WorkPath = dialog.FileName;
            }
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(WorkPath);
        [RelayCommand(CanExecute = (nameof(CanSave)))]
        public void Save()
        {
            ShowMessageEvent("Save collected data");

            Task[] tasks = new Task[5]
            {
                SaveCoords(),
                SaveSpInfos(),
                SaveTimeStamps(),
                ShallCollectNcCode ? SaveNcCodeLines() : Task.CompletedTask,
                ShallCollectNcCode ? SaveNcCodeLineNos() : Task.CompletedTask
            };

            Task.WaitAll(tasks);
        }

        public Task SaveTimeStamps()
        {
            if (TimeStamps.Count > 0)
            {
                ShowMessageEvent($"Info: Save TimeStamp array as text file to {WorkPath}\\time.txt");
                using StreamWriter writer = new StreamWriter($"{WorkPath}\\time.txt");
                for (int i = 0; i < TimeStamps.Count; ++i)
                {
                    writer.WriteLine(TimeStamps[i]);
                }
                TimeStamps.Clear();
            }
            return Task.CompletedTask;
        }

        public Task SaveCoords()
        {
            if (Coords.Count > 0)
            {
                Parallel.For(0, coord.Length, j =>
                {
                    ShowMessageEvent($"Info: Save coordinate array as text file to {WorkPath}\\coordinate_{j}.txt");
                    using StreamWriter writer = new StreamWriter($"{WorkPath}\\coordinate_{j}.txt");
                    for (int i = 0; i < Coords.Count; ++i)
                    {
                        writer.WriteLine(Coords[i][j]);
                    }
                });
                Coords.Clear();
            }
            return Task.CompletedTask;
        }

        public Task SaveSpInfos()
        {
            if (SpInfos.Count > 0)
            {
                Parallel.For(0, spInfo.Length, j =>
                {
                    ShowMessageEvent($"Info: Save spindle info array as text file to {WorkPath}\\spindle_{j}.txt");
                    using StreamWriter writer = new StreamWriter($"{WorkPath}\\spindle_{j}.txt");
                    for (int i = 0; i < SpInfos.Count; ++i)
                    {
                        writer.WriteLine(SpInfos[i][j]);
                    }
                });
                SpInfos.Clear();
            }
            return Task.CompletedTask;
        }

        public Task SaveNcCodeLines()
        {
            if (CodeLines.Count > 0)
            {
                ShowMessageEvent($"Info: Save NC code as text file to {WorkPath}\\code_lines.txt");
                using StreamWriter writer = new StreamWriter($"{WorkPath}\\code_lines.txt");
                for (int i = 0; i < CodeLines.Count; i++) 
                {
                    writer.Write(CodeLines[i]);
                }
            }
            return Task.CompletedTask;
        }

        public Task SaveNcCodeLineNos()
        {
            if (CodeLineNos.Count > 0)
            {
                ShowMessageEvent($"Info: Save NC code as text file to {WorkPath}\\code_Nos.txt");
                using StreamWriter writer = new StreamWriter($"{WorkPath}\\code_Nos.txt");
                for (int i = 0; i < CodeLineNos.Count; i++)
                {
                    writer.WriteLine(CodeLineNos[i]);
                }
            }
            return Task.CompletedTask;
        }

        #endregion
    }
}
