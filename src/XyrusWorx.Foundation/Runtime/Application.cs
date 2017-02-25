using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using XyrusWorx.Diagnostics;
using XyrusWorx.IO;
using XyrusWorx.Threading;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public abstract class Application : Operation
	{
		private static Application mCurrent;
		private CommandLineProcessor mCommandLine;

		protected Application()
		{
			if (mCurrent != null)
			{
				throw new InvalidOperationException("Only one application per domain is allowed.");
			}

#if (NO_NATIVE_BOOTSTRAPPER)
			Assembly assembly = null;
#else
			var assembly = Assembly.GetEntryAssembly() ?? GetType().GetTypeInfo().Assembly;
#endif

			Log = new NullLogWriter();
			CommandLine = new CommandLineKeyValueStore();
			Metadata = new AssemblyMetadata(assembly);
			Context = new ApplicationExecutionContext();

			mCommandLine = new CommandLineProcessor(GetType());

			var productKey = Metadata.FileName.NormalizeNull().TryTransform(Path.GetFileNameWithoutExtension);

#if (NO_NATIVE_BOOTSTRAPPER)
			var assemblyDir = Directory.GetCurrentDirectory();
#else
			var assemblyDir = (assembly.Location.TryTransform(Path.GetDirectoryName) ?? Directory.GetCurrentDirectory()).AsKey();
#endif

			var userDir = Context.GetUserDataDirectoryName("Local", Metadata.CompanyName.NormalizeNull() ?? "XW", productKey ?? ".Default");
			var machineDir = Context.GetMachineDataDirectoryName(Metadata.CompanyName.NormalizeNull() ?? "XW", productKey ?? ".Default");

			WorkingDirectory = new FileSystemStore(assemblyDir);
			UserDataDirectory = new FileSystemStore(userDir);
			MachineDataDirectory = new FileSystemStore(machineDir);

			Settings = new ConfigurationReaderChain();

			base.DispatchMode = OperationDispatchMode.Synchronous;
			mCurrent = this;
		}

		[CanBeNull]
		public static Application Current => mCurrent ?? (mCurrent = new GenericApplication());
		public sealed override OperationDispatchMode DispatchMode
		{
			get { return base.DispatchMode; }
			set { base.DispatchMode = value; }
		}

		[NotNull]
		public override string DisplayName => Metadata.ProductName ?? Metadata.AssemblyName ?? GetType().Name;

		[NotNull] public ApplicationExecutionContext Context { get; }

		[NotNull] public FileSystemStore WorkingDirectory { get; }
		[NotNull] public FileSystemStore UserDataDirectory { get; }
		[NotNull] public FileSystemStore MachineDataDirectory { get; }

		[NotNull] public AssemblyMetadata Metadata { get; }
		[NotNull] public LogWriter Log { get; }
		[NotNull] public CommandLineKeyValueStore CommandLine { get; }
		[NotNull] public ConfigurationReaderChain Settings { get; }

		[NotNull]
		public CommandLineProcessor GetCommandLineProcessor() => mCommandLine;

		protected sealed override IResult Initialize()
		{
			SetupApplication();
			mCommandLine.Read(CommandLine, this);

			return InitializeApplication();
		}

		protected virtual void SetupApplication() { }
		protected virtual IResult InitializeApplication() => Result.Success;

		protected void ForceDispatchMode(OperationDispatchMode mode) => base.DispatchMode = OperationDispatchMode.BackgroundThread;
	}
}