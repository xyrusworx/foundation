using System;
using System.Threading;
using JetBrains.Annotations;

namespace XyrusWorx.Runtime
{
	[PublicAPI]
	public abstract class ConsoleApplication : Application
	{
		public string ReadPassword()
		{
			if (Console.IsInputRedirected)
			{
				return null;
			}

			var result = "";
			ConsoleKeyInfo key;

			do
			{
				key = Console.ReadKey(true);

				if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
				{
					result += key.KeyChar;
					Console.Write(@"*");
				}
				else
				{
					if (key.Key == ConsoleKey.Backspace && result.Length > 0)
					{
						result = result.Substring(0, result.Length - 1);
						// ReSharper disable once LocalizableElement
						Console.Write("\b \b");
					}
				}
			}
			while (key.Key != ConsoleKey.Enter);

			return result.Trim('\0');
		}

		protected sealed override IResult InitializeApplication()
		{
			var result = InitializeOverride();
			if (result.HasError)
			{
				return result;
			}

			if (ShowBanner)
			{
				WriteBanner();
			}

			return Result.Success;
		}
		protected virtual IResult InitializeOverride()
		{
			return Result.Success;
		}

		public bool ShowBanner { get; set; } = true;
		public bool Interactive { get; set; } = true;

		[NotNull]
		public IScope WithColor(ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			return new ConsoleColorScope(foreground, background).Enter();
		}

		protected sealed override void Cleanup(bool wasCancelled)
		{
			try
			{
				CleanupOverride();
			}
			finally
			{
				if (Interactive)
				{
					WaitForKey();
				}
			}
		}
		protected virtual void CleanupOverride()
		{
		}

#if(!NO_NATIVE_BOOTSTRAPPER)
		[ContractAnnotation("=> halt")]
		protected void Terminate(int exitCode)
		{
			WaitForKey();
			Environment.Exit(exitCode);
		}
#endif

		private void WriteBanner()
		{
			if (Console.IsOutputRedirected)
			{
				return;
			}

			ConsoleColor emphasisColor;
			ConsoleColor dimColor;

			if (Context.IsWindows)
			{
				emphasisColor = ConsoleColor.White;
				dimColor = ConsoleColor.DarkGray;
			}
			else if (Context.IsLinux)
			{
				emphasisColor = Console.ForegroundColor;
				dimColor = ConsoleColor.Gray;
			}
			else
			{
				emphasisColor = Console.ForegroundColor;
				dimColor = Console.ForegroundColor;
			}

			using (new ConsoleColorScope(emphasisColor).Enter())
			{
				Console.Out.WriteLine(Metadata.ProductName);
			}

			using (new ConsoleColorScope(dimColor).Enter())
			{
				Console.Out.WriteLine(string.Join(Environment.NewLine, Metadata.LegalCopyright, string.Empty));
			}

			
		}
		private void WaitForKey()
		{
			if (Console.IsErrorRedirected || Console.IsOutputRedirected)
			{
				return;
			}

			Console.Out.WriteLine("Press any key to continue . . . ");

			WaitHandler.Wait(() => Console.KeyAvailable);

			Console.ReadKey(true);
		}
	}
}