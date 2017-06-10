using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public sealed class ApplicationStore
	{
		private const string mApplicationGroupKey = @"XyrusWorx";

		private static FileSystemStore mProgramData;
		private static FileSystemStore mLocalAppData;
		private static FileSystemStore mRoamingAppData;

		static ApplicationStore()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

			// ReSharper disable once AssignNullToNotNullAttribute
			var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			var productName = versionInfo.ProductName.NormalizeNull() ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Name.NormalizeNull() ?? "Common";

			foreach (var c in Path.GetInvalidFileNameChars())
			{
				productName = productName.Replace($"{c}", "");
			}

			if (string.IsNullOrEmpty(productName))
			{
				productName = "_";
			}

			mProgramData = new FileSystemStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), mApplicationGroupKey, productName));
			mLocalAppData = new FileSystemStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), mApplicationGroupKey, productName));
			mRoamingAppData = new FileSystemStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), mApplicationGroupKey, productName));
		}

		[NotNull]
		public static FileSystemStore ProgramData => mProgramData;

		[NotNull]
		public static FileSystemStore LocalAppData => mLocalAppData;

		[NotNull]
		public static FileSystemStore RoamingAppData => mRoamingAppData;
	}
}