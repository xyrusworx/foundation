using System.Windows.Forms;
using JetBrains.Annotations;
using XyrusWorx.Windows.Native;

namespace XyrusWorx.Windows.Input
{
	[PublicAPI]
	public static class KeyboardInputManager
	{
		static KeyboardInputManager()
		{
			InputHooks.KeyDown += (o, e) => KeyDown?.Invoke(new KeyboardHookEventArgs((int)e.KeyData));
			InputHooks.KeyUp += (o, e) => KeyUp?.Invoke(new KeyboardHookEventArgs((int)e.KeyData));
			InputHooks.KeyPress += (o, e) => KeyPress?.Invoke(new KeyPressHookEventArgs(e.KeyChar));
		}

		public static event KeyboardHookEventHandler KeyUp;
		public static event KeyboardHookEventHandler KeyDown;
		public static event KeyPressHookEventHandler KeyPress;

		public static bool GetKeyState(Keys vk) => InputHooks.GetKeyState((int)vk) < 0;
	}
}