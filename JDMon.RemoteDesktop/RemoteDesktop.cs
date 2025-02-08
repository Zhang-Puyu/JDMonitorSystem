using System.Drawing;
using System.Windows.Forms;

namespace JDMon.RemoteDesktop
{
    public partial class RemoteDesktop : Form
    {
        public RemoteDesktop()
        {
            InitializeComponent();

            this.Width = 800;
            this.Height = 600;

            System.Timers.Timer t = new System.Timers.Timer(1000);   // 实例化Timer类，设置间隔时间为10000毫秒；   
            t.Elapsed += new System.Timers.ElapsedEventHandler(ThreadOut); // 到达时间的时候执行事件；   
            t.AutoReset = true;   // 设置是执行一次（false）还是一直执行(true)；   
            t.Enabled = true;
        }

        public void ThreadOut(object source, System.Timers.ElapsedEventArgs e)
        {
            RemoteDesktopData RDData = RemoteDesktopData.GetData();

            lock (RDData.lockObj)
            {
                Graphics grp = this.CreateGraphics();
                Bitmap bmp = new Bitmap(800, 600);

                for (int height = 0; height < 600; height++)
                    for (int width = 0; width < 800; width++)
                    {
                        int index = height * 800 + width;

                        bmp.SetPixel(width, height, Color.FromArgb(RDData.bmpData[index * 4 + 2], RDData.bmpData[index * 4 + 1], RDData.bmpData[index * 4]));
                    }

                grp.DrawImage(bmp, new Point(0, 0));
                RDData.bUpdate = true;
            }

        }

        private void RemoteDesktop_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemoteDesktopData RDData = RemoteDesktopData.GetData();
            RDData.bViewing = false;
        }
    }
}
