using CommunityToolkit.Mvvm.Input;
using JDMon.RemoteDesktop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace JDMon.ViewModels
{
	public partial class MainViewModel
	{
		#region 远程桌面

		private readonly RemoteDesktopData RDData = RemoteDesktopData.GetData();

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
				Thread.Sleep((int)UpdateInterval);
			}
		}

		[RelayCommand]
		private void ShowRemoteDesktop()
		{
			if (jdMachMon.IsConnect() && !RDData.bViewing)
			{
				RDData.bViewing = true;
				Thread UpdateThread = new Thread(RemoteDesktopDataUpdate);
				UpdateThread.Start();

				RemoteDesktop.RemoteDesktop dsk = new RemoteDesktop.RemoteDesktop();
				dsk.Show();
			}
		}

		#endregion
	}
}
