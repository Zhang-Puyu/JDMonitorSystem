
namespace JDMon.RemoteDesktop
{
    public class RemoteDesktopData
    {
        static RemoteDesktopData SingleInstance = new RemoteDesktopData();

        public static RemoteDesktopData GetData()
        {
            return SingleInstance;
        }

        public object lockObj = new object();

        public int len = 0;
        public bool bViewing = false;
        public bool bUpdate = true;

        public byte[] bmpData = new byte[2100000];
        public byte[] bmpInfo = new byte[32];
    }
}
