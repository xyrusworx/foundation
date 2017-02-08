using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public class ApplicationExecutionContext
	{
		internal ApplicationExecutionContext()
		{
			
		}

		public string OperatingSystem => RuntimeInformation.OSDescription;
		public string RuntimeEnvironment => RuntimeInformation.FrameworkDescription;

		public bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		public bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		public bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		public bool IsUnix => IsLinux || IsMacOS;

		public bool Is32BitProcess => RuntimeInformation.ProcessArchitecture == Architecture.X86 || RuntimeInformation.ProcessArchitecture == Architecture.Arm;
		public bool Is64BitProcess => RuntimeInformation.ProcessArchitecture == Architecture.X64 || RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

		public bool IsX86Platform => RuntimeInformation.OSArchitecture == Architecture.X86;
		public bool IsX64Platform => RuntimeInformation.OSArchitecture == Architecture.X64;
		public bool IsArm32Platform => RuntimeInformation.OSArchitecture == Architecture.Arm;
		public bool IsArm64Platform => RuntimeInformation.OSArchitecture == Architecture.Arm64;

		public bool Is32BitPlatform => IsX86Platform || IsArm32Platform;
		public bool Is64BitPlatform => IsX64Platform || IsArm64Platform;

		public bool IsIntelPlatform => RuntimeInformation.OSArchitecture == Architecture.X86 || RuntimeInformation.OSArchitecture == Architecture.X64;
		public bool IsArmPlatform => RuntimeInformation.OSArchitecture == Architecture.Arm || RuntimeInformation.OSArchitecture == Architecture.Arm64;

		internal StringKey GetMachineDataDirectoryName(params string[] @namespace)
		{
			if (IsWindows)
			{
				var appData = Path.GetFullPath(Path.Combine(Environment.ExpandEnvironmentVariables("%programdata%")));
				var fullNamespace = new[] { appData }.Concat(@namespace).ToArray();
				var path = Path.Combine(fullNamespace);

				return new StringKey(path);
			}

			if (IsLinux)
			{
				var fullNamespace = new[] { "/var/programData" }.Concat(@namespace).ToArray();
				var path = Path.Combine(fullNamespace);

				return new StringKey(path);
			}

			throw new NotSupportedException("The current platform is not supported.");
		}
		internal StringKey GetUserDataDirectoryName(params string[] @namespace)
		{
			if (IsWindows)
			{
				var appData = Path.GetFullPath(Path.Combine(Environment.ExpandEnvironmentVariables("%appdata%"), ".."));
				var fullNamespace = new[] { appData }.Concat(@namespace).ToArray();
				var path = Path.Combine(fullNamespace);

				return new StringKey(path);
			}

			if (IsLinux)
			{
				var fullNamespace = new[] { Environment.GetEnvironmentVariable("HOME"), ".appData" }.Concat(@namespace).ToArray();
				var path = Path.Combine(fullNamespace);

				return new StringKey(path);
			}

			throw new NotSupportedException("The current platform is not supported.");
		}
	}
}