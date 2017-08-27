using JetBrains.Annotations;
using System;
using System.Linq;
using XyrusWorx.Diagnostics;

namespace XyrusWorx.Runtime
{

	[PublicAPI]
	public abstract class ConsoleApplication : Application
	{
		public static int GetAvailableBufferWidth()
		{
			try
			{
				return Console.BufferWidth;
			}
			catch
			{
				return 120;
			}
		}
		public static int GetAvailableWindowWidth()
		{
			try
			{
				return Console.WindowWidth;
			}
			catch
			{
				return 120;
			}
		}
		
		public static string ReadPassword()
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
			Log.LinkedDispatchers.Add(new ConsoleWriter());
			Log.Verbosity = Verbosity;

			var result = InitializeOverride();
			if (result.HasError)
			{
				return result;
			}

			if (!NoLogo)
			{
				WriteBanner();
			}

			return Result.Success;
		}
		protected virtual IResult InitializeOverride() => Result.Success;

		[CommandLineProperty("verbosity")]
		public LogVerbosity Verbosity { get; set; }

		[CommandLineSwitch("nologo")]
		public bool NoLogo { get; set; }

		[CommandLineSwitch("confirm-exit")]
		public bool ConfirmExit { get; set; }

		[NotNull]
		public static IScope WithColor(ConsoleColor? foreground = null, ConsoleColor? background = null) 
			=> new ConsoleColorScope(foreground, background).Enter();

		public void WriteHelp(ConsoleColor emphasisColor = ConsoleColor.White)
		{
			var doc = new CommandLineDocumentation();
			var processor = GetCommandLineProcessor();

			processor.WriteDocumentation(doc);

			WriteHelp(doc, emphasisColor);
		}
		public void WriteHelp([NotNull] CommandLineDocumentation documentation, ConsoleColor emphasisColor = ConsoleColor.White)
		{
			if (documentation == null)
			{
				throw new ArgumentNullException(nameof(documentation));
			}

			var writer = new ConsoleWriter { IncludeScope = false };

			var names = documentation.GetSortedTokens().OfType<CommandLineNamedTokenDocumentation>().ToArray();
			var shortDescriptions = names.Where(x => !string.IsNullOrWhiteSpace(x.ShortDescription)).ToArray();

			using (WithColor(emphasisColor))
			{
				Console.WriteLine("Usage");
			}

			var bufferWidth = GetAvailableBufferWidth();

			Console.WriteLine($"   {Metadata.ModuleName} {documentation}".WordWrap(bufferWidth, new string(' ', 3), ""));

			if (shortDescriptions.Any())
			{
				using (WithColor(emphasisColor))
				{
					Console.WriteLine();
					Console.WriteLine("Available command line parameters");
					Console.WriteLine();
				}

				var padWidth = shortDescriptions.Max(x => GetTokenLabel(x).Length) + 2;

				foreach (var token in shortDescriptions)
				{
					var label = GetTokenLabel(token).PadRight(padWidth);
					writer.WriteInformation($"   {label}{token.ShortDescription}".WordWrap(bufferWidth, new string(' ', padWidth + 3), ""));
				}
			}

			var descriptions = names.Where(x => !string.IsNullOrWhiteSpace(x.Description)).ToArray();

			if (descriptions.Any())
			{
				using (WithColor(emphasisColor))
				{
					Console.WriteLine();
					Console.WriteLine("Parameter descriptions");
					Console.WriteLine();
				}

				foreach (var token in descriptions)
				{
					var label = token.Name;

					using (WithColor(emphasisColor))
					{
						Console.WriteLine($"   {label}");
					}

					writer.WriteInformation(token.Description.WordWrap(bufferWidth, "      "));
					Console.WriteLine();
				}
			}
		}
		
		public void WaitForKey()
		{
			if (Console.IsErrorRedirected || Console.IsOutputRedirected || Console.IsInputRedirected)
			{
				return;
			}

			WaitHandler.Wait(() => Console.KeyAvailable);
			Console.ReadKey(true);
		}
		public void WaitForKey(ConsoleKey key, ConsoleModifiers modifiers = default(ConsoleModifiers))
		{
			if (Console.IsErrorRedirected || Console.IsOutputRedirected || Console.IsInputRedirected)
			{
				return;
			}

			WaitHandler.Wait(
				() =>
				{
					if (!Console.KeyAvailable)
					{
						return false;
					}
					
					var interceptedKey = Console.ReadKey(true);
					return 
						interceptedKey.Key == key && 
						interceptedKey.Modifiers == modifiers;
				});
		}

		protected sealed override void Cleanup(bool wasCancelled)
		{
			try
			{
				CleanupOverride();
			}
			finally
			{
				QueryUserConfirmationIfConfigured();
			}
		}

		protected virtual void CleanupOverride()
		{
			if (ExecutionResult.HasError)
			{
				Log.Write(ExecutionResult);
			}
		}

#if(!NO_NATIVE_BOOTSTRAPPER)
		[ContractAnnotation("=> halt")]
		protected void Terminate(int exitCode)
		{
			if (ConfirmExit)
			{
				Console.Out.WriteLine("Press any key to continue . . . ");
				WaitForKey();				
			}
			Environment.Exit(exitCode);
		}
#endif

		private void WriteBanner()
		{
			if (Console.IsOutputRedirected)
			{
				return;
			}

			var emphasisColor = ConsoleColor.White;
			var dimColor = ConsoleColor.DarkGray;

			using (new ConsoleColorScope(emphasisColor).Enter())
			{
				Console.Out.WriteLine(Metadata.ProductName);
			}

			using (new ConsoleColorScope(dimColor).Enter())
			{
				Console.Out.WriteLine(string.Join(Environment.NewLine, Metadata.LegalCopyright, string.Empty));
			}

			
		}
		private string GetTokenLabel(CommandLineNamedTokenDocumentation token)
		{
			if (string.IsNullOrWhiteSpace(token.ShortName))
			{
				return $"--{token.Name}";
			}

			return $"-{token.ShortName} / --{token.Name}";
		}
		private void QueryUserConfirmationIfConfigured()
		{
			var hasRedirections = Console.IsInputRedirected || Console.IsOutputRedirected || Console.IsErrorRedirected;
			if (ConfirmExit && !hasRedirections)
			{
				Console.Out.WriteLine("Press any key to continue . . . ");
				WaitForKey();
			}
		}
	}
}