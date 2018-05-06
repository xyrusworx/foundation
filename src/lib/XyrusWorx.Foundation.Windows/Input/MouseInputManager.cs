using JetBrains.Annotations;
using XyrusWorx.Windows.Native;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public static class MouseInputManager
	{
		static MouseInputManager()
		{
			InputHooks.MouseMove += (o, e) => MouseMove?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
			InputHooks.MouseClick += (o, e) => MouseClick?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
			InputHooks.MouseDown += (o, e) => MouseDown?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
			InputHooks.MouseUp += (o, e) => MouseUp?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
			InputHooks.MouseWheel += (o, e) => MouseWheel?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
			InputHooks.MouseDoubleClick += (o, e) => MouseDoubleClick?.Invoke(new MouseHookEventArgs(e.X, e.Y, (e.Button), e.Delta));
		}

		public static event MouseHookEventHandler MouseMove;
		public static event MouseHookEventHandler MouseClick;
		public static event MouseHookEventHandler MouseDown;
		public static event MouseHookEventHandler MouseUp;
		public static event MouseHookEventHandler MouseWheel;
		public static event MouseHookEventHandler MouseDoubleClick;

		public static System.Windows.Point GetPosition()
		{
			InputHooks.GetCursorPos(out var p);
			return new System.Windows.Point(p.X, p.Y);
		}
		public static void SetPosition(System.Windows.Point p)
		{
			InputHooks.SetCursorPos((int)p.X, (int)p.Y);
		}
	}
}