using JetBrains.Annotations;
using System;
using System.Linq;
using XyrusWorx.Diagnostics;

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

			if (!NoLogo)
			{
				WriteBanner();
			}

			return Result.Success;
		}
		protected virtual IResult InitializeOverride()
		{
			return Result.Success;
		}

		[CommandLineSwitch("nologo")]
		public bool NoLogo { get; set; }

		[CommandLineSwitch("confirm-exit")]
		public bool ConfirmExit { get; set; }

		[NotNull]
		public IScope WithColor(ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			return new ConsoleColorScope(foreground, background).Enter();
		}

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

			var writer = new LightConsoleWriter { IncludeScope = false };

			var names = documentation.GetSortedTokens().OfType<CommandLineNamedTokenDocumentation>().ToArray();
			var shortDescriptions = names.Where(x => !string.IsNullOrWhiteSpace(x.ShortDescription)).ToArray();

			using (WithColor(emphasisColor))
			{
				Console.WriteLine("Usage");
			}

			Console.WriteLine($"   {Metadata.ModuleName} {documentation}".WordWrap(writer.SuggestedMaxLineLength, new string(' ', 3), ""));

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
					writer.WriteInformation($"   {label}{token.ShortDescription}".WordWrap(writer.SuggestedMaxLineLength, new string(' ', padWidth + 3), ""));
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

					writer.WriteInformation(token.Description.WordWrap(writer.SuggestedMaxLineLength, "      "));
					Console.WriteLine();
				}
			}
		}

		protected sealed override void Cleanup(bool wasCancelled)
		{
			try
			{
				CleanupOverride();
			}
			finally
			{
				if (ConfirmExit)
				{
					WaitForKey();
				}
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

		private string GetTokenLabel(CommandLineNamedTokenDocumentation token)
		{
			if (string.IsNullOrWhiteSpace(token.ShortName))
			{
				return $"--{token.Name}";
			}

			return $"-{token.ShortName} / --{token.Name}";
		}
	}
}