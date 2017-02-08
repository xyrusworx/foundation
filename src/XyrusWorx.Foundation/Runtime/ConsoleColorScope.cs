using System;
using System.Collections.Generic;

namespace XyrusWorx.Runtime
{
	sealed class ConsoleColorScope : Scope
	{
		private ConsoleColor? mForeground;
		private ConsoleColor? mBackground;

		private Stack<ColorInfo> mStack;

		public ConsoleColorScope(ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			mStack = new Stack<ColorInfo>();

			mForeground = foreground;
			mBackground = background;
		}

		protected override void EnteringOverride()
		{
			var ci = new ColorInfo
			{
				Foreground = Console.ForegroundColor,
				Background = Console.BackgroundColor
			};

			mStack.Push(ci);
		}
		protected override void EnteredOverride()
		{
			var ci = mStack.Peek();

			Console.ForegroundColor = mForeground ?? ci.Foreground;
			Console.BackgroundColor = mBackground ?? ci.Background;
		}
		protected override void LeftOverride()
		{
			var ci = mStack.Pop();

			Console.ForegroundColor = ci.Foreground;
			Console.BackgroundColor = ci.Background;
		}

		struct ColorInfo
		{
			public ConsoleColor Foreground;
			public ConsoleColor Background;
		}
	}
}