using System;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JDMon.ViewModels
{
    public partial class MainViewModel
	{

		[DllImport("Kernel32.dll")]
		public static extern Boolean AllocConsole();
		[DllImport("Kernel32.dll")]
		public static extern Boolean FreeConsole();

		private bool ConsoleIsOpen = true;

		[RelayCommand]
		private void DebugWithConsole()
		{
			//if (ConsoleIsOpen)
			//{
			//	ShowMessageEvent -= Shell.WriteLine;
			//	ConsoleIsOpen = true;
			//	//FreeConsole();
			//}
			//else
			//{
			//	//AllocConsole();
			//	ConsoleIsOpen = false;
			//	ShowMessageEvent += Shell.WriteLine;
			//}
		}
    }

	static class Shell
	{
		/// <summary>
		/// 输出调用方法
		/// </summary>
		/// <param name="format">格式</param>
		/// <param name="args">需要拼接的参数</param>
		public static void WriteLine(string format, params object[] args)
		{
			WriteLine(string.Format(format, args));		//将指定字符串中的格式项替换为指定数组中相应对象的字符串表示形式。
		}
		/// <summary>
		/// 输出方法
		/// </summary>
		/// <param name="output">输出的文本</param>
		public static void WriteLine(string output)
		{
			Console.ForegroundColor = GetConsoleColor(output);//设置颜色
			Console.WriteLine(@"[{0:G}]{1}", DateTimeOffset.Now, output);		//输出到控制台
		}
		/// <summary>
		/// 根据类型区分输出颜色
		/// </summary>
		/// <param name="output">需要输出的字符串</param>
		/// <returns></returns>
		static ConsoleColor GetConsoleColor(string output)
		{
			if (output.StartsWith("Warn")) return ConsoleColor.Yellow;		// 根据前缀返回颜色
			if (output.StartsWith("Error")) return ConsoleColor.Red;
			if (output.StartsWith("Info")) return ConsoleColor.Green;

			return ConsoleColor.Gray;
		}
	}
}
