using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Native 
{
	[PublicAPI]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000002-0000-0000-C000-000000000046")]
	public interface IMalloc
	{
		[MethodImpl(MethodImplOptions.PreserveSig)]
		IntPtr Alloc([In] int cb);

		[MethodImpl(MethodImplOptions.PreserveSig)]
		IntPtr Realloc([In] IntPtr pv, [In] int cb);

		[MethodImpl(MethodImplOptions.PreserveSig)]
		void Free([In] IntPtr pv);

		[MethodImpl(MethodImplOptions.PreserveSig)]
		int GetSize([In] IntPtr pv);

		[MethodImpl(MethodImplOptions.PreserveSig)]
		int DidAlloc(IntPtr pv);

		[MethodImpl(MethodImplOptions.PreserveSig)]
		void HeapMinimize();
	}
}