using System;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public sealed class KeyboardHookEventArgs : EventArgs
	{
		public KeyboardHookEventArgs(int vKeyFlags)
		{
			KeyFlags = (Keys)vKeyFlags;
		}
		public Keys KeyFlags
		{
			get;
		}

		public bool AltKey => ((KeyFlags & Keys.LMenu) != 0) || ((KeyFlags & Keys.RMenu) != 0);
		public bool ShiftKey => ((KeyFlags & Keys.LShiftKey) != 0) || ((KeyFlags & Keys.RShiftKey) != 0);
		public bool ControlKey => ((KeyFlags & Keys.LControlKey) != 0) || ((KeyFlags & Keys.RControlKey) != 0);
	}

}