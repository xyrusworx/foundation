using System;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public sealed class MouseHookEventArgs : EventArgs
	{
		public MouseHookEventArgs(int x, int y, MouseButtons button, float delta)
		{
			Button = button;
			X = x;
			Y = y;
			Delta = delta;
		}

		public int X
		{
			get;
		}
		public int Y
		{
			get;
		}
		public float Delta
		{
			get;
		}
		public MouseButtons Button
		{
			get;
		}
	}
}