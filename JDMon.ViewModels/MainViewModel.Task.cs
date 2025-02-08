using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
    {
        [ObservableProperty]
        private ObservableCollection<TreeNodeViewModel> rootNodes
            = new ObservableCollection<TreeNodeViewModel>();

        private readonly object lockTaskInfo = new object();
        [RelayCommand]
        private void UpdateTaskInfo()
        {
            lock (lockTaskInfo)
            {
                string FileName = "", TaskName = "", MainRunProg = "", MainProgs = "", SubProgs = "";
                if (jdMachMon.GetTask(ref FileName, ref TaskName, ref MainRunProg, ref MainProgs, ref SubProgs))
                {

                    TreeNodeViewModel MainProgNodes = new TreeNodeViewModel("主程序");
                    TreeNodeViewModel SubProgNodes = new TreeNodeViewModel("子程序");
                    RootNodes.Clear();
                    RootNodes.Add(MainProgNodes);
                    RootNodes.Add(SubProgNodes);

                    bool bEnd = false;
                    int startPos = 0;
                    if (MainProgs.Length > 0)
                    {
                        // 添加主程序节点
                        while (!bEnd)
                        {
                            int Pos = MainProgs.IndexOf(',', startPos);
                            if (Pos == -1)
                            {
                                bEnd = true;
                                Pos = MainProgs.Length;
                            }
                            string MainProg = MainProgs.Substring(startPos, Pos - startPos);
                            startPos = Pos + 1;
                            MainProgNodes.Add(new TreeNodeViewModel(MainProg));
                        }
                    }

                    bEnd = false;
                    startPos = 0;

                    if (SubProgs.Length > 0)
                    {
                        // 添加子程序节点
                        while (!bEnd)
                        {
                            int Pos = SubProgs.IndexOf(',', startPos);
                            if (Pos == -1)
                            {
                                bEnd = true;
                                Pos = SubProgs.Length;
                            }
                            string SubProg = SubProgs.Substring(startPos, Pos - startPos);
                            startPos = Pos + 1;
                            SubProgNodes.Add(new TreeNodeViewModel(SubProg));
                        }
                    }
                }
            }
        }
    }
}
