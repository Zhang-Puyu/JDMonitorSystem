using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using AxNCMONIOEXLib;

namespace JDMon.RemoteDesktop.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RemoteDesktopData RDData;
        private readonly AxNcMonIOEx jdMachMon;

        public MainWindow()
        {
            InitializeComponent();
            jdMachMon = this.jdHostControl.JDMonHost;
            RDData = RemoteDesktopData.GetData();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            jdMachMon.SetRPCTimeout(1000);
            jdMachMon.SetConnectionTimeout(1000);
            if (jdMachMon.ConnectJDMach(this.textBox.Text))
            {
                RDData.bViewing = true;
                Thread UpdateThread = new Thread(RemoteDesktopDataUpdate);
                UpdateThread.Start();

                RemoteDesktop dsk = new RemoteDesktop();
                dsk.FormClosed += (s, args) =>
                {
                    RDData.bViewing = false;
                    UpdateThread.Abort();
                    this.Close();
                };
                this.Hide();
                dsk.Show(); 
            }
            else
            {
                MessageBox.Show("连接失败", "错误", MessageBoxButton.OK);
            }
        }

        private void RemoteDesktopDataUpdate()
        {
            while (RDData.bViewing)
            {
                lock (RDData.lockObj)
                {
                    if (jdMachMon.IsConnect())
                    {
                        object bmpInfoObj = new VariantWrapper(RDData.bmpInfo);
                        object bmpDataObj = new VariantWrapper(RDData.bmpData);

                        jdMachMon.GetMachScreen(ref RDData.len, ref bmpInfoObj, ref bmpDataObj);

                        RDData.bmpInfo = (byte[])bmpInfoObj;
                        RDData.bmpData = (byte[])bmpDataObj;
                    }
                }
                Thread.Sleep(50);
            }
        }
    }
}
