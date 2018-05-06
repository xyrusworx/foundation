using System;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input 
{
	[PublicAPI]
	public sealed class KeyPressHookEventArgs : EventArgs
	{
		public KeyPressHookEventArgs(char character)
		{
			KeyChar = character;
		}
		public char KeyChar
		{
			get;
		}
	}
}