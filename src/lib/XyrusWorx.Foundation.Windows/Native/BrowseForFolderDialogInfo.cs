using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Native
{
	[PublicAPI]
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
	[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
	struct BrowseForFolderDialogInfo
	{
		public IntPtr hwndOwner;
		public IntPtr pidlRoot;
		public IntPtr pszDisplayName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string lpszTitle;
		public int ulFlags;
		[MarshalAs(UnmanagedType.FunctionPtr)]
		public BrowseForFolderCallback lpfn;
		public IntPtr lParam;
		public int iImage;
	}
}