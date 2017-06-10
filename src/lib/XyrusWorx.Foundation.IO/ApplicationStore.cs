using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;

namespace XyrusWorx.IO
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
			var productName = versionInfo.ProductName.NormalizeNull() ?? (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Name;

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