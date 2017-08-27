using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using Microsoft.Win32;
using XyrusWorx.Collections;
using XyrusWorx.Runtime;
using Application = System.Windows.Application;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public class WindowsOpenFileDialog : IOpenFileDialog, IAsyncOpenFileDialog
	{
		private IApplicationHost mApplication;

		private readonly Dictionary<string, string> mFormats;

		private string mTitle;
		private string mDirectory;
		private object mOwner;

		public WindowsOpenFileDialog()
		{
			mFormats = new Dictionary<string, string>();
		}
		public WindowsOpenFileDialog([NotNull] IApplicationHost application) : this()
		{
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			mApplication = application;
		}

		public IOpenFileDialog Title(string title)
		{
			mTitle = title;
			return this;
		}
		public IOpenFileDialog InitialDirectory(string path)
		{
			mDirectory = path;
			return this;
		}
		public IOpenFileDialog Format(string pattern, string displayName)
		{
			if (pattern.NormalizeNull() == null)
			{
				throw new ArgumentNullException(pattern);
			}

			pattern = pattern.ToLower();
			mFormats.AddOrUpdate(pattern, displayName);

			return this;
		}
		public IOpenFileDialog Owner(object view)
		{
			mOwner = view;
			return this;
		}

		public Result<string> Ask()
		{
			var dialog = new OpenFileDialog();

			dialog.Title = mTitle ?? "Open...";
			dialog.InitialDirectory = mDirectory;
			dialog.Filter = string.Join("|", mFormats.Select(x => $"{x.Value.NormalizeNull().TryTransform(y => $"{y} ({x.Key})") ?? x.Key}|{x.Key}"));
			dialog.Filter += (string.IsNullOrEmpty(dialog.Filter) ? "" : "|") + "All files (*.*)|*.*";

			var result = mOwner is Window w ? dialog.ShowDialog(w) : dialog.ShowDialog();
			if (!result.HasValue || !result.Value)
			{
				return Result.CreateError<Result<string>>(new OperationCanceledException());
			}

			return new Result<string>(dialog.FileName);

		}
		async Task<Result<string>> IAsyncOpenFileDialog.Ask()
		{
			var dispatcher = mApplication?.GetDispatcher() ?? Application.Current?.Dispatcher;
			if (dispatcher != null)
			{
				return await dispatcher.InvokeAsync(Ask);
			}

			return await Task.Run(() => Ask());
		}

		IAsyncOpenFileDialog IOpenFileDialog.Async => this;
	}
}