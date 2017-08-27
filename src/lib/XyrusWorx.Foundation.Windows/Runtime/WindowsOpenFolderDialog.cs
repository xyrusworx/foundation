using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using JetBrains.Annotations;
using XyrusWorx.Runtime;
using XyrusWorx.Windows.Native;

namespace XyrusWorx.Windows.Runtime
{
	[PublicAPI]
	public class WindowsOpenFolderDialog : IOpenFolderDialog, IAsyncOpenFolderDialog
	{
		[DllImport("user32.dll")]  static extern IntPtr GetActiveWindow();
		[DllImport("shell32.dll")] static extern int SHGetMalloc(out IMalloc ppMalloc);
		[DllImport("shell32.dll")] static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, int nFolder, out IntPtr ppidl);
		[DllImport("shell32.dll")] static extern int SHGetPathFromIDList(IntPtr pidl, StringBuilder path);
		[DllImport("shell32.dll", CharSet = CharSet.Auto)] static extern IntPtr SHBrowseForFolder(ref BrowseForFolderDialogInfo bi);
		[DllImport("user32.dll", CharSet = CharSet.Auto)] static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		private static readonly int mMaxPathLength = 260;

		private IApplicationHost mApplication;

		private int mPublicOptions = (int)BrowseForFolderStyles.RestrictToFilesystem | (int)BrowseForFolderStyles.RestrictToDomain;
		private int mPrivateOptions = (int)BrowseForFolderStyles.NewDialogStyle;

		private Environment.SpecialFolder mRoot = Environment.SpecialFolder.Desktop;

		private string mPrompt;
		private object mOwner;
		private string mPath;

		public WindowsOpenFolderDialog()
		{
		}
		public WindowsOpenFolderDialog([NotNull] IApplicationHost application) : this()
		{
			if (application == null)
			{
				throw new ArgumentNullException(nameof(application));
			}

			mApplication = application;
		}

		public IOpenFolderDialog Prompt(string prompt)
		{
			mPrompt = prompt.NormalizeNull();
			return this;
		}
		public IOpenFolderDialog InitialSelection(string path)
		{
			mPath = path.NormalizeNull();
			return this;
		}
		public IOpenFolderDialog RootFolder(Environment.SpecialFolder specialFolder)
		{
			mRoot = specialFolder;
			return this;
		}
		public IOpenFolderDialog Owner(object view)
		{
			mOwner = view;
			return this;
		}

		public Result<string> Ask()
		{
			IntPtr pidlRoot;
			IntPtr hWndOwner;

			if (mOwner is Window window)
			{
				var iw = new WindowInteropHelper(window);
				hWndOwner = iw.Handle;
			}
			else if (mOwner is IWin32Window win32Window)
			{
				hWndOwner = win32Window.Handle;
			}
			else
			{
				hWndOwner = GetActiveWindow();
			}

			SHGetSpecialFolderLocation(hWndOwner, (int)mRoot, out pidlRoot);

			if (pidlRoot == IntPtr.Zero)
			{
				return Result.CreateError<Result<string>>("Error reading the root directory of the user profile. Perhaps there are no sufficient permissions to perform this action or the user profile is corrupted.");
			}

			var mergedOptions = mPublicOptions | mPrivateOptions;
			if ((mergedOptions & (int)BrowseForFolderStyles.NewDialogStyle) != 0)
			{
				if (System.Threading.ApartmentState.MTA == System.Windows.Forms.Application.OleRequired())
				{
					mergedOptions = mergedOptions & ~(int)BrowseForFolderStyles.NewDialogStyle;
				}
			}

			var pidlRet = IntPtr.Zero;
			try
			{
				var bi = new BrowseForFolderDialogInfo();
				var buffer = Marshal.AllocHGlobal(mMaxPathLength);

				bi.pidlRoot = pidlRoot;
				bi.hwndOwner = hWndOwner;
				bi.pszDisplayName = buffer;
				bi.lpszTitle = mPrompt;
				bi.ulFlags = mergedOptions;
				bi.lpfn = BrowseDialogCallback;

				pidlRet = SHBrowseForFolder(ref bi);
				Marshal.FreeHGlobal(buffer);

				if (pidlRet == IntPtr.Zero)
				{
					return Result.CreateError<Result<string>>(new OperationCanceledException());
				}

				var sb = new StringBuilder(mMaxPathLength);
				if (0 == SHGetPathFromIDList(pidlRet, sb))
				{
					return Result.CreateError<Result<string>>(new OperationCanceledException());
				}

				mPath = sb.ToString();
			}
			finally
			{
				IMalloc malloc;
				SHGetMalloc(out malloc);
				malloc.Free(pidlRoot);

				if (pidlRet != IntPtr.Zero)
				{
					malloc.Free(pidlRet);
				}
			}

			return new Result<string>(mPath);
		}
		async Task<Result<string>> IAsyncOpenFolderDialog.Ask()
		{
			if (mApplication != null)
			{
				return await mApplication.ExecuteAsync(Ask);
			}

			return await Task.Run(() => Ask());
		}

		IAsyncOpenFolderDialog IOpenFolderDialog.Async => this;

		private int BrowseDialogCallback(IntPtr hwnd, uint umsg, IntPtr lparam, IntPtr lpdata)
		{
			if (umsg == 1)
			{
				var buffer = Marshal.StringToHGlobalAuto(mPath);
				try
				{
					SendMessage(hwnd, 0x400 + 103, new IntPtr(1), buffer);
				}
				finally
				{
					Marshal.FreeHGlobal(buffer);
				}

				return 1;
			}

			return 0;
		}
	}
}